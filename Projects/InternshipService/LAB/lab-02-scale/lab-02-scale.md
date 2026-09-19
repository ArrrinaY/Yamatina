# Лабораторная работа №2
# Когда индексов недостаточно

> Сырые результаты: `results/sizes.txt`, `results/seqscan.txt`, `results/indexscan.txt`

---

## Часть A. lab.events

### Задание 3. Размер таблицы

| Строк | Размер таблицы | Общий размер |
|-------|----------------|--------------|
| 10 000 | 672 kB | 944 kB |
| 100 000 | 6672 kB (6.5 MB) | 8912 kB (8.7 MB) |
| 1 000 000 | 65 MB | 87 MB |
| 5 000 000 | — | не генерировали (ограничение времени) |
| 10 000 000 | — | не генерировали |

### Задание 4. SELECT без индекса

`EXPLAIN ANALYZE SELECT * FROM lab.events WHERE user_id = 123`

| Строк | Scan | Actual Rows | Rows Removed | Execution Time |
|-------|------|-------------|--------------|----------------|
| 10 000 | Seq Scan | 0 | 10 000 | **0.506 ms** |
| 100 000 | Seq Scan | 0 | 100 000 | **8.469 ms** |
| 1 000 000 | Parallel Seq Scan | 10 | 999 990 | **35.626 ms** |

**Вывод:** время растёт линейно — ~0.05 ms/10k на малых объёмах, ~35 ms на 1M.

### Задание 5. С индексом idx_events_user_id

| Объём | Без индекса | С индексом | Ускорение |
|-------|-------------|------------|-----------|
| 100 000 | 8.469 ms | **0.059 ms** | ~143× |
| 1 000 000 | 35.626 ms | **0.098 ms** | ~363× |

На 10k разница мала (0.506 vs 0.096 ms) — Seq Scan дешевле overhead индекса.

### Задание 6. Диапазон дат

Короткий диапазон (1 day): Index Scan выгоден.  
Широкий (365 days): Seq Scan — индекс возвращает ~100% строк.

### Задание 7. Sort

`WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100`:
- До `(user_id, created_at DESC)`: Sort присутствует
- После: Index Scan без Sort

### Задание 8. GROUP BY

HashAggregate после фильтрации по created_at. Индекс помогает WHERE, но не устраняет агрегацию.

### Задание 9. INSERT 100 000 строк

| | Время |
|---|-------|
| Без индексов | **205.392 ms** |
| 3 индекса | **739.195 ms** |

Замедление ~3.6× — каждый INSERT обновляет все B-tree.

### Задание 10. Размер индексов (1M строк)

Индексы ~22 MB суммарно (total 87 MB − table 65 MB).

### Задание 11. Когда индекс не спасает

`GROUP BY DATE(created_at)` за 365 дней на 100M строк: индекс находит строки, но агрегация миллионов записей остаётся дорогой. Решения: партиционирование, materialized views.

---

## Часть B. applications (100 000 записей)

### Задание 12–13. Сущность и запросы

**Query 1** — по candidate_id:
```sql
SELECT a.id FROM applications a WHERE a.candidate_id = $1
ORDER BY a.applied_date DESC LIMIT 20;
```

**Query 2** — date range:
```sql
SELECT COUNT(*) FROM applications
WHERE applied_date >= '2025-06-01' AND applied_date < '2025-07-01';
```

**Query 3** — filter + sort:
```sql
SELECT a.* FROM applications a WHERE a.candidate_id = $1
  AND a.status = 0 ORDER BY a.applied_date DESC LIMIT 20;
```

### Задание 14–16. Измерения (100k)

| Запрос | Execution Time | Scan | Индекс |
|--------|----------------|------|--------|
| Q1 | 0.350 ms | Index Scan (партиции) | candidate_applied_date |
| Q2 | 0.971 ms | Index Only Scan | applied_date |
| Q3 | 0.089 ms | Index Scan | candidate_applied_date |

### Задание 17. Узкое место

**Query 2** без candidate_id — сканирует партицию целиком. При росте до 5M+ и широком диапазоне время будет расти линейно.

### Задание 18. Оптимизация

Партиционирование по `applied_date` (Lab 3) + индекс: Query 2 использует Index Only Scan на одной партиции (**0.971 ms** на 8640 rows).

### Задание 19. Граница индексов

При 1M events Seq Scan занимает **35.6 ms**, с индексом — **0.1 ms**.  
Но Query 2 на applications с date range без partition pruning на всех партициях станет узким местом >100 ms — **одного индекса недостаточно**, нужно партиционирование.
