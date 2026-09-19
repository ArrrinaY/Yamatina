-- ============================================================
-- Лабораторная №5. Шаг 6–7: Consistent Hash Ring и сравнение стратегий.
-- Выполняется на любом шарде:
--   docker exec -i lab5-sharding-shard0-1 psql -U postgres -d postgres < 05_consistent_hashing.sql
--
-- Базовая версия: 1 виртуальная нода на шард.
-- Точка ключа на кольце = hash(key); владельцем считается ближайший
-- шард по часовой стрелке (первый token >= pos, иначе wrap на минимальный).
-- ============================================================

CREATE TEMP TABLE keys AS
SELECT g AS candidate_id,
       hashtext(g::text) & 2147483647 AS pos
FROM generate_series(1, 100000) AS g;

-- Кольцо из 3 шардов и кольцо из 4 шардов
CREATE TEMP TABLE ring3 AS
SELECT i AS shard, hashtext('shard-' || i::text) & 2147483647 AS token
FROM generate_series(0, 2) AS i;

CREATE TEMP TABLE ring4 AS
SELECT i AS shard, hashtext('shard-' || i::text) & 2147483647 AS token
FROM generate_series(0, 3) AS i;

-- Позиции шардов на кольце
SELECT 'ring3' AS ring, shard, token FROM ring3
UNION ALL
SELECT 'ring4' AS ring, shard, token FROM ring4
ORDER BY ring, token;

-- Распределение ключей по кольцу (3 шарда)
WITH a3 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM ring3 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM ring3 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
)
SELECT shard, count(*) AS keys, round(count(*) * 100.0 / 100000, 2) AS percent
FROM a3 GROUP BY shard ORDER BY shard;

-- Распределение ключей по кольцу (4 шарда)
WITH a4 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM ring4 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM ring4 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
)
SELECT shard, count(*) AS keys, round(count(*) * 100.0 / 100000, 2) AS percent
FROM a4 GROUP BY shard ORDER BY shard;

-- Сколько ключей меняет шард при 3 → 4 (Consistent Hashing)
WITH a3 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM ring3 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM ring3 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
), a4 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM ring4 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM ring4 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
)
SELECT count(*) AS total_keys,
       count(*) FILTER (WHERE a3.shard = a4.shard) AS keys_stayed,
       count(*) FILTER (WHERE a3.shard <> a4.shard) AS keys_moved,
       round(count(*) FILTER (WHERE a3.shard <> a4.shard) * 100.0 / count(*), 2) AS moved_percent
FROM a3 JOIN a4 USING (candidate_id);

-- ============================================================
-- Бонус: Consistent Hashing с виртуальными нодами (100 на шард) —
-- демонстрация выравнивания распределения.
-- ============================================================

CREATE TEMP TABLE vring3 AS
SELECT (i / 100) AS shard, hashtext('shard-' || (i / 100)::text || '-vnode-' || (i % 100)::text) & 2147483647 AS token
FROM generate_series(0, 299) AS i;

CREATE TEMP TABLE vring4 AS
SELECT (i / 100) AS shard, hashtext('shard-' || (i / 100)::text || '-vnode-' || (i % 100)::text) & 2147483647 AS token
FROM generate_series(0, 399) AS i;

-- Распределение с виртуальными нодами (4 шарда)
WITH a4 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM vring4 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM vring4 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
)
SELECT shard, count(*) AS keys, round(count(*) * 100.0 / 100000, 2) AS percent
FROM a4 GROUP BY shard ORDER BY shard;

-- Перемещение при 3 → 4 с виртуальными нодами
WITH a3 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM vring3 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM vring3 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
), a4 AS (
    SELECT k.candidate_id,
           COALESCE(
               (SELECT r.shard FROM vring4 r WHERE r.token >= k.pos ORDER BY r.token LIMIT 1),
               (SELECT r.shard FROM vring4 r ORDER BY r.token LIMIT 1)) AS shard
    FROM keys k
)
SELECT count(*) AS total_keys,
       count(*) FILTER (WHERE a3.shard = a4.shard) AS keys_stayed,
       count(*) FILTER (WHERE a3.shard <> a4.shard) AS keys_moved,
       round(count(*) FILTER (WHERE a3.shard <> a4.shard) * 100.0 / count(*), 2) AS moved_percent
FROM a3 JOIN a4 USING (candidate_id);
