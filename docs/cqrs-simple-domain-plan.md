# CQRS Web Application Implementation Plan for Platform / Infrastructure Systems

## 1) Goal

Implement a **web application** that uses CQRS to manage infrastructure change requests end-to-end:
- reliable write workflows (commands)
- fast operational read views (queries)
- clear separation of write and read concerns in code and API

## 2) Business Domain

Domain: **Infrastructure Change Management**.

Primary aggregate: `InfrastructureChangeRequest`.

Use case: platform teams create, approve, schedule, execute, and track changes for services/systems across environments.

## 3) Web Application Scope (MVP)

### User-facing capabilities
- Create a change request from a form.
- View request details and current status.
- Approve/reject pending requests.
- Schedule approved requests.
- Mark execution started/completed/failed.
- View operational dashboards (pending approvals, scheduled changes, failed changes).

### CQRS capabilities
- Write-side handlers process all state-changing operations.
- Read-side handlers serve all list/detail/dashboard views.
- Web UI calls command endpoints for mutations and query endpoints for reads.

## 4) Solution Structure (Web + CQRS)

Recommended project layout:

- `WebApplication/Domain`
  - Aggregate, value objects, status enum, domain rules
- `WebApplication/Application/Commands`
  - Command contracts + command handlers
- `WebApplication/Application/Queries`
  - Query contracts + query handlers + DTOs
- `WebApplication/Infrastructure/Persistence`
  - Write repository implementations
- `WebApplication/Infrastructure/ReadModels`
  - Projection store + query persistence
- `WebApplication/Infrastructure/Messaging`
  - Domain event dispatcher/outbox (phase 2)
- `WebApplication/Controllers`
  - HTTP endpoints (command and query routes)
- `WebApplication/Views`
  - Razor pages for forms, detail pages, and dashboards

## 5) Data Models

### Write model: `InfrastructureChangeRequest`
- `Id: Guid`
- `Title: string`
- `SystemName: string`
- `Environment: string` (`dev`/`staging`/`prod`)
- `RequestedBy: string`
- `ApprovedBy: string?`
- `ScheduledAtUtc: DateTime?`
- `Status: ChangeStatus` (`Draft`, `PendingApproval`, `Approved`, `Scheduled`, `InProgress`, `Completed`, `Failed`, `Rejected`)
- `FailureReason: string?`
- `CreatedAtUtc: DateTime`
- `UpdatedAtUtc: DateTime`

### Read models
- `ChangeRequestDetailsVm`
- `PendingApprovalVm`
- `ScheduledChangeVm`
- `FailedChangeVm`
- `OpsSummaryVm`

## 6) Commands, Queries, and HTTP Mapping

### Commands
- `CreateChangeRequestCommand`
- `SubmitForApprovalCommand`
- `ApproveChangeRequestCommand`
- `RejectChangeRequestCommand`
- `ScheduleExecutionCommand`
- `MarkExecutionStartedCommand`
- `MarkExecutionCompletedCommand`
- `MarkExecutionFailedCommand`

Command endpoints:
- `POST /infra/changes`
- `POST /infra/changes/{id}/submit`
- `POST /infra/changes/{id}/approve`
- `POST /infra/changes/{id}/reject`
- `POST /infra/changes/{id}/schedule`
- `POST /infra/changes/{id}/start`
- `POST /infra/changes/{id}/complete`
- `POST /infra/changes/{id}/fail`

### Queries
- `GetChangeRequestByIdQuery`
- `ListPendingApprovalsQuery`
- `ListScheduledChangesQuery`
- `ListFailedChangesQuery`
- `GetOperationalChangeSummaryQuery`

Query endpoints:
- `GET /infra/changes/{id}`
- `GET /infra/approvals/pending`
- `GET /infra/changes/scheduled`
- `GET /infra/changes/failed`
- `GET /infra/changes/summary`

## 7) Web UI Pages / Screens

### Pages
- `/infra/changes/new`
  - Form for creating a new change request.
- `/infra/changes/{id}`
  - Details + action buttons based on current status.
- `/infra/approvals`
  - Table of pending approvals.
- `/infra/schedule`
  - Upcoming scheduled changes.
- `/infra/failures`
  - Recent failed changes.
- `/infra/dashboard`
  - Summary cards + operational lists.

### UI behavior
- Buttons map to command endpoints.
- Page loads and lists use query endpoints only.
- Disable/hidden actions when transitions are invalid for current status.

## 8) State Transition Rules (Write Side)

- `Draft -> PendingApproval` via `SubmitForApproval`
- `PendingApproval -> Approved` via `Approve`
- `PendingApproval -> Rejected` via `Reject`
- `Approved -> Scheduled` via `ScheduleExecution`
- `Scheduled -> InProgress` via `MarkExecutionStarted`
- `InProgress -> Completed` via `MarkExecutionCompleted`
- `InProgress -> Failed` via `MarkExecutionFailed`

All transitions enforced in aggregate methods.

## 9) Implementation Plan (Web Application)

### Phase 1: Build running CQRS web MVP
1. Add domain aggregate + status transitions.
2. Implement command/query contracts and handlers.
3. Use in-memory write repository + in-memory read store.
4. Add MVC controllers for command and query endpoints.
5. Build Razor views/pages for forms, tables, and dashboard.
6. Register dependencies in `Program.cs`.
7. Add basic validation and user feedback messages.

### Phase 2: Event-driven projections
1. Emit domain events on each write-side transition.
2. Add in-process projection handlers.
3. Update read models asynchronously from events.
4. Show projection freshness timestamp in dashboard.

### Phase 3: Production-ready web platform
1. Replace in-memory stores with persistent databases.
2. Add outbox for reliable event publication.
3. Add authentication + role-based authorization.
4. Add idempotency for command submissions.
5. Add observability (logs, metrics, traces, alerts).

## 10) Testing Plan

### Unit tests
- Aggregate transition rules and invariant checks.
- Command handler success/failure cases.
- Query handler filtering and DTO mapping.

### Integration tests
- Endpoint tests for all command routes.
- Endpoint tests for all query routes.
- Full workflow: create -> approve -> schedule -> start -> complete/fail.

### UI tests (basic)
- Form submit success/error handling.
- Status-based button visibility.
- Dashboard page data rendering.

## 11) Definition of Done (MVP)

- Web pages for create/detail/approvals/schedule/failures/dashboard are implemented.
- Command and query pipelines are separated in code.
- Domain enforces all listed transition rules.
- Query pages are served from read models only.
- Automated unit + integration tests pass for core flows.
- README updated with run steps and CQRS web architecture notes.

## 12) Suggested Enhancements

- Pagination, sorting, and search on list pages.
- Optimistic concurrency token on write model.
- Incident integration when change execution fails.
- Notification hooks (email/Slack/webhook) for approvals and failures.
