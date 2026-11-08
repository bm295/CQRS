# Change Advisor MVP

Inspired by Adi Polak, [Systemic Approach to Memory, Knowledge, and Context in Agentic AI Architectures](https://www.infoq.com/articles/agentic-ai-context-memory-knowledge/) (2026-11-02).

Run with .NET 10: `dotnet run --project WebApplication`. Open `/infra/dashboard`, create a change, then choose **AI Change Advisor** in its details page.

1. Save description, implementation, rollback and verification plans.
2. Publish a team-approved policy for the exact system/environment.
3. Analyze. Review findings, missing information, checklist and source snapshots.
4. Complete or fail a change, save a draft lesson, then explicitly confirm it.
5. Create another change for the same system/environment: analysis includes up to three confirmed, unexpired lessons. Restart the app to verify persistence.
6. Update a policy, plan or change status: earlier advice is marked outdated.

## Architecture

Knowledge is scoped, versioned policy text. Memory consists of explicitly confirmed lessons, with a 90-day expiry. Context combines the current persisted change and plan, all applicable policies and up to three recent valid lessons. The UTF-8 serialized context has a conservative 12,000-byte ceiling; mandatory content exceeding it blocks inference instead of silently losing policies. Optional lessons are omitted if they do not fit. This bounds input size rather than claiming an exact tokenizer count.

MediatR commands create analyses; queries return latest analysis and staleness. Metadata form endpoints persist plans/policies/lessons. Each analysis stores exact input, output, source identifiers, mode and a SHA-256 source fingerprint. Changes, plans, policies, lessons and analyses persist in SQLite (`App_Data/advisor.db`), while operational read projections rebuild at startup. Existing transitions still enforce domain rules. Staleness is checked on read, including a one-hour freshness ceiling; concurrent changes during model inference leave a reviewable, stale snapshot.

## Model configuration

Default `rules` mode is a conservative deterministic checklist, not LLM inference and not a semantic policy evaluator. It never declares deployment authorized. It enables the full memory/context workflow without credentials.

For a service exposing the chat-completions contract, set `Advisor__Protocol=chat-completions`, `Advisor__Model` to its model identifier, `Advisor__Endpoint` to the complete HTTPS chat-completions URL, and `Advisor__ApiKey` via environment secrets. The provider must support JSON response mode. This adapter sends the curated context directly to the model and validates the returned evidence identifiers.

For a custom advisor service, leave `Advisor__Protocol` unset. For model inference set `Advisor__Endpoint=https://your-advisor-service/analyze` and optionally `Advisor__ApiKey` via environment/configuration secrets. The provider-neutral adapter POSTs `{instructions,context}` and expects JSON with `readiness` (string), `findings`, `missingInformation`, `checklist`, `evidenceSources` (string arrays). Evidence identifiers must belong to the supplied context. The HTTPS service must implement its chosen model API; this endpoint is not directly interchangeable with arbitrary vendor completion endpoints. Timeout is 30 seconds; provider failure produces a retry message and does not persist a successful analysis. Never commit keys.

## Boundaries

This retains the existing app's **single trusted workspace** model: there is no user authentication or tenant-specific access control in the running web application. System/environment filters are relevance filters, not authorization. The separate retailer interceptor library does not protect AdvisorDb. Run locally for this MVP; authenticated identity, role-based publishing/confirmation and tenant scoping across every query are prerequisites for a shared deployment.

Forms enforce antiforgery and field limits. Provider output is schema/evidence-identifier checked and Razor encoded; these checks do not prove semantic correctness. Humans must verify evidence, policy precedence and advice. The advisor cannot invoke approval or execution commands. No live telemetry, multi-agent loop, vector database, automated extraction or automatic execution is included.

`EnsureCreated` bootstraps a new database; adopt versioned migrations before evolving an existing deployed schema. Single-process read projections and SQLite are MVP choices; durable outbox/event history and multi-instance projection consistency remain future work. Back up the configured SQLite file.

Validation: `dotnet test WebApplication.Tests/WebApplication.Tests.csproj`. Tests cover restart persistence, staleness on policy/state changes, scoped retrieval, lesson confirmation/expiry and mandatory context overflow.

HTTP smoke: start the app on `http://127.0.0.1:5187`, then run `python scripts/advisor-smoke.py` against a disposable database. Set `ADVISOR_SMOKE_URL` to override the base URL. It checks real antiforgery forms, create/plan/policy/analyze and stale-result rendering.
