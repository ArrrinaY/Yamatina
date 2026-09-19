-- Лабораторная 1: эксперименты EXPLAIN ANALYZE

-- Задание 3-4: поиск по user_id без индекса
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123;

-- Задание 6: B-tree индекс
CREATE INDEX IF NOT EXISTS idx_orders_user_id ON lab.orders(user_id);
ANALYZE lab.orders;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123;

-- Задание 7-8: индекс по status и селективность
CREATE INDEX IF NOT EXISTS idx_orders_status ON lab.orders(status);
ANALYZE lab.orders;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE status = 'PAID';
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE status = 'NEW';

SELECT status, COUNT(*) FROM lab.orders GROUP BY status;

-- Задание 9: range query
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE created_at > NOW() - INTERVAL '7 days';
CREATE INDEX IF NOT EXISTS idx_orders_created_at ON lab.orders(created_at);
ANALYZE lab.orders;
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE created_at > NOW() - INTERVAL '7 days';

-- Задание 12: составной индекс
CREATE INDEX IF NOT EXISTS idx_orders_user_status ON lab.orders(user_id, status);
EXPLAIN ANALYZE SELECT * FROM lab.orders WHERE user_id = 123 AND status = 'PAID';

-- Задание 14-15: сортировка и pagination
EXPLAIN ANALYZE
SELECT * FROM lab.orders
WHERE user_id = 123
ORDER BY created_at DESC
LIMIT 20;

CREATE INDEX IF NOT EXISTS idx_orders_user_created_at_desc ON lab.orders(user_id, created_at DESC);
EXPLAIN ANALYZE
SELECT * FROM lab.orders
WHERE user_id = 123
ORDER BY created_at DESC
LIMIT 20;

-- Задание 21: финальная оптимизация
EXPLAIN ANALYZE
SELECT id, amount, status, created_at
FROM lab.orders
WHERE user_id = 123
  AND status = 'PAID'
  AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC
LIMIT 50;

CREATE INDEX IF NOT EXISTS idx_orders_user_status_created_at
ON lab.orders(user_id, status, created_at DESC);
ANALYZE lab.orders;
EXPLAIN ANALYZE
SELECT id, amount, status, created_at
FROM lab.orders
WHERE user_id = 123
  AND status = 'PAID'
  AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC
LIMIT 50;
