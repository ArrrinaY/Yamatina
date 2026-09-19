# Лабораторная работа №5
# Шардирование PostgreSQL: Router и Consistent Hashing

> Сырые результаты: `results/01_setup_shards.txt` … `results/06_router_csharp_output.txt`
> SQL: `sql/01_setup_shards.sql` … `sql/05_consistent_hashing.sql`

---

## 1. Выбор данных для шардирования

| | |
|---|---|
| Сущность | **applications** (заявки на стажировку) — основная и самая объёмная таблица сервиса |
| Shard key | **candidate_id** |
| Почему | заявки почти всегда запрашиваются в разрезе кандидата (`WHERE candidate_id = ?`); ключ — ссылка на `candidates.id`, распределение значений равномерное; все заявки одного кандидата попадают на один шард |
| Равномерность | hash от монотонного integer даёт равномерное распределение — подтверждено экспериментом: 33.29% / 33.44% / 33.27% |
| Запросы с этим ключом | `GetByCandidateIdAsync`, `GetFilteredAsync` (фильтр + пагинация), проверка «уже подал заявку» при создании |

## 2. Запуск трёх PostgreSQL

`LAB/lab-05-sharding/docker-compose.yml` (изолированный compose-проект `lab5-sharding`):

| Контейнер | Порт |
|-----------|------|
| `lab5-sharding-shard0-1` | 54340 |
| `lab5-sharding-shard1-1` | 54341 |
| `lab5-sharding-shard2-1` | 54342 |
| `lab5-sharding-shard3-1` | 54343 (пустой — для эксперимента 3 → 4) |

Таблица `applications` (структура как в сервисе: `id, candidate_id, vacancy_id, status, applied_date, cover_letter`) создана на каждом шарде одинаково (`01_setup_shards.sql`).

## 3. Реализация Router

Код в сервисе (`InternshipService/Sharding/`):

| Компонент | Файл |
|-----------|------|
| Интерфейс router'а | `IShardRouter.cs` — `int GetShardIndex(long shardKey)` |
| Детерминированный FNV-1a хеш | `StableHash.cs` |
| Стратегия `hash(key) % N` | `HashModuloShardRouter.cs` |
| Consistent Hash Ring | `ConsistentHashRing.cs` (бинарный поиск по кольцу, virtual nodes) |
| Стратегия Consistent Hashing | `ConsistentHashShardRouter.cs` |
| Фабрика подключений к шардам | `ShardConnectionFactory.cs` + `ShardSettings` в `appsettings.json` |
| Демонстрация через API | `Controllers/ShardingController.cs` → `GET /api/sharding/resolve/{key}` |

Пример маршрутизации (C# router, `results/06_router_csharp_output.txt`):

```
hash(101) = 7870873943183657792 | hash % 3 -> shard 2 | consistent -> shard 1
hash(102) = 5344448127231650211 | hash % 3 -> shard 0 | consistent -> shard 2
hash(103) = 3112132720264060802 | hash % 3 -> shard 2 | consistent -> shard 2
```

## 4. Распределение данных

Загружено 100 000 заявок (`02_load_shard.sql`): router (hash % 3) отправляет
каждую строку только на «свой» шард.

| Шард | Заявок | % | Размер |
|------|--------|---|--------|
| Shard 0 | 33 286 | 33.29 | 4584 kB |
| Shard 1 | 33 440 | 33.44 | 4608 kB |
| Shard 2 | 33 274 | 33.27 | 4584 kB |

**Вывод:** распределение практически идеальное (разброс ~0.2 п.п.) — hash от
равномерного ключа хорошо балансирует данные. Дисбаланса нет; если бы он был
(например, у «горячего» ключа аномально много строк), нагруженный шард стал бы
узким местом — проблема «hot shard» (Лабораторная №6).

## 5. Добавление нового шарда (3 → 4), hash(key) % N

Раньше `hash % 3`, теперь `hash % 4`. Для тех же 100 000 ключей
(`results/04_reshard_hash_mod.txt`):

| Всего | Изменили шард | Остались | Перемещено |
|-------|---------------|----------|------------|
| 100 000 | **75 052** | 24 948 | **75.05 %** |

Ключи разъезжаются по всем парам шардов (0→1, 0→2, 0→3, 1→0, … — по ~8 300 в каждой паре).

**Почему так много:** остаток от деления зависит от N. При смене модуля с 3 на 4
остаток меняется у ~3/4 всех ключей — практически все данные нужно
перераспределить между экземплярами. Добавление одного PostgreSQL вызывает
перенос большинства данных → долгая миграция, пиковая нагрузка на сеть и диск.
