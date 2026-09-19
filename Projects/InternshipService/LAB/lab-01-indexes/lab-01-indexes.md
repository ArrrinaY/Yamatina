# Лабораторная работа №1
# Индексы и EXPLAIN ANALYZE

> Сырые результаты: `results/02_experiments.txt`, `results/03_remaining.txt`, `results/service_queries_pgadmin.txt`  
> SQL: `sql/01_setup_orders.sql`, `sql/02_experiments.sql`, `sql/03_remaining_experiments.sql`

---

## Часть 1. Тестовая база (lab.orders, 1 000 000 строк)

### Задание 3–4. EXPLAIN / EXPLAIN ANALYZE

**До индекса** (`WHERE user_id = 123`):
```
Parallel Seq Scan on orders
  Filter: (user_id = 123)
  Rows Removed by Filter: 333330
Execution Time: 24.226 ms
```

**Ответы:**
1. План: Parallel Seq Scan + Filter.
2. Тип сканирования: Sequential Scan (параллельный).
3. PostgreSQL читает все страницы таблицы и отфильтровывает строки.

EXPLAIN только строит план; EXPLAIN ANALYZE выполняет запрос и показывает actual time/rows.

### Задание 5. Sequential Scan

| Запрос | Scan | Execution Time |
|--------|------|----------------|
| `SELECT * FROM orders` | Seq Scan | 68.738 ms |
| `WHERE amount > 0` | Seq Scan | 115.500 ms |

Seq Scan нормален, когда читается почти вся таблица (~999 999 строк).

### Задание 6. Первый индекс

| Метрика | До | После |
|---------|-----|-------|
| Тип Scan | Parallel Seq Scan | Bitmap Index Scan |
| Execution Time | **24.226 ms** | **0.182 ms** |
| Обработано строк | ~1 000 000 | 11 actual rows |
| Индекс | — | idx_orders_user_id |

### Задание 7–8. Селективность status

| status | COUNT | ~% |
|--------|-------|-----|
| CANCELLED | 249 684 | 25% |
| DELIVERED | 250 357 | 25% |
| NEW | 250 153 | 25% |
| PAID | 249 806 | 25% |

`WHERE status = 'PAID'`: Bitmap Index Scan, **42.204 ms**, 249 806 rows — индекс используется, но возвращает 25% таблицы, поэтому дорого.

### Задание 9. Range query

| Диапазон | До индекса | После idx_orders_created_at |
|----------|------------|----------------------------|
| 7 days | Parallel Seq Scan, **61.966 ms** | Bitmap Index Scan, **7.357 ms** |

### Задание 10. Bitmap Scan

`WHERE status = 'NEW'`: Bitmap Heap Scan → Bitmap Index Scan, 40.968 ms, 250 153 rows.

### Задание 11. BitmapAnd

`WHERE user_id = 123 AND status = 'PAID'`: Index Scan using **idx_orders_user_status**, **0.049 ms**, 4 rows.

### Задание 12. Составной индекс

Составной `(user_id, status)` эффективнее двух отдельных индексов для совместного условия: один Index Scan вместо BitmapAnd.

### Задание 13. Порядок колонок

| Запрос | (user_id, created_at) | (created_at, user_id) |
|--------|----------------------|----------------------|
| `WHERE user_id = 123` | Index Scan, 0.130 ms | Bitmap Scan, 0.075 ms |
| `WHERE created_at > 30 days` | Seq Scan, 16.263 ms | Bitmap on created_at, 16.173 ms |

Индекс `(user_id, created_at)` не помогает запросу только по `created_at` (left-prefix rule).

### Задание 14–15. ORDER BY + LIMIT

| Этап | Scan | Sort | Execution Time |
|------|------|------|----------------|
| До `(user_id, created_at DESC)` | Bitmap + **Sort** | да | 0.111 ms |
| После индекса | Index Scan | **нет** | 0.024 ms |

### Задание 16. Index Only Scan

`SELECT id, user_id WHERE user_id = 123`: Bitmap Scan, 0.069 ms.  
С INCLUDE-индексом: Index Only Scan возможен при актуальном visibility map.

### Задание 17. Partial index

| | Execution Time | Sort |
|---|----------------|------|
| До partial | 130.374 ms | external merge 16 MB |
| После `WHERE status = 'NEW'` | 153.004 ms | external merge 16 MB |

При равномерном распределении status (~25% NEW) partial index **не даёт выигрыша** — это важный вывод.

### Задание 18. Expression index

| | Scan | Execution Time |
|---|------|----------------|
| `LOWER(email)` без expr index | Seq Scan | **27.664 ms** |
| С `idx_users_lower_email` | Index Scan | **0.044 ms** |

### Задание 19. INSERT (50 000 строк)

| | Время |
|---|-------|
| Без индексов | **152 ms** |
| С 2 индексами | **364 ms** |

### Задание 20. pg_stat_user_indexes

Наиболее используемые: `idx_orders_status` (6 scans), `idx_orders_created_at` (4).  
`idx_orders_new`, `idx_orders_created_at_user` — 0 scans (созданы, но запросы их не выбрали).

### Задание 21. Финальная оптимизация

```sql
SELECT id, amount, status, created_at FROM orders
WHERE user_id = 123 AND status = 'PAID'
  AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC LIMIT 50;
```

| | Execution Time | Scan |
|---|----------------|------|
| До | 0.024 ms | Index Scan (user_id, created_at DESC) + Filter по status |
| После `(user_id, status, created_at DESC)` | **0.039 ms** | Index Scan по `idx_orders_user_status_created_at` |

Составной индекс покрывает все три условия (`user_id`, `status`, `created_at`) в **Index Cond** — отдельный Filter и Sort не нужны.

---

## Часть 2. InternshipService (applications)

### Задание 22. Scaling Entity

| | |
|---|---|
| Таблица | `applications` |
| Назначение | Заявки кандидатов на вакансии |
| Рост | O(кандидаты × вакансии) |
| Объём | 100 000 записей (generate-test-data) |
| Поля поиска | candidate_id, vacancy_id, status, applied_date |

### Задания 23–24. Запросы и измерения

> Измерения выполнены в pgAdmin, `candidate_id = 1` (0 строк в выборке).

#### Query 1 — по candidate_id + JOIN

```sql
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
JOIN vacancies v ON a.vacancy_id = v.id
JOIN companies co ON v.company_id = co.id
WHERE a.candidate_id = 1
ORDER BY a.applied_date DESC;
```

| Метрика | Значение |
|---------|----------|
| Execution Time | **0.111 ms** |
| Planning Time | 1.240 ms |
| Scan по applications | **Append** по 16 партициям |
| JOIN | **Nested Loop** → Index Scan `vacancies_pkey` → Index Only Scan `companies_pkey` |
| Сортировка | **Sort** по `applied_date DESC` |
| Индекс на партициях | `applications_*_candidate_id_applied_date_idx` |

План: PostgreSQL собирает строки из всех партиций (Append), для каждой строки делает JOIN по PK вакансий и компаний, затем сортирует результат. JOIN-ветки помечены `never executed`, т.к. заявок для `candidate_id = 1` нет.

#### Query 2 — status + date range

```sql
WHERE candidate_id = 1 AND status = 0
  AND applied_date >= '2026-01-01' AND applied_date < '2026-12-31'
ORDER BY applied_date DESC LIMIT 20;
```

| Метрика | Значение |
|---------|----------|
| Execution Time | **0.081 ms** |
| Planning Time | 1.491 ms |
| Scan | **Append** только по партициям **2026_01 … 2026_12** (12 шт.) |
| Partition pruning | ✅ партиции 2025 и default **отсечены** |
| Сортировка | Sort → Limit 20 |

Ключевой вывод: при фильтре по диапазону дат PostgreSQL применяет **partition pruning** и сканирует только релевантные месячные партиции.

#### Query 3 — pagination

```sql
WHERE candidate_id = 1
ORDER BY applied_date DESC LIMIT 20;
```

| Метрика | Значение |
|---------|----------|
| Execution Time | **0.094 ms** |
| Planning Time | 0.778 ms |
| Scan | **Append** по всем 16 партициям |
| Сортировка | **Sort** → Limit 20 |
| Индекс | Index Scan `candidate_id_applied_date_idx` в партициях с данными (2025_10–2025_12) |

Без фильтра по дате pruning не срабатывает — сканируются все партиции, затем общий Sort.

#### Сводная таблица

| Query | Execution Time | Append | Sort | Partition pruning |
|-------|----------------|--------|------|-------------------|
| 1 (JOIN) | **0.111 ms** | 16 партиций | да | нет |
| 2 (фильтры) | **0.081 ms** | 12 партиций (2026) | да | **да** |
| 3 (pagination) | **0.094 ms** | 16 партиций | да | нет |

### Задание 25–26. Анализ индексов

Индексы на `applications`: `candidate_id`, `vacancy_id`, `status`, `applied_date`, составной `(candidate_id, applied_date DESC)` на каждой партиции.

**Query 1:** индексы на партициях используются (Index Scan по `candidate_id`), JOIN идёт по PK связанных таблиц — эффективно.

**Query 2:** partition pruning сокращает число сканируемых партиций с 16 до 12; на пустых партициях — быстрый Seq Scan.

**Query 3:** без фильтра по дате Append охватывает все партиции, Sort неизбежен — это цена партиционирования при «широком» запросе.

### Задания 27–29. Оптимизация

Миграция `010_add_applications_composite_index.sql`:
```sql
CREATE INDEX idx_applications_candidate_applied_date
ON applications (candidate_id, applied_date DESC);
```

Составной индекс создаётся на родительской таблице и наследуется партициями. В EXPLAIN видно `*_candidate_id_applied_date_idx` на партициях 2025_10–2025_12.

| Метрика | Без composite (только candidate_id) | С composite |
|---------|-------------------------------------|-------------|
| Scan | Index/Bitmap Scan + Filter + Sort | Index Scan по партициям |
| Execution Time (Query 3) | выше при большом числе строк | **0.094 ms** |

### Задание 30. Почему нельзя индексировать всё?

- Рост диска (INSERT 50k: 152→364 ms при 2 индексах)
- Каждый INSERT/UPDATE обновляет все индексы
- Неиспользуемые индексы (idx_scan=0) — чистые накладные расходы
- Планировщик не обязан использовать индекс
