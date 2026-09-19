# Лабораторная работа №4
# Масштабирование чтения PostgreSQL: Primary + Replica

> Сырые результаты: `results/01_setup_primary.txt` … `results/06_lag_check.txt`
> SQL: `sql/01_setup_primary.sql` … `sql/06_lag_probe.sql`

---

## Часть 1. Primary и Replica (docker-compose)

В `docker-compose.yml` добавлен второй экземпляр PostgreSQL:

| | Сервис | Порт | Роль |
|---|--------|------|------|
| Primary | `postgres` | **54320** (внутри сети: `postgres:5432`) | принимает INSERT / UPDATE / DELETE |
| Replica | `postgres-replica` | **54330** (`postgres-replica:5432`) | копия Primary, только чтение |

### Ключевые фрагменты docker-compose.yml

**Primary** — включён режим репликации:

```yaml
postgres:
  image: 'postgres:17.5'
  command:
    - postgres
    - -c
    - hba_file=/etc/postgresql/pg_hba.conf   # разрешаем replication-подключения
    - -c
    - wal_level=replica
    - -c
    - max_wal_senders=10
    - -c
    - max_replication_slots=10
    - -c
    - wal_keep_size=128MB
```

**Replica** — создаётся из `pg_basebackup` от Primary, затем постоянно
воспроизводит WAL-поток:

```yaml
postgres-replica:
  image: 'postgres:17.5'
  command:
    - /bin/sh
    - -c
    - |
      if [ ! -s "$$PGDATA/PG_VERSION" ]; then
        pg_basebackup -h postgres -U replicator -D "$$PGDATA" -Fp -Xs -P -R
      fi
      exec gosu postgres postgres -c hot_standby=on
```

### Подключение к экземплярам

```powershell
# Primary (с хоста)
docker exec -it internshipservice-postgres-1 psql -U postgres -d postgres
psql -h localhost -p 54320 -U postgres -d postgres

# Replica (с хоста)
docker exec -it internshipservice-postgres-replica-1 psql -U postgres -d postgres
psql -h localhost -p 54330 -U postgres -d postgres
```

---

## Часть 2. Streaming replication

Цепочка настройки:

1. Primary изменяет данные → изменение фиксируется в **WAL**;
2. Replica (роль `replicator`, создана в `01_setup_primary.sql`) подключается
   по протоколу репликации — `pg_basebackup -R` прописал `primary_conninfo`
   и создал `standby.signal`;
3. WAL sender на Primary отправляет записи, WAL receiver на Replica принимает;
4. Replica **воспроизводит** изменения (recovery / hot standby).

Состояние репликации на Primary (`results/02_replication_status.txt`):

```
 pid |  usename   | application_name | client_addr |   state   | sent_lsn  | ... | replay_lsn
 407 | replicator | walreceiver      | 172.21.0.3  | streaming | 0/7000060 | ... | 0/7000060
```

`state = streaming`, `sent_lsn = replay_lsn` — Replica полностью догнала Primary.

---

## Часть 3. Доказательство работы репликации

**Запись на Primary** (`results/03_write_primary.txt`):

```sql
INSERT INTO applications (candidate_id, vacancy_id, status, cover_letter)
VALUES (777, 7, 1, 'Lab4: written on primary') RETURNING id, candidate_id, status;
-- id = 1001
```

**SELECT на Replica через 2 секунды:**

```
  id  | candidate_id | vacancy_id | status |       cover_letter
 1001 |          777 |          7 |      1 | Lab4: written on primary
(1 row)
```

Запись, созданная только на Primary, появилась на Replica — репликация работает.

---

## Часть 4. Read-only поведение Replica

Попытка INSERT на Replica (`results/05_readonly_test.txt`):

```
ERROR:  cannot execute INSERT in a read-only transaction
rows_with_candidate_999 → 0
```

**Почему Replica не может быть независимой базой для записи:**
Replica работает в режиме восстановления (recovery): она не обслуживает
собственный WAL, а только проигрывает WAL, полученный от Primary. Это
гарантирует, что Replica — точная копия Primary. Если бы Replica принимала
свои записи, данные разошлись бы (расхождение нечем разрешить: у Replica нет
механизма отправки изменений обратно), а репликация разорвалась бы при первом
конфликте. Поэтому сервер в recovery принудительно переводит все транзакции
в read-only.

---

## Часть 5. Чтение собственного сервиса с Replica

Добавлено отдельное подключение к Replica и сервис read-запросов:

Реальный read-сценарий сервиса, идущий на Replica, —
`GET /applications/candidate/{id}` (аналог списка заявок кандидата):

```csharp
// ReplicaQueryService.cs — SELECT уходит на postgres-replica
const string sql = """
    SELECT a.id AS Id, a.candidate_id AS CandidateId, a.vacancy_id AS VacancyId,
           a.status::int AS Status, a.applied_date AS AppliedDate
    FROM applications a
    WHERE a.candidate_id = @CandidateId
    ORDER BY a.applied_date DESC
    """;
```

Также на Replica выполняется агрегация статистики по статусам
(`GROUP BY status`) — типичная тяжёлая read-нагрузка, которую выгодно снять
с Primary.

**Операции записи** (создание заявки, изменение статуса) по-прежнему идут
на Primary через `AppDbContext` / репозитории — их подключение
(`ConnectionStrings:Postgres`) не изменялось.

---

## Часть 6. Replication lag

Два замера (`results/06_lag_check.txt`):

1. **Опрос Replica после INSERT**: запись стала видна на Replica на первом
   же опросе (~208 мс, из которых почти всё — запуск `docker exec psql`;
   `replay_lag` в `pg_stat_replication` в этот момент = **3 мс**).
2. **10 мгновенных выборок `pg_wal_lsn_diff` сразу после INSERT** (на Primary):
   во всех выборках `replay_lag_bytes = 0` — WAL успевает уйти на Replica
   раньше, чем выполняется следующая команда.

**Вывод:** на локальной сети отставание не удалось поймать — репликация
догоняет за единицы миллисекунд. Но репликация асинхронная: Primary не ждёт
подтверждения Replica (синхронный режим не настроен). Под нагрузкой, при
сетевых проблемах или длинных транзакциях Replica отстаёт — и SELECT,
отправленный на Replica сразу после записи на Primary, может увидеть старые
данные. Это и есть **replication lag**: репликация не означает мгновенную
синхронизацию.

---

## Воспроизведение

```powershell
cd Yamatina\Projects\InternshipService
docker compose up -d postgres postgres-replica
docker exec -i internshipservice-postgres-1 psql -U postgres -d postgres < LAB\lab-04-replication\sql\01_setup_primary.sql
docker exec -i internshipservice-postgres-1 psql -U postgres -d postgres < LAB\lab-04-replication\sql\02_replication_status.sql
docker exec -i internshipservice-postgres-replica-1 psql -U postgres -d postgres < LAB\lab-04-replication\sql\03_read_from_replica.sql
docker exec -i internshipservice-postgres-replica-1 psql -U postgres -d postgres < LAB\lab-04-replication\sql\04_readonly_test.sql
docker exec -i internshipservice-postgres-1 psql -U postgres -d postgres < LAB\lab-04-replication\sql\06_lag_probe.sql
```
