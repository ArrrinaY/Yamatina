-- Параметр: измените upper bound generate_series для нужного объёма
-- 10000, 100000, 1000000, 5000000

TRUNCATE lab.events RESTART IDENTITY;

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
FROM generate_series(1, 1000000);

ANALYZE lab.events;

SELECT COUNT(*) AS row_count FROM lab.events;
