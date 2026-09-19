-- ============================================================
-- Лабораторная №5. Шаг 5: что произойдёт при добавлении шарда (3 → 4)
-- для стратегии hash(shard_key) % N.
-- Выполняется на любом шарде (чистое вычисление по 100 000 ключей).
--   docker exec -i lab5-sharding-shard0-1 psql -U postgres -d postgres < 04_reshard_hash_mod.sql
-- ============================================================

WITH keys AS (
    SELECT g AS candidate_id,
           (hashtext(g::text) & 2147483647) % 3 AS shard_before,
           (hashtext(g::text) & 2147483647) % 4 AS shard_after
    FROM generate_series(1, 100000) AS g
)
SELECT
    count(*)                                                            AS total_keys,
    count(*) FILTER (WHERE shard_before = shard_after)                  AS keys_stayed,
    count(*) FILTER (WHERE shard_before <> shard_after)                 AS keys_moved,
    round(count(*) FILTER (WHERE shard_before <> shard_after) * 100.0 / count(*), 2) AS moved_percent
FROM keys;

-- Куда уходят перемещаемые ключи
WITH keys AS (
    SELECT (hashtext(g::text) & 2147483647) % 3 AS shard_before,
           (hashtext(g::text) & 2147483647) % 4 AS shard_after
    FROM generate_series(1, 100000) AS g
)
SELECT shard_before, shard_after, count(*) AS keys
FROM keys
WHERE shard_before <> shard_after
GROUP BY shard_before, shard_after
ORDER BY shard_before, shard_after;
