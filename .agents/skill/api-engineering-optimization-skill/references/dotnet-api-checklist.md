# C# / .NET API checklist

Use this checklist selectively according to the repository's architecture and task risk. Do not force every item into every project.

## Repository and framework

- Confirm target framework and SDK from `.csproj`, `global.json`, and CI.
- Check nullable reference settings, analyzers, formatting, test framework, dependency versions, and existing conventions.
- Do not assume .NET 8 if the repository targets another version; .NET 8 is the reference baseline, not permission to retarget.
- Do not install or update NuGet packages without explicit approval.

## Endpoint and contracts

- Match the project's controller or Minimal API pattern.
- Preserve route, HTTP verb, parameter names, JSON shape, status codes, and documented semantics unless a justified, approved change is required.
- Validate request shape, bounds, formats, and business invariants at the appropriate boundaries.
- Use explicit DTOs; avoid exposing persistence entities directly when that would couple the public contract to storage.
- Avoid over-posting/mass assignment; allowlist writable properties.
- Return appropriate status codes and consistent error contracts.
- Update OpenAPI/Swagger documentation when the project already uses it.
- Consider pagination, filtering, ordering, maximum page sizes, and resource limits for collection endpoints.

## Authentication and authorization

- Determine the existing authentication scheme before changing it.
- For JWT bearer validation, inspect signature validation, allowed algorithms, issuer, audience, lifetime/expiration, clock skew, and key handling as appropriate to the library and deployment.
- Do not confuse authentication with authorization.
- Apply endpoint policies/roles and resource-level ownership checks where required.
- Never rely solely on client-provided user, tenant, account, or resource identifiers.
- For refresh-token systems, assess entropy, secure storage (prefer hashed storage when suitable), rotation, reuse detection, revocation, expiration, and replay risks according to the existing design.
- Never log bearer tokens, passwords, refresh tokens, signing keys, or secrets.
- Avoid inventing a new identity architecture when a sound one exists.

## Business and application logic

- Keep business invariants in the established business/application layer rather than duplicating them in controllers and repositories.
- Avoid putting persistence and complex business rules into endpoint handlers if the project already separates those concerns.
- Use typed results or domain-specific exceptions where consistent with the existing architecture.
- Avoid broad catch blocks that swallow exceptions or return success-like defaults.
- Ensure side effects and transaction boundaries are explicit.
- Pass cancellation tokens through supported async call chains when appropriate.
- Avoid `.Result`, `.Wait()`, and sync-over-async in asynchronous request paths.
- Do not change business rules just to make tests pass; clarify ambiguous requirements.

## Data access and transactions

- Use parameterized queries or safe ORM APIs; never concatenate untrusted input into SQL.
- Check tenant/resource filters and authorization in the actual data-access path when required.
- Evaluate transaction scope, atomicity, concurrency, duplicate submissions, and idempotency for writes.
- Avoid N+1 query patterns and unbounded result sets where relevant.
- Do not add migrations, drop/rename columns, or perform destructive data changes without explicit approval and a rollback/compatibility plan.
- Do not add redundant fields or duplicate business logic without a demonstrated need.

## Errors, logs, and observability

- Prefer `ProblemDetails` for new APIs and compatible migrations.
- Preserve existing error response contracts when compatibility requires it.
- Map known validation, not-found, conflict, authentication, authorization, and business errors to suitable HTTP responses.
- Do not expose stack traces, raw exception text, SQL, internal hostnames, secrets, or sensitive values to clients.
- Log useful diagnostics with correlation/context where available, while redacting sensitive data.
- Do not log secrets or unnecessarily collect personal data.
- Avoid catching exceptions merely to log and rethrow repeatedly at every layer.

## Reliability and performance

- Bound request sizes, collection sizes, execution time, and concurrency where appropriate.
- Consider cancellation, timeouts, retry policies, and idempotency for external dependencies.
- Avoid retrying non-idempotent operations without a safe strategy.
- Check async I/O, database query shape, indexes based on evidence, and resource lifetime.
- Avoid premature caching or complexity without a measured or well-supported need.
- Treat rate limiting and abuse controls according to endpoint exposure and risk.

## Testing

- Unit-test changed business logic and important boundaries.
- Test valid inputs, invalid inputs, boundary values, and relevant error paths.
- Test authentication and authorization failures, including access to another user's/tenant's resource where applicable.
- Add regression tests for fixed vulnerabilities and previously observed bugs where feasible.
- Integration-test route binding, serialization, DI, database interactions, and middleware where relevant.
- Validate public response contracts and status codes.
- Run existing build, tests, analyzers, and format checks where available.
- Clearly identify tests that were not run and why.

## New endpoint acceptance checklist

Before calling the endpoint complete, verify all applicable items:
- [ ] Requirements and acceptance criteria are explicit.
- [ ] Existing patterns and contracts were inspected.
- [ ] Request validation and business invariants are enforced.
- [ ] Authentication and authorization are correct.
- [ ] Resource ownership/tenant isolation is checked where applicable.
- [ ] Data access is safe and appropriately bounded.
- [ ] Error mapping matches the API's contract.
- [ ] No secret or internal detail leaks through response/logs.
- [ ] Tests cover expected behavior, failure paths, and regressions.
- [ ] API documentation follows existing conventions.
- [ ] Focused security audit is complete, with scope stated.
- [ ] Diff contains no unrelated changes.
