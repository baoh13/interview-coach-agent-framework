# Project Guidelines

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


## Git Conventions

### Commit Message Format
Use the commit message as a two-part object:

Subject:
<type>(<scope>): <short description>

Body:
<what changed and why in 1-3 sentences. explain what and why, not how>

Changes:
<specific change 1>
<specific change 2>

### Types
- feat: New feature
- fix: Bug fix
- refactor: Code change that neither fixes nor adds
- docs: Documentation only
- test: Adding tests
- chore: Maintenance

### Rules
- Subject line: max 50 characters, imperative mood
- Body: wrap at 72 characters
- One logical change per commit

### Example
A commit message for a new feature might look like this:

```
feat(italy-legacy): support explicit exclusion expiry

Adds support in the Italy legacy stack for displaying "indefinite" exclusions based on the new ExclusionExpiryType flag, so CS Admin exclusion history and the responsible gambling/self-exclusion UI can rely on an explicit expiry type instead of only date heuristics.

Changes:
- Introduces WebService_ExclusionExpiryTypes and wires it through domain entities (SiteExclusion, SiteAndProductExclusion) and view models (ActiveExclusionViewModel).
- Extends gRPC/proto and mapping layers to carry exclusion_expiry_type and map it into web-service enums.
- Updates CS Admin and RG UI mapping to use an explicit IsIndefinite flag.
```

## Pull Request Guidelines

When opening a pull request, always populate every field of the PR template (`.github/PULL_REQUEST_TEMPLATE.md`) as described below. Never leave fields at their placeholder defaults.

### Single scope

A PR must address exactly one concern — one feature, one bug, or one refactor. This reinforces the convention from Section 3. If your changes span multiple unrelated concerns, split them into separate PRs.