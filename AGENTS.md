# AGENTS.md: AI Agent Guidelines

This file helps AI coding agents be immediately productive in the Interview Coach codebase.

## Quick Commands

```sh
# Build
dotnet build InterviewCoach.slnx

# Test all
dotnet test InterviewCoach.slnx

# Test focused
dotnet test tests/InterviewCoach.Agent.Tests/InterviewCoach.Agent.Tests.csproj

# Run full stack (Aspire)
aspire run --file ./apphost.cs
```

## Project Architecture at a Glance

This is a **multi-service Aspire app** orchestrating an AI interview coaching platform.

**Service map:**
- `src/InterviewCoach.Agent` — Interview orchestration via Microsoft Agent Framework + MCP clients
- `src/InterviewCoach.WebUI` — Blazor chat UI (presentation only)
- `src/InterviewCoach.Mcp.InterviewData` — Session persistence via EF Core + SQLite
- `src/InterviewCoach.AppHost` — Aspire orchestration and provider wiring
- `src/InterviewCoach.ServiceDefaults` — Shared cross-cutting defaults

**Tech stack:** C# on .NET 10, Blazor, Agent Framework, MCP, Aspire, EF Core, xUnit.

See [ARCHITECTURE.md](docs/ARCHITECTURE.md) for detailed system design and [MULTI-AGENT.md](docs/MULTI-AGENT.md) for handoff workflow.

## Architecture Boundaries (Hard Rules)

- **Separate responsibilities:** Do not add data/storage logic to Agent or WebUI; use MCP servers.
- **Provider abstraction:** LLM backend (`LlmProvider`) is pluggable at runtime (Foundry, Azure OpenAI, GitHub Models).
- **Agent-scoped logic:** Each service should not know about other services' internals; use contracts.
- **Provider wiring lives in AppHost:** Configuration of LLM providers belongs in `src/InterviewCoach.AppHost`, not scattered in feature code.

## Code Conventions

**C# style:**
- Nullable enabled, implicit usings enabled (see csproj files)
- Async end-to-end; never block on `.Result` or `.Wait()`
- Use `IChatClient` for LLM calls (ensures provider swapping works)

**Configuration-driven changes:**
- `LlmProvider` enum and `AgentMode` are the primary runtime switches
- Use `apphost.settings.json`, user secrets, and environment variables for config/secrets

**Preserve backward compat for:**
- `HandoffToolResultFix` behavior — if you touch handoff/streaming, align tests and docs

## Common Patterns

### LLM Provider Wiring
Providers are wired in `AgentDelegateFactory`. To swap providers:
1. Set `LlmProvider` in apphost.settings.json or env vars
2. Factory automatically routes to Foundry, Azure OpenAI, or GitHub Models
3. `IChatClient` is passed to the agent; agent does not know which provider

### MCP Tool Integration
To add a new tool:
1. Define the tool in an MCP server (e.g., `InterviewData` or `MarkItDown`)
2. Wire the MCP client in the agent's ContextProviders
3. Agent calls the tool via the MCP protocol (HTTP/SSE)
4. Do **not** add direct storage calls in the agent

### Testing
Tests are in `tests/InterviewCoach.Agent.Tests/`. Use `[Fact]` for unit tests, mock MCP clients for isolated testing.

## Practical Guardrails

When making non-trivial changes:
1. **Run tests** → `dotnet test InterviewCoach.slnx`
2. **Verify docs** — If touching architecture, update [ARCHITECTURE.md](docs/ARCHITECTURE.md) and [MULTI-AGENT.md](docs/MULTI-AGENT.md)
3. **Verify docs** — If touching setup/commands, update [README.md](README.md) and [TUTORIALS.md](docs/TUTORIALS.md)
4. **Minimal diffs** — Preserve existing naming and patterns

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

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for full commit spec, scopes, and troubleshooting.

## Pull Request Guidelines

When opening a PR:
- **One concern per PR:** One feature, one bug, or one refactor — no mixing
- **Populate all fields** in the PR template (`.github/PULL_REQUEST_TEMPLATE.md`) — never leave defaults
- **Link to issues** using `Fixes #123`, `Closes #456`, or `Related to #789`

See [CONTRIBUTING.md](docs/CONTRIBUTING.md) for submission guidelines.

## Azure Deployment

When working with Azure:
- Use the `@azure` MCP rule to invoke best practices tools
- Infrastructure-as-code lives in `resources-foundry/infra/` (Bicep templates)
- Provider config in [docs/providers/AZURE-OPENAI.md](docs/providers/AZURE-OPENAI.md)
- Deploy via `azd up` — see [TUTORIALS.md](docs/TUTORIALS.md)

## Further Reading

- [Architecture Deep Dive](docs/ARCHITECTURE.md)
- [Multi-Agent Handoff](docs/MULTI-AGENT.md)
- [Contributing](docs/CONTRIBUTING.md)
- [Learning Objectives](docs/LEARNING-OBJECTIVES.md)
- [FAQ](docs/FAQ.md)

