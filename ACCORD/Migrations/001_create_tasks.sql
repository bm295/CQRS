-- Initial ACCORD schema. Applied automatically at startup; do not edit after deployment.
create table if not exists public.tasks (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null default auth.uid() references auth.users(id) on delete cascade,
  title text not null check (char_length(trim(title)) between 1 and 200),
  is_complete boolean not null default false,
  created_at timestamptz not null default now()
);
alter table public.tasks enable row level security;
revoke all on public.tasks from anon;
grant select, insert, update, delete on public.tasks to authenticated;
drop policy if exists tasks_owner on public.tasks;
create policy tasks_owner on public.tasks for all to authenticated
  using ((select auth.uid()) = user_id)
  with check ((select auth.uid()) = user_id);
create index if not exists tasks_user_created_idx on public.tasks(user_id, created_at desc);
