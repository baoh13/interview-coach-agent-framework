# GitHub Copilot Instructions

**Primary reference:** See [AGENTS.md](../../AGENTS.md) in the repository root for comprehensive AI agent guidance. This file provides GitHub Copilot-specific configuration and supplements the main guidelines.

---

## Azure Rules (Required)
- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.

## Build and Test
- Use .NET 10 SDK (see `global.json`).
- Preferred build command from repo root: `dotnet build InterviewCoach.slnx`.
- Run tests with: `dotnet test InterviewCoach.slnx`.
- For focused tests: `dotnet test tests/InterviewCoach.Agent.Tests/InterviewCoach.Agent.Tests.csproj`.
- Run full local app stack through Aspire with: `aspire run --file ./apphost.cs`.

## Architecture Boundaries
- Treat this as a multi-service Aspire app. Keep service responsibilities separated:
	- `src/InterviewCoach.Agent`: interview orchestration and MCP tool usage.
	- `src/InterviewCoach.WebUI`: Blazor frontend only.
	- `src/InterviewCoach.Mcp.InterviewData`: MCP server and EF Core persistence.
	- `src/InterviewCoach.AppHost`: orchestration and provider wiring.
	- `src/InterviewCoach.ServiceDefaults`: shared service defaults.
- Do not bypass MCP boundaries by adding direct data/storage logic in the Agent or WebUI.
- Keep provider-selection logic in app host/provider factory layers, not scattered in feature code.

## Conventions
- Keep changes configuration-driven:
	- `LlmProvider` and `AgentMode` are the primary runtime switches.
	- Use `apphost.settings.json`, user secrets, and environment variables for secrets/config.
- Preserve current C# project style:
	- Nullable enabled and implicit usings enabled.
	- Use async APIs end-to-end; avoid blocking calls.
- When changing handoff/workflow streaming behavior, keep `HandoffToolResultFix` behavior and corresponding tests aligned.

## Practical Guardrails for Agents
- Validate with build/tests after non-trivial edits.
- Prefer minimal diffs and preserve existing naming and patterns.
- If touching architecture behavior, verify docs are still accurate:
	- `docs/ARCHITECTURE.md`
	- `docs/MULTI-AGENT.md`
- If touching setup or commands, verify docs are still accurate:
	- `README.md`
	- `docs/TUTORIALS.md`

## Commit Conventions

Follow [Conventional Commits](https://www.conventionalcommits.org/). The **pre-push hook** validates this automatically.

**Quick ref:**
- **Type:** `feat`, `fix`, `refactor`, `docs`, `test`, `chore`, `ci`, `build`, `style`, `perf`
- **Scope:** e.g., `(agent)`, `(ui)`, `(db)`, `(docs)` — optional, omit for multi-area changes
- **Subject:** imperative mood, lowercase, no period, ≤50 chars
- **Body:** wrap at 72 chars, explain *what* and *why*, not *how*

**Example:**
```
feat(agent): add batch processing for discovery phase

Improve performance by grouping discovery questions into configurable
batches, reducing round-trip latency and token overhead.
```

## Pull Request Guidelines

When opening a pull request, always populate every field of the PR template (`.github/PULL_REQUEST_TEMPLATE.md`) as described below. Never leave fields at their placeholder defaults.

### Single scope

A PR must address exactly one concern — one feature, one bug, or one refactor. This reinforces the convention from Section 3. If your changes span multiple unrelated concerns, split them into separate PRs.