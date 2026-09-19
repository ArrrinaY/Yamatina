-- Part 3: RANGE by price
CREATE SCHEMA IF NOT EXISTS lab;

DROP TABLE IF EXISTS lab.products CASCADE;
CREATE TABLE lab.products (
    id BIGINT NOT NULL,
    name TEXT NOT NULL,
    price NUMERIC NOT NULL
) PARTITION BY RANGE (price);

CREATE TABLE lab.products_cheap PARTITION OF lab.products FOR VALUES FROM (0) TO (100);
CREATE TABLE lab.products_medium PARTITION OF lab.products FOR VALUES FROM (100) TO (1000);
CREATE TABLE lab.products_expensive PARTITION OF lab.products FOR VALUES FROM (1000) TO (MAXVALUE);

INSERT INTO lab.products VALUES
    (1, 'Pen', 10),
    (2, 'Book', 50),
    (3, 'Laptop', 500),
    (4, 'Server', 5000);

SELECT tableoid::regclass AS partition_name, COUNT(*) FROM lab.products GROUP BY 1 ORDER BY 1;

\echo '=== Part 3: price range query ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.products WHERE price >= 100 AND price < 500;

-- Part 8-9: partitioning + indexes on events_part
\echo '=== Part 8: partition + index ==='
CREATE INDEX IF NOT EXISTS idx_events_part_user_id ON lab.events_part(user_id);
EXPLAIN (ANALYZE, BUFFERS)
SELECT * FROM lab.events_part
WHERE created_at >= '2026-09-10' AND created_at < '2026-09-11' AND user_id = 12345;

\echo '=== Part 9: event_type without pruning help ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM lab.events_part WHERE event_type = 'click';
CREATE INDEX IF NOT EXISTS idx_events_part_event_type ON lab.events_part(event_type);
EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM lab.events_part WHERE event_type = 'click';

-- Service: applications partition pruning
\echo '=== Service: applications pruning ==='
EXPLAIN (ANALYZE, BUFFERS)
SELECT COUNT(*) FROM applications
WHERE applied_date >= '2026-09-01' AND applied_date < '2026-10-01';

EXPLAIN (ANALYZE, BUFFERS)
SELECT a.id, a.candidate_id, a.status, a.applied_date
FROM applications a WHERE a.candidate_id = 1
ORDER BY a.applied_date DESC LIMIT 20;

EXPLAIN (ANALYZE, BUFFERS)
SELECT a.status, COUNT(*) FROM applications a
INNER JOIN vacancies v ON a.vacancy_id = v.id
WHERE a.vacancy_id = 1
GROUP BY a.status;
