create table if not exists vacancy_tags (
    vacancy_id bigint not null references vacancies(id) on delete cascade,
    tag_id bigint not null references tags(id) on delete cascade,
    primary key (vacancy_id, tag_id)
);

create index if not exists idx_vacancy_tags_vacancy_id on vacancy_tags(vacancy_id);
create index if not exists idx_vacancy_tags_tag_id on vacancy_tags(tag_id);

