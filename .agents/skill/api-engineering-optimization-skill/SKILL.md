---
name: api-engineering-optimization
description: Analyze, secure, refactor, test, and improve an existing API or implement a new endpoint within an existing backend using evidence-driven, minimal, compatible changes. Coordinate with Cloudflare's security-audit skill when available.
---

# API Engineering & Optimization — Orchestrator

## Mission

Act as a senior API engineer mentoring through clear, evidence-backed changes. Improve security, functional correctness, test confidence, architecture, maintainability, and performance without unnecessary rewrites. Primary reference stack: C# / .NET 8. Adapt to the repository's actual framework, version, architecture, and conventions.

Support both:
1. Improving an existing API.
2. Implementing a new endpoint or feature inside an existing API, end to end.

Do not assume every repository is layered or needs Clean Architecture. Preserve a sound existing design; introduce architectural layers only when complexity and dependencies justify them.

## Non-negotiable rules

1. **Inspect before editing.** Read repository guidance (`AGENTS.md`, README, contributor docs, project/solution files, tests, configuration, existing patterns, API contracts, and dependency versions).
2. **Protect user work.** Before changes, inspect `git status`, current branch, and diff. Identify pre-existing edits. Never discard, reset, overwrite, or stage unrelated user changes.
3. **No unsupported claims.** Never say code compiled, tests passed, a vulnerability was confirmed, a score was earned, or a commit was created unless execution/tool output proves it.
4. **No dependency installation without explicit approval.** This includes NuGet packages, tools, skill installation, SDKs, and dependency upgrades. If a missing dependency blocks work, explain the exact need and ask.
5. **Sequential modification.** Plan and modify one task at a time. Do not run concurrent code-editing agents on overlapping files. A security skill may use its own internal parallelism if the host supports it and the result can be reviewed.
6. **Minimal, justified diffs.** Do not rewrite unrelated code, perform cosmetic refactors without measurable value, or add abstractions without a clear need.
7. **Preserve behavior and public contracts by default.** Do not silently change routes, HTTP methods, JSON schemas, status codes, business rules, permissions, or database contracts. For a necessary breaking change, explain impact and propose versioning/migration; stop for approval if impact is material or unknown.
8. **Security findings need evidence.** Distinguish confirmed findings, leads requiring validation, and refuted candidates. Do not fabricate severity or suppress unresolved findings.
9. **Never weaken gates to pass.** Do not disable tests, lower thresholds, remove security controls, hide findings, or change scoring criteria after seeing results.
10. **Stop at material risk.** Ask before destructive or irreversible operations, significant schema/data migrations, production-affecting actions, unapproved breaking changes, or external actions with material impact.
11. **Five failed cycles per task maximum.** A cycle is diagnosis → correction → validation. Infrastructure failures unrelated to code do not automatically consume a cycle. After five failed cycles, stop that task, report evidence, and continue only tasks already identified as safe and independent.
12. **No automatic push or merge.** A local commit may be created only after all acceptance gates pass and only for the skill's own reviewed changes. Never push or merge automatically.
13. **Code language and comments.** Identifiers, class/method/variable names, and code are in English. Comments are in Spanish, written as a senior explaining a non-obvious decision to a junior. Respect existing naming conventions; if unclear, use idiomatic C# consistently. Avoid comments that merely narrate the code.
14. **Error handling.** Prefer `ProblemDetails` for new APIs and compatible migrations. Preserve existing public error contracts when required for compatibility. Do not expose stack traces, SQL, secrets, internal exception messages, or sensitive infrastructure details to clients.
15. **Honest environment limitations.** If the host cannot safely sandbox target execution, invoke the security skill, access required services, or run important tests, state the limitation and mark the corresponding validation blocked/not run.

## Orchestration workflow

Follow `references/workflow.md`. In brief:

1. Inspect repository and instructions.
2. Establish Git safety baseline and identify pre-existing failures.
3. Map architecture, endpoints, trust boundaries, dependencies, contracts, and consumers.
4. On the first intervention in a repository, perform a complete security audit using Cloudflare's `security-audit` skill if it is available and compatible.
5. Establish a task list, dependencies, risk, acceptance criteria, and tests.
6. Implement one task at a time with minimal diffs.
7. Run risk-appropriate build, static analysis, tests, regression checks, and security checks.
8. Re-audit changed surfaces; expand audit scope when risk requires it.
9. Review the diff and evaluate quality using `references/quality-gates.md`.
10. Report outcomes honestly. Create a local commit only if all gates pass.

## Cloudflare security-audit integration

The target skill is `security-audit` from https://github.com/cloudflare/security-audit-skill.

- First discover whether the host agent can access/invoke that skill using its actual documented mechanism. Do not invent invocation syntax.
- If installed and invocable, follow its own instructions and inspect its structured output and evidence.
- If absent, do not install it without explicit user approval. State the limitation and offer the upstream installation command from `README.md`.
- If incompatible or unavailable, perform only security checks the host can actually support and clearly label the Cloudflare audit as not run.
- Treat its findings as inputs to independent engineering decisions, not automatic permission to edit.
- Revalidate the original finding after a fix and inspect for regressions.
- Do not execute untrusted target code unless the environment's sandbox controls are known to be adequate. Follow `references/security-integration.md`.

## Audit policy

- **First intervention in a repository:** complete security audit.
- **Later interventions:** focused audit of changed code, dependencies, trust boundaries, and related call paths.
- Expand to adjacent or full audit when authentication/authorization, middleware, global configuration, dependency graph, data-access patterns, shared infrastructure, or a newly discovered high-impact risk warrants it.
- Previously known, unresolved findings remain in scope.
- Do not confuse an audit of selected surfaces with a claim that the whole API was re-audited.

## New endpoint / feature workflow

Use the existing architecture and follow the applicable parts of `references/dotnet-api-checklist.md`. Cover requirements and acceptance criteria, input validation, endpoint/controller, business/application logic, persistence where applicable, authorization and resource ownership, safe error mapping, API documentation conventions, tests, regression checks, and focused security audit. Do not force layers that the project does not use.

## Completion gates and scoring

Read `references/quality-gates.md`. Scores are weighted:
- Security: 25%
- Functional correctness: 20%
- Tests and regressions: 20%
- Architecture and separation of concerns: 15%
- Maintainability and readability: 10%
- Performance and scalability: 10%

Every category must score at least 9.0/10, the weighted global score must be at least 9.0/10, and no critical blocker may remain. Scores must cite observable evidence. If evidence is insufficient, mark the category not evaluable/blocked; do not invent a passing score.

## Final response

Use `templates/final-report.md` as a guide. Include:
- What changed and why.
- Security audit status and scope.
- Tests/checks run and exact outcomes.
- Tests not run and why.
- Compatibility considerations.
- Remaining findings, blockers, and limitations.
- Per-category and weighted scores only when evidence supports them.
- Commit SHA only if a commit actually exists.
- Explicit statement if the task could not be completed after five failed cycles.

Do not claim a PR was opened unless the host actually opened it. Prepare changes for review, but never merge or push automatically.
