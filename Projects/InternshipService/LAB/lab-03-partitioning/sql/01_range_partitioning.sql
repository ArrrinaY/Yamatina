-- Лабораторная 3: RANGE partitioning (учебные примеры)
CREATE SCHEMA IF NOT EXISTS lab;

DROP TABLE IF EXISTS lab.events_part CASCADE;

CREATE TABLE lab.events_part (
    id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload TEXT,
    created_at TIMESTAMP NOT NULL
) PARTITION BY RANGE (created_at);

CREATE TABLE lab.events_part_2026_09_09 PARTITION OF lab.events_part
    FOR VALUES FROM ('2026-09-09') TO ('2026-09-10');
CREATE TABLE lab.events_part_2026_09_10 PARTITION OF lab.events_part
    FOR VALUES FROM ('2026-09-10') TO ('2026-09-11');
CREATE TABLE lab.events_part_2026_09_11 PARTITION OF lab.events_part
    FOR VALUES FROM ('2026-09-11') TO ('2026-09-12');

INSERT INTO lab.events_part (id, user_id, event_type, payload, created_at)
SELECT
    gs,
    (random() * 1000)::bigint,
    CASE WHEN random() < 0.5 THEN 'click' ELSE 'view' END,
    'payload',
    TIMESTAMP '2026-09-09' + (random() * INTERVAL '3 days')
FROM generate_series(1, 3000) gs;

SELECT tableoid::regclass AS partition_name, COUNT(*)
FROM lab.events_part
GROUP BY tableoid
ORDER BY partition_name;

EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*)
FROM lab.events_part
WHERE created_at >= '2026-09-10'
  AND created_at < '2026-09-11';

EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*)
FROM lab.events_part
WHERE event_type = 'click';
