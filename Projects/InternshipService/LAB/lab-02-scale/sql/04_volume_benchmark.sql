-- Benchmark at configurable volume. Set :row_count before running.
-- Usage: psql -v row_count=100000 -f 04_volume_benchmark.sql

\set rows :row_count

\echo '=== Volume:' :rows 'rows ==='

DROP TABLE IF EXISTS lab.events CASCADE;
CREATE TABLE lab.events (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL
);

INSERT INTO lab.events (user_id, event_type, payload, created_at)
SELECT
    (random() * 100000)::bigint,
    CASE
        WHEN random() < 0.4 THEN 'MESSAGE'
        WHEN random() < 0.7 THEN 'LOGIN'
        WHEN random() < 0.9 THEN 'PURCHASE'
        ELSE 'OTHER'
    END,
    '{}'::jsonb,
    NOW() - (random() * INTERVAL '365 days')
FROM generate_series(1, :rows);

ANALYZE lab.events;

SELECT :rows AS row_count,
       pg_size_pretty(pg_relation_size('lab.events')) AS table_size,
       pg_size_pretty(pg_total_relation_size('lab.events')) AS total_size;

\echo '=== Q4: no index ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE user_id = 123;

\echo '=== Q5: with index ==='
CREATE INDEX idx_events_user_id ON lab.events(user_id);
ANALYZE lab.events;
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE user_id = 123;

\echo '=== Q6: date range ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE created_at >= NOW() - INTERVAL '1 day';
CREATE INDEX idx_events_created_at ON lab.events(created_at);
ANALYZE lab.events;
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE created_at >= NOW() - INTERVAL '1 day';

\echo '=== Q7: sort ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;
CREATE INDEX idx_events_user_created ON lab.events(user_id, created_at DESC);
ANALYZE lab.events;
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events WHERE user_id = 123 ORDER BY created_at DESC LIMIT 100;

\echo '=== Q8: aggregation ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT event_type, COUNT(*)
FROM lab.events
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY event_type;

\echo '=== Q10: index sizes ==='
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid)) AS index_size
FROM pg_stat_user_indexes
WHERE schemaname = 'lab' AND relname = 'events';
