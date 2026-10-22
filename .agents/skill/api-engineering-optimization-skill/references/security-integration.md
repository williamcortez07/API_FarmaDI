# Cloudflare security-audit integration

## Target

Upstream repository: https://github.com/cloudflare/security-audit-skill  
Expected skill name: `security-audit`

The upstream methodology describes architecture reconnaissance, attack-surface mapping, coverage tracking, candidate discovery, independent validation, structured findings, and derived reports. Use the upstream skill's own current instructions and schemas as authoritative for its output format.

## Invocation discovery

1. Use the host agent's documented skill-discovery mechanism.
2. Confirm that the skill is installed, readable, and invocable in this session.
3. Follow the exact invocation format documented by the host and the skill. Do not guess a slash command, tool name, or argument schema.
4. If the skill is absent, explain that it is unavailable and request explicit approval before installing it.
5. Do not install the Skills CLI, Node.js, packages, or other tools without approval.

The upstream README documents this example installation command:

```bash
npx skills add https://github.com/cloudflare/security-audit-skill --skill security-audit
```

This is a reference only. Do not execute it automatically. Confirm host compatibility and installation scope, and obtain user approval first.

## Required audit policy

### First intervention in a repository: complete audit

Attempt a complete initial security audit, subject to the skill's supported scope and safe execution environment. Map:
- Architecture and trust boundaries.
- Externally reachable endpoints and input surfaces.
- Authentication, authorization, roles/policies, and resource ownership.
- Data access, serialization, file handling, external calls, and privileged operations.
- Security-relevant configuration and dependencies.
- Existing audit artifacts and their freshness.

Record scope and limitations. A completed scan is not proof that every possible vulnerability was found.

### Later interventions: focused audit, risk-expanded

Audit changed code and its relevant call paths, including tests and dependencies. Expand the scope when changes touch shared middleware, identity/authentication, authorization, global configuration, dependency graph, database access patterns, cryptographic logic, multi-tenant boundaries, or other high-impact shared components.

Previously identified unresolved findings remain relevant. Do not treat old evidence as proof about modified code.

## Findings and validation

Follow the current upstream schema and status vocabulary. At minimum, preserve these distinctions:
- Confirmed: evidence supports a real vulnerability and plausible impact.
- Needs validation: a lead remains unresolved; do not present it as a confirmed vulnerability or assign unsupported severity.
- Rejected/refuted: independent validation found the candidate does not hold under the tested conditions.

For each confirmed finding, retain enough evidence to locate the affected code and explain the attack path, preconditions, impact, and relevant defensive controls. Redact secrets and personal data. Avoid copying sensitive proof-of-concept payloads into broadly visible reports when a safer summary suffices.

Independently evaluate candidates before implementing a fix. Defense-in-depth gaps are not automatically exploitable vulnerabilities if another verified control blocks the attack; explain residual risk rather than overstating impact.

After a fix:
1. Re-run the relevant audit or validation path.
2. Confirm the original attack path is blocked.
3. Add or run regression tests when feasible.
4. Check adjacent code for new vulnerabilities.
5. Preserve unresolved items as unresolved.

## Sandbox and untrusted code

Do not execute target code, tests, build scripts, migrations, or untrusted project commands under an assumption that a sandbox exists. Verify the actual host controls first.

Prefer a sandbox that provides:
- No external network access by default.
- A sanitized, allowlisted environment.
- Resource limits.
- Restricted write access, ideally only to designated scratch/work directories and the intended working tree as needed.
- No access to production credentials, developer secrets, or unrelated files.
- Explicit handling of package restore and test services.

If controls are unavailable or unknown, do not run potentially unsafe code. Continue static inspection and other safe checks where possible, and mark dynamic validation as blocked/not run. Never claim Cloudflare's audit ran if it did not.

## Fallback behavior

If the Cloudflare skill is missing, incompatible, or cannot be invoked:
- State the precise limitation.
- Ask for approval before installation or environment changes.
- Run only the security checks genuinely supported by the host and label them as fallback checks.
- Do not manufacture Cloudflare report files, findings, coverage ledgers, or confirmation statuses.
- Do not treat fallback checks as equivalent to the complete upstream audit.
