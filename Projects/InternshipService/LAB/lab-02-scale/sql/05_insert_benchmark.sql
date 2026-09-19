-- Assignment 9: INSERT with/without indexes
DROP TABLE IF EXISTS lab.events_insert CASCADE;
CREATE TABLE lab.events_insert (
    id BIGSERIAL PRIMARY KEY,
    user_id BIGINT NOT NULL,
    event_type VARCHAR(50) NOT NULL,
    payload JSONB,
    created_at TIMESTAMP NOT NULL
);

\echo '=== INSERT 100k WITHOUT indexes ==='
\timing on
INSERT INTO lab.events_insert (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'MESSAGE', '{}'::jsonb, NOW()
FROM generate_series(1, 100000);
\timing off

TRUNCATE lab.events_insert;
CREATE INDEX idx_events_insert_user ON lab.events_insert(user_id);
CREATE INDEX idx_events_insert_created ON lab.events_insert(created_at);
CREATE INDEX idx_events_insert_user_created ON lab.events_insert(user_id, created_at DESC);

\echo '=== INSERT 100k WITH 3 indexes ==='
\timing on
INSERT INTO lab.events_insert (user_id, event_type, payload, created_at)
SELECT (random()*100000)::bigint, 'MESSAGE', '{}'::jsonb, NOW()
FROM generate_series(1, 100000);
\timing off
