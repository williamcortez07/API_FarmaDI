# Workflow and decision protocol

## 1. Repository reconnaissance

Inspect, where present:
- `AGENTS.md`, README, contributing docs, architecture notes, CI workflows, and local conventions.
- `.sln`, `.csproj`, `global.json`, `Directory.Build.*`, `Directory.Packages.props`, `NuGet.config`, lock files, and test projects.
- Endpoint/controller/minimal API patterns, middleware, dependency injection, service/repository abstractions, validators, error handling, authentication and authorization.
- Database schemas, migrations, stored procedures, transaction boundaries, and persistence patterns relevant to the task.
- API contracts, OpenAPI/Swagger configuration, integration consumers, and tests.
- Security audit artifacts and previous findings, if present. Check whether evidence still applies to current code.

Do not dump secrets into reports. When inspecting configuration, redact secret values and avoid printing environment values unnecessarily.

## 2. Git safety baseline

Before editing:
1. Run `git status --short --branch`.
2. Determine the current branch and relevant diff, including staged changes.
3. Identify untracked files and pre-existing user edits.
4. Record baseline build/test failures where practical.
5. Never use destructive cleanup commands or revert unrelated work.
6. Stage only explicitly reviewed files belonging to this task; do not stage the entire repository when unrelated changes exist.

If Git is unavailable, state this and do not pretend the baseline was verified.

## 3. Map behavior and risk

For the target feature, document internally:
- Inputs and trust boundaries.
- Call path and dependencies.
- Authentication and authorization checks.
- Resource ownership/tenant boundaries where applicable.
- Data writes, transactions, side effects, external calls, and failure modes.
- Public contracts and likely consumers.
- Current tests and coverage gaps.
- Whether the task is security-sensitive, data-destructive, compatibility-sensitive, or performance-sensitive.

Separate pre-existing defects from defects introduced by the planned changes.

## 4. Plan before editing

Write a compact task plan with:
- Specific problem and evidence.
- Smallest viable correction.
- Files/components expected to change.
- Risk and possible compatibility effects.
- Tests and acceptance criteria.
- Dependencies between tasks.
- Whether the change requires user approval.

Work sequentially. If a finding is uncertain, validate it before making a speculative change. Do not alter business rules merely to satisfy a test; investigate the intended behavior.

## 5. Implement

- Match established patterns unless those patterns are demonstrably unsafe or defective.
- Prefer cohesive changes and clear names over generic abstractions.
- Keep input validation at trust boundaries and business invariants in the appropriate layer.
- Use parameterized database access and safe serialization.
- Apply least privilege and explicit authorization.
- Avoid broad exception swallowing, empty catches, duplicated business logic, hidden side effects, and unnecessary synchronous I/O in asynchronous paths.
- Use cancellation tokens where the existing stack and call chain support them.
- Avoid introducing breaking changes or unrelated formatting churn.
- Do not install packages or upgrade dependencies without approval.
- For security fixes, add a regression test that would fail before the fix where feasible.

## 6. Validate after each task

Run the narrowest relevant tests first, then broader checks based on risk:
- Formatting/style checks already configured by the repository.
- Build/compile.
- Unit tests.
- Integration/API tests with the required dependencies.
- Authorization and input-validation regression tests.
- Static analyzers and dependency/security scans already available.
- Relevant database tests and migration validation.
- API contract/OpenAPI checks where configured.
- Diff review and search for unintended changes.

Use existing scripts and repository-approved tooling. Do not assume a command exists; inspect project configuration or available tool output first. Do not install tools without approval.

If a check fails:
1. Capture the exact error and identify whether it is code-related, baseline, or environmental.
2. Fix the cause when within scope and safe.
3. Re-run the failed check and relevant regression checks.
4. Count one failed cycle only when a diagnosis/correction/validation attempt fails to resolve the task; unrelated infrastructure outage does not automatically consume a cycle.

## 7. Re-audit and expand scope

After code changes:
- Revalidate each security finding addressed.
- Inspect related routes, middleware, shared services, and data access for regressions.
- Use focused audit for local changes.
- Expand scope when the change affects shared security controls, auth, authorization, configuration, dependency graph, persistence patterns, or broader trust boundaries.
- Do not claim a full audit was rerun when only a focused audit ran.

## 8. Five-cycle stop rule

After five failed cycles for a task:
- Stop trying the same task.
- Summarize attempted changes, commands, observed errors, and remaining uncertainty.
- Mark the task blocked/not completed.
- Continue only tasks already identified as safe and independent.
- Do not create a passing score or success claim for the blocked task.

## 9. Git and delivery

Before any commit:
- Review `git diff` and `git diff --cached`.
- Confirm no unrelated or pre-existing user changes are staged.
- Ensure applicable gates pass.
- Stage only task-owned files.
- Create a local commit only when gates pass.
- Do not push, merge, or open a PR unless explicitly requested and the host provides the capability.
- If gates do not pass, leave changes available for inspection and explain why no validated commit was made.
