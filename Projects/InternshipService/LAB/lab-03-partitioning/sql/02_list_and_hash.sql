-- LIST и HASH partitioning
CREATE SCHEMA IF NOT EXISTS lab;

DROP TABLE IF EXISTS lab.customers CASCADE;
CREATE TABLE lab.customers (
    id BIGINT NOT NULL,
    name TEXT NOT NULL,
    customer_type VARCHAR(30) NOT NULL
) PARTITION BY LIST (customer_type);

CREATE TABLE lab.customers_b2c PARTITION OF lab.customers FOR VALUES IN ('B2C');
CREATE TABLE lab.customers_b2b PARTITION OF lab.customers FOR VALUES IN ('B2B');
CREATE TABLE lab.customers_enterprise PARTITION OF lab.customers FOR VALUES IN ('Enterprise');
CREATE TABLE lab.customers_default PARTITION OF lab.customers DEFAULT;

INSERT INTO lab.customers VALUES (1, 'Alice', 'B2C'), (2, 'Bob Corp', 'B2B'), (3, 'VIP User', 'VIP');

EXPLAIN (ANALYZE, BUFFERS) SELECT * FROM lab.customers WHERE customer_type = 'B2B';

DROP TABLE IF EXISTS lab.user_events CASCADE;
CREATE TABLE lab.user_events (
    id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50),
    created_at TIMESTAMP NOT NULL
) PARTITION BY HASH (user_id);

CREATE TABLE lab.user_events_0 PARTITION OF lab.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 0);
CREATE TABLE lab.user_events_1 PARTITION OF lab.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 1);
CREATE TABLE lab.user_events_2 PARTITION OF lab.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 2);
CREATE TABLE lab.user_events_3 PARTITION OF lab.user_events FOR VALUES WITH (MODULUS 4, REMAINDER 3);

INSERT INTO lab.user_events (id, user_id, event_type, created_at)
SELECT gs, (random() * 100000)::bigint, 'login', NOW()
FROM generate_series(1, 100000) gs;

SELECT tableoid::regclass AS partition_name, COUNT(*)
FROM lab.user_events
GROUP BY tableoid
ORDER BY partition_name;
