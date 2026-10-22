# API Engineering & Optimization — Orchestrator Skill

Portable, agent-oriented skill specification for safely improving existing APIs, primarily C#/.NET 8, while coordinating with Cloudflare's `security-audit` skill when available.

## Package contents

- `SKILL.md` — entry point and non-negotiable operating rules.
- `references/workflow.md` — sequential workflow for maintenance and new endpoints.
- `references/security-integration.md` — Cloudflare security-audit integration and safe fallback.
- `references/quality-gates.md` — evidence-based scoring, blockers, retries, and completion criteria.
- `references/dotnet-api-checklist.md` — API-specific implementation and security checklist.
- `templates/final-report.md` — final delivery report template.

## Install/use

Place this directory in the skill location supported by your coding agent, preserving the directory name and relative paths. Skill discovery, invocation, and frontmatter conventions differ between agents; adapt only the installation wrapper/frontmatter required by the target agent, without weakening the rules in `SKILL.md`.

The orchestrator does **not** install packages, skills, tools, or dependencies without explicit user approval. If Cloudflare's skill is absent, explain that and ask for approval before installing it. The documented upstream installation command is:

```bash
npx skills add https://github.com/cloudflare/security-audit-skill --skill security-audit
```

Do not run this command automatically. Confirm the target agent supports the Skills CLI and the requested installation scope first.

Upstream project: https://github.com/cloudflare/security-audit-skill

## Important limits

This package defines behavior and decision rules; it does not itself provide sandboxing, run tests, grant repository access, invoke another skill, or prove security. Those capabilities must be supplied by the host agent and environment. Never claim a tool, audit, build, test, score, commit, or PR operation succeeded unless it actually ran and its result was inspected.
