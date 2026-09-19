-- Задания 5, 10, 11, 13, 16-20

-- === Задание 5: Sequential Scan ===
-- Assignment 5: full table scan
EXPLAIN ANALYZE SELECT * FROM lab.orders;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE amount > 0;

-- === Задание 10: Bitmap Scan ===
-- Assignment 10: bitmap scan
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE status = 'NEW';
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE amount BETWEEN 1000 AND 3000;

-- === Задание 11: BitmapAnd ===
-- Assignment 11: two indexes
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123 AND status = 'PAID';

-- === Задание 13: column order ===
-- Assignment 13: column order
CREATE INDEX IF NOT EXISTS idx_orders_user_created_at ON lab.orders(user_id, created_at);
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123 AND created_at > NOW() - INTERVAL '30 days';
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE created_at > NOW() - INTERVAL '30 days';

CREATE INDEX IF NOT EXISTS idx_orders_created_at_user ON lab.orders(created_at, user_id);
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE created_at > NOW() - INTERVAL '30 days';

-- === Задание 16: Index Only Scan ===
-- Assignment 16: index only scan
EXPLAIN ANALYZE SELECT id, user_id FROM lab.orders WHERE user_id = 123;
CREATE INDEX IF NOT EXISTS idx_orders_user_id_include ON lab.orders(user_id) INCLUDE (id, status, created_at);
ANALYZE lab.orders;
EXPLAIN ANALYZE SELECT id, user_id, status FROM lab.orders WHERE user_id = 123;

-- === Задание 17: Partial index ===
-- Assignment 17: partial index
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE status = 'NEW' ORDER BY created_at;
CREATE INDEX IF NOT EXISTS idx_orders_new ON lab.orders(created_at) WHERE status = 'NEW';
ANALYZE lab.orders;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE status = 'NEW' ORDER BY created_at;

-- === Задание 18: Expression index ===
-- Assignment 18: expression index
DROP TABLE IF EXISTS lab.users_email CASCADE;
CREATE TABLE lab.users_email (
    id BIGSERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL
);
INSERT INTO lab.users_email (email)
SELECT 'user' || gs || '@example.com' FROM generate_series(1, 100000) gs;
CREATE INDEX idx_users_email ON lab.users_email(email);
ANALYZE lab.users_email;
EXPLAIN ANALYZE SELECT * FROM lab.users_email WHERE LOWER(email) = 'user1@example.com';
CREATE INDEX idx_users_lower_email ON lab.users_email(LOWER(email));
ANALYZE lab.users_email;
EXPLAIN ANALYZE SELECT * FROM lab.users_email WHERE LOWER(email) = 'user1@example.com';

-- === Задание 19: INSERT cost ===
-- Assignment 19: insert benchmark (время смотри в Messages после каждого INSERT)
DROP TABLE IF EXISTS lab.insert_test CASCADE;
CREATE TABLE lab.insert_test (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    amount NUMERIC(10,2) NOT NULL
);
-- INSERT без индексов (засеки время в Messages):
INSERT INTO lab.insert_test (user_id, amount)
SELECT (random()*100000)::bigint, random()*10000 FROM generate_series(1, 50000);
TRUNCATE lab.insert_test;
CREATE INDEX idx_insert_test_user ON lab.insert_test(user_id);
CREATE INDEX idx_insert_test_amount ON lab.insert_test(amount);
-- INSERT с 2 индексами (сравни время с предыдущим):
INSERT INTO lab.insert_test (user_id, amount)
SELECT (random()*100000)::bigint, random()*10000 FROM generate_series(1, 50000);

-- === Задание 20: index usage stats ===
-- Assignment 20: pg_stat_user_indexes
SELECT schemaname, relname, indexrelname, idx_scan
FROM pg_stat_user_indexes
WHERE schemaname = 'lab' AND relname = 'orders'
ORDER BY idx_scan DESC;

-- === Service queries (assignments 24-26) ===
-- Service Query 1
EXPLAIN ANALYZE
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
INNER JOIN vacancies v ON a.vacancy_id = v.id
INNER JOIN companies co ON v.company_id = co.id
WHERE a.candidate_id = 1
ORDER BY a.applied_date DESC;

-- Service Query 2
EXPLAIN ANALYZE
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
INNER JOIN vacancies v ON a.vacancy_id = v.id
WHERE a.candidate_id = 1
  AND a.status = 0
  AND a.applied_date >= '2026-01-01'
  AND a.applied_date < '2026-12-31'
ORDER BY a.applied_date DESC
LIMIT 20;

-- Service Query 3
EXPLAIN ANALYZE
SELECT a.id, a.candidate_id, a.vacancy_id, a.status, a.applied_date
FROM applications a
WHERE a.candidate_id = 1
ORDER BY a.applied_date DESC
LIMIT 20 OFFSET 0;
