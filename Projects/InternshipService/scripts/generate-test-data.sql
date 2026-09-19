-- Массовая генерация тестовых данных для InternshipService
-- psql -h localhost -p 54320 -U postgres -d postgres -f scripts/generate-test-data.sql

INSERT INTO companies (name, description, website, email, phone)
SELECT
    'Company ' || gs,
    'Generated company #' || gs,
    'https://company' || gs || '.example.com',
    'company' || gs || '@example.com',
    '+7-900-' || lpad(gs::text, 7, '0')
FROM generate_series(1, 1000) AS gs;

INSERT INTO candidates (first_name, last_name, email, phone, skills, experience_level)
SELECT
    'Candidate' || gs,
    'Test' || gs,
    'candidate' || gs || '@example.com',
    '+7-901-' || lpad(gs::text, 7, '0'),
    'C#, PostgreSQL, Docker',
    (gs % 3)
FROM generate_series(1, 10000) AS gs;

INSERT INTO vacancies (title, description, requirements, salary_range, type, location, is_active, company_id)
SELECT
    'Vacancy ' || gs,
    'Generated vacancy #' || gs,
    'Requirements #' || gs,
    (50000 + (random() * 150000)::int)::text || ' RUB',
    (gs % 2),
    CASE (gs % 4) WHEN 0 THEN 'Moscow' WHEN 1 THEN 'Saint Petersburg' WHEN 2 THEN 'Remote' ELSE 'Kazan' END,
    (gs % 10) <> 0,
    (SELECT id FROM companies ORDER BY random() LIMIT 1)
FROM generate_series(1, 5000) AS gs;

INSERT INTO applications (candidate_id, vacancy_id, status, applied_date, cover_letter)
SELECT
    c.id,
    v.id,
    floor(random() * 4)::int,
    timestamp '2025-01-01' + (gs * interval '5 minutes'),
    'Generated application #' || gs
FROM generate_series(1, 100000) AS gs
JOIN LATERAL (
    SELECT id FROM candidates
    OFFSET floor(random() * GREATEST((SELECT COUNT(*) FROM candidates), 1)) LIMIT 1
) c ON true
JOIN LATERAL (
    SELECT id FROM vacancies
    OFFSET floor(random() * GREATEST((SELECT COUNT(*) FROM vacancies), 1)) LIMIT 1
) v ON true;

ANALYZE companies;
ANALYZE candidates;
ANALYZE vacancies;
ANALYZE applications;

SELECT 'companies' AS tbl, COUNT(*) FROM companies
UNION ALL SELECT 'candidates', COUNT(*) FROM candidates
UNION ALL SELECT 'vacancies', COUNT(*) FROM vacancies
UNION ALL SELECT 'applications', COUNT(*) FROM applications;
