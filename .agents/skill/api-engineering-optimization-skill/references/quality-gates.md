# Quality gates and evidence-based scoring

## Acceptance rule

A task is accepted only if all conditions hold:
1. Weighted global score is at least 9.0/10.
2. Each of the six category scores is at least 9.0/10.
3. No critical blocker remains.
4. Evidence is sufficient for every passing score.
5. Required validations have passed, or any permitted non-applicable checks are explicitly justified.

A blocked or not-evaluable category is not a passing category. Do not substitute a guessed score. If a required gate is blocked, report the task as blocked rather than accepted.

## Weights

| Category | Weight |
|---|---:|
| Security | 25% |
| Functional correctness | 20% |
| Tests and regressions | 20% |
| Architecture and separation of concerns | 15% |
| Maintainability and readability | 10% |
| Performance and scalability | 10% |
| **Total** | **100%** |

Formula: `global = Σ(category_score × category_weight)`, with weights expressed as fractions. Keep the rubric and weights fixed before evaluating results.

## Evidence rubric

Use a 0–10 scale with concrete observations, not intuition. A score of 9 or 10 requires strong evidence and no material known gap within the evaluated scope.

### Security — 25%

Inspect confirmed and unresolved findings, authentication and authorization, resource ownership, input/data boundaries, secret handling, safe errors/logging, and the relevant audit evidence.

- 9–10: no known critical/high issue in scope; relevant controls are verified; addressed findings are retested; residual risks are documented.
- 7–8: material control or validation gaps remain, or evidence is incomplete.
- 0–6: serious vulnerability remains, security controls are broken, or evidence shows unsafe behavior.
- Blocked/not evaluable: required security environment or audit is unavailable and no adequate alternative evidence exists.

A single unresolved critical vulnerability is a blocker regardless of the category score.

### Functional correctness — 20%

Use acceptance criteria, expected behavior, boundary cases, error paths, and regression results.

- 9–10: behavior matches explicit requirements and applicable tests pass; no known unintended contract/business-rule changes.
- 7–8: relevant edge cases or behavior remain unverified, or a non-critical regression is unresolved.
- 0–6: significant incorrect behavior or a confirmed regression.
- Blocked/not evaluable: required behavior cannot be tested or determined.

### Tests and regressions — 20%

Evaluate whether tests target changed behavior and risks, whether they actually ran, and whether regression coverage is adequate. Do not use raw coverage percentage alone as proof of quality.

- 9–10: relevant tests exist and pass; meaningful security/regression cases are covered; build and required checks pass.
- 7–8: useful tests pass but important edge cases or integration paths are missing.
- 0–6: tests fail due to the change, no meaningful verification exists for risky changes, or tests were weakened.
- Blocked/not evaluable: test infrastructure or required services are unavailable.

### Architecture and separation — 15%

Inspect dependency direction, cohesive responsibilities, consistency with repository patterns, and whether new abstractions solve demonstrated problems.

- 9–10: changes fit the architecture, isolate responsibilities appropriately, and avoid unnecessary abstractions.
- 7–8: moderate coupling, duplication, or boundary confusion remains.
- 0–6: significant architectural regression or unsafe coupling.
- Do not penalize a simple architecture merely because it is not Clean Architecture.

### Maintainability and readability — 10%

Inspect naming, duplication, complexity, error paths, consistency, useful comments, and diff scope.

- 9–10: focused, readable, consistent change with no known material duplication or confusing logic in scope.
- 7–8: some avoidable complexity or inconsistency remains.
- 0–6: fragile, opaque, duplicated, or unnecessarily broad changes.

### Performance and scalability — 10%

Use measurements when possible and reason from actual query patterns, allocations, blocking I/O, concurrency, caching, and resource usage. Do not optimize based on unsupported guesses.

- 9–10: no known material regression; relevant expensive paths are validated or justified.
- 7–8: potential bottleneck remains without sufficient measurement, or modest inefficiency is known.
- 0–6: material performance regression or clearly unsafe scaling behavior.
- Blocked/not evaluable: meaningful performance claims cannot be tested; do not invent benchmarks.

## Critical blockers

Examples include:
- Confirmed critical vulnerability still exploitable.
- High-impact authorization bypass or cross-tenant/resource exposure.
- Destructive or irreversible data change without authorization and recovery plan.
- Serious regression to a public contract or business invariant.
- Required tests were disabled or weakened to obtain a pass.
- Build or required tests fail due to the changes.
- Evidence is misrepresented, or a required security audit is falsely claimed to have run.

Assess severity in context. Do not label every theoretical defense-in-depth gap as critical.

## Failed-cycle protocol

A cycle is diagnosis → correction → validation. Stop a task after five failed cycles. Infrastructure failures unrelated to the code do not automatically consume a cycle. At the limit, document the exact evidence and stop that task; continue only work already identified as safe and independent.

## Commit gate

Create a local commit only after:
- All applicable required tests/checks pass.
- No critical blockers remain.
- All category scores and global score meet thresholds with evidence.
- The final diff contains only reviewed task changes and does not include unrelated user work.

Do not push or merge automatically. If gates are blocked or failed, report that no validated commit was created.
