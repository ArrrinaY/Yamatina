insert into tags (name, category) values
    ('C#', 0),
    ('ASP.NET Core', 0),
    ('JavaScript', 0),
    ('TypeScript', 0),
    ('React', 0),
    ('PostgreSQL', 0),
    ('Docker', 0),
    ('Git', 0),
    ('Project Management', 1),
    ('Agile', 1),
    ('Scrum', 1),
    ('Business Analysis', 1),
    ('Marketing', 1),
    ('UI/UX Design', 2),
    ('Figma', 2),
    ('Adobe XD', 2),
    ('Graphic Design', 2)
on conflict (name) do nothing;

insert into users (username, email, password, role) values
    ('admin', 'admin@internship-service.com', '$2a$11$placeholder_hash_should_be_here', 'Admin')
on conflict (username) do nothing;

