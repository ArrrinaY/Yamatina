# Лабораторная работа №3
# Партиционирование PostgreSQL

> Сырые результаты: `results/01_range.txt`, `results/02_list_hash.txt`, `results/03_products.txt`

---

## Части 1–2. RANGE + pruning (lab.events_part)

### Ответы

1. `2026-09-10 12:00:00` → `events_part_2026_09_10`
2. `2026-09-11 00:00:00` → `events_part_2026_09_11` (FROM inclusive)
3. `2026-09-12` без партиции → **ERROR: no partition**
4. `TO ('2026-09-11')` не включает 11-е (полуинтервал [FROM, TO))

### Partition pruning (измерено)

```sql
EXPLAIN ANALYZE SELECT COUNT(*) FROM lab.events_part
WHERE created_at >= '2026-09-10' AND created_at < '2026-09-11';
```

→ **Seq Scan только на events_part_2026_09_10**, Execution Time: **0.143 ms**

```sql
WHERE event_type = 'click';
```

→ **Append всех 3 партиций**, Execution Time: **0.822 ms** — pruning не работает (ключ = created_at).

---

## Часть 3. RANGE по price (lab.products)

| partition | count |
|-----------|-------|
| products_cheap | 2 |
| products_medium | 1 |
| products_expensive | 1 |

`WHERE price >= 100 AND price < 500` → **Seq Scan только products_medium**, 0.011 ms.

---

## Части 4–7. LIST, HASH, DEFAULT, стратегии

- LIST `customer_type = 'B2B'` → одна партиция
- VIP без DEFAULT → ошибка; с DEFAULT → попадает в default
- HASH 4 партиции: ~равномерное распределение 100k строк

| Сценарий | Стратегия |
|----------|-----------|
| A: удаление старых событий | RANGE по дате |
| B: B2C/B2B/Enterprise | LIST |
| C: равномерно по user_id | HASH |
| D: аналитика по created_at | RANGE |
| E: страны EE/LV/LT | LIST |

---

## Части 8–9. Партиции + индексы

```sql
CREATE INDEX idx_events_part_user_id ON lab.events_part(user_id);
EXPLAIN ANALYZE SELECT * FROM lab.events_part
WHERE created_at >= '2026-09-10' AND created_at < '2026-09-11'
  AND user_id = 12345;
```

→ Pruning до 1 партиции + **Bitmap Index Scan** на user_id, **0.158 ms**.

`WHERE event_type = 'click'`: partitioning не помог → индекс на event_type ускоряет внутри партиций.

---

## Части 10–11. Автоматизация (InternshipService)

| Компонент | Файл |
|-----------|------|
| CreatePartitionsJob | `Partitioning/CreatePartitionsJob.cs` |
| PartitionHealthMonitor | `Partitioning/PartitionHealthMonitor.cs` |
| PartitionHealthCheck | `Partitioning/PartitionHealthCheck.cs` + `/health` |
| PartitionAlertService | `Partitioning/PartitionAlertService.cs` (dedup alert) |

**Обоснование monthly vs daily:** заявки (`applications`) — месячные объёмы; горизонт 3 месяца вперёд.

### Сценарий alert (11.2)

```sql
DROP TABLE applications_2026_12;  -- симуляция сбоя job
-- PartitionHealthMonitor → CRITICAL → alert в лог
-- CreatePartitionsJob → создаёт партицию
-- повторная проверка → OK → recovery alert
```

---

## Часть 12. applications (сервис)

| | |
|---|---|
| Таблица | applications |
| Ключ | applied_date |
| Стратегия | RANGE (monthly) |
| Партиции | applications_2025_10 … applications_2026_12 + default |

### 3 API-запроса с EXPLAIN (ANALYZE, BUFFERS)

**1. Date range (partition pruning):**
```sql
SELECT COUNT(*) FROM applications
WHERE applied_date >= '2025-06-01' AND applied_date < '2025-07-01';
```
→ Index Only Scan на **applications_default**, **0.971 ms**, 8640 rows.

**2. By candidate (GET /applications/candidate/{id}/filter):**
→ Index Scan по партициям, **0.350 ms**.

**3. Statistics (GET /applications/vacancy/{id}/statistics):**
→ HashAggregate + Join, pruning по партициям.

---

## Воспроизведение

```powershell
Get-Content LAB\lab-03-partitioning\sql\01_range_partitioning.sql | docker exec -i internshipservice-postgres-1 psql -U postgres -d postgres
curl http://localhost:8080/health
```
