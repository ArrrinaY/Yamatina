-- Лабораторная 2: измерения на разных объёмах
-- Перед каждым блоком загрузите нужный объём через 03_generate_events.sql

-- SELECT без индекса
EXPLAIN ANALYZE SELECT * FROM lab.events WHERE user_id = 123;

-- SELECT с индексом
CREATE INDEX IF NOT EXISTS idx_events_user_id ON lab.events(user_id);
EXPLAIN ANALYZE SELECT * FROM lab.events WHERE user_id = 123;

-- Диапазон дат
EXPLAIN ANALYZE SELECT * FROM lab.events WHERE created_at >= NOW() - INTERVAL '1 day';
CREATE INDEX IF NOT EXISTS idx_events_created_at ON lab.events(created_at);
EXPLAIN ANALYZE SELECT * FROM lab.events WHERE created_at >= NOW() - INTERVAL '1 day';

-- Фильтрация + сортировка
EXPLAIN ANALYZE
SELECT * FROM lab.events
WHERE user_id = 123
ORDER BY created_at DESC
LIMIT 100;

CREATE INDEX IF NOT EXISTS idx_events_user_created ON lab.events(user_id, created_at DESC);
EXPLAIN ANALYZE
SELECT * FROM lab.events
WHERE user_id = 123
ORDER BY created_at DESC
LIMIT 100;

-- Агрегация
EXPLAIN ANALYZE
SELECT event_type, COUNT(*)
FROM lab.events
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY event_type;

-- Размер таблицы и индексов
SELECT pg_size_pretty(pg_relation_size('lab.events'));
SELECT pg_size_pretty(pg_total_relation_size('lab.events'));
SELECT indexrelname, pg_size_pretty(pg_relation_size(indexrelid))
FROM pg_stat_user_indexes
WHERE schemaname = 'lab' AND relname = 'events';
