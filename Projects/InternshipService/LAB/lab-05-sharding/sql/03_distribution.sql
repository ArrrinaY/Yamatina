-- ============================================================
-- Лабораторная №5. Шаг 4: распределение данных по шардам.
-- Выполняется на каждом шарде:
--   docker exec -i lab5-sharding-shard0-1 psql -U postgres -d postgres -v shard=0 < 03_distribution.sql
-- ============================================================

SELECT :shard AS shard_index,
       count(*) AS applications,
       round(count(*) * 100.0 / 100000, 2) AS percent_of_total,
       pg_size_pretty(pg_total_relation_size('applications')) AS table_size
FROM applications;

-- На какой шард попали первые 10 кандидатов (по правилу hash % 3)
SELECT g AS candidate_id,
       (hashtext(g::text) & 2147483647) % 3 AS shard,
       hashtext(g::text) & 2147483647 AS hash_value
FROM generate_series(1, 10) AS g
ORDER BY g;
