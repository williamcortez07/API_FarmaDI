# Final engineering report

> Populate only with facts supported by inspected code, command output, audit artifacts, and test results. Delete or mark sections not applicable; never fabricate results.

## 1. Outcome

- **Status:** Completed / Partially completed / Blocked
- **Scope:** [API, endpoint, feature, or components]
- **Summary:** [Concise factual summary]

## 2. Changes made

| Area/file | Change | Reason | Compatibility impact |
|---|---|---|---|
| [path] | [change] | [evidence/problem] | None / compatible / approved migration / blocked |

## 3. Security audit

- **Cloudflare `security-audit` invoked:** Yes / No
- **Audit type:** Complete initial / Focused / Risk-expanded / Fallback checks only
- **Scope actually examined:** [specific paths/components]
- **Confirmed findings:** [finding IDs, severity, evidence, status]
- **Needs validation:** [items and missing evidence]
- **Refuted findings:** [items and basis]
- **Original findings revalidated after fixes:** [details]
- **Limitations:** [sandbox, tools, credentials, coverage, or other limitations]

Do not say “no vulnerabilities exist.” State only what was checked and what the evidence supports.

## 4. Validation results

| Check | Command/tool | Result | Evidence or limitation |
|---|---|---|---|
| Build | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |
| Unit tests | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |
| Integration/API tests | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |
| Static analysis/format | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |
| Security regression tests | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |
| Contract/database checks | [exact command] | Passed / Failed / Not run / Blocked | [output summary] |

## 5. Quality scoring

| Category | Weight | Score / 10 | Evidence | Gate |
|---|---:|---:|---|---|
| Security | 25% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| Functional correctness | 20% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| Tests and regressions | 20% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| Architecture/separation | 15% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| Maintainability/readability | 10% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| Performance/scalability | 10% | [score or not evaluable] | [evidence] | Pass / Fail / Blocked |
| **Weighted global** | **100%** | **[score or not evaluable]** | [calculation] | Pass / Fail / Blocked |

Acceptance requires every category ≥ 9.0, weighted global ≥ 9.0, and no critical blockers. If evidence is insufficient, do not assign a passing score.

## 6. Remaining risks and blockers

- [Finding or limitation, severity, evidence, owner/next step]
- [Unrun tests and reason]
- [Compatibility/deployment considerations]

## 7. Cycle accounting

- **Failed cycles used:** [0–5]
- **Blocked tasks after five cycles:** [list or none]
- **Independent safe tasks continued:** [list or none]

## 8. Git and handoff

- **Initial branch/status:** [observed baseline]
- **Files changed:** [reviewed list]
- **Unrelated/pre-existing changes preserved:** Yes / No / Unable to verify
- **Commit created:** [SHA and message, or “No — acceptance gates not met/not applicable”]
- **Push:** Not performed unless explicitly authorized.
- **Merge:** Not performed automatically.
- **PR:** [only report if actually created by the host]
