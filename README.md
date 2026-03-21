# Interview Coach with Microsoft Agent Framework

An AI-powered interview coach that shows how to wire up [Microsoft Agent Framework](https://aka.ms/agent-framework), [Model Context Protocol (MCP)](https://modelcontextprotocol.io), and [Aspire](https://aspire.dev) into a working application you can deploy.

## Repository Snapshot (For Agents)

### 1. Repository Purpose

This repository contains a production-style sample for an AI interview coaching application. It demonstrates how to orchestrate multi-agent interview workflows, connect MCP tools, persist interview sessions, and run the full stack locally or in Azure.

Domain and business area:

- Talent development and interview preparation
- Conversational AI orchestration for guided coaching
- Multi-service app composition for enterprise-ready AI solutions

### 2. Repository Type

This is a full-stack, multi-service solution (not a single app type). It includes:

- UI/frontend: Blazor Web UI in `src/InterviewCoach.WebUI`
- API/backend services: Agent orchestration and MCP services in `src/InterviewCoach.Agent` and `src/InterviewCoach.Mcp.InterviewData`
- Infrastructure/orchestration: .NET Aspire app host and Azure deployment assets via `src/InterviewCoach.AppHost`, `apphost.cs`, and `azure.yaml`
- Shared library/utilities: cross-cutting defaults in `src/InterviewCoach.ServiceDefaults`

### 3. Technologies

- Language: C# on .NET 10
- Web framework: Blazor (interactive UI)
- Agent framework: Microsoft Agent Framework
- Tool protocol: Model Context Protocol (MCP)
- Orchestration and local distributed app runtime: .NET Aspire
- Data access: EF Core with SQLite (Interview session persistence)
- Cloud/deployment: Azure Developer CLI (`azd`), Azure Container Apps, Bicep templates under `resources-foundry/infra`
- Testing: xUnit with targeted tests under `tests/InterviewCoach.Agent.Tests`

### 4. When To Modify This Repository

Developers usually update this repository when they need to:

- Add or change interview coaching behavior, prompts, or multi-agent handoff flows
- Integrate or update MCP tools and server contracts
- Introduce a new LLM provider, adjust provider wiring, or change provider configuration behavior
- Update the Blazor UI experience, chat flow, landing content, or interaction patterns
- Fix session persistence, data access, or repository behavior in InterviewData
- Improve local orchestration, startup wiring, deployment infrastructure, or environment configuration
- Add or update tests for workflow behavior, parsing/deserialization, or UI component rendering

### 5. Documentation Found

Primary docs discovered in this repository/workspace:

- Root overview: `README.md`
- Copilot instructions: `.github/copilot-instructions.md`
- AGENTS.md: not found in this repository
- docs folder (key files):
   - `docs/ARCHITECTURE.md`
   - `docs/MULTI-AGENT.md`
   - `docs/TUTORIALS.md`
   - `docs/FAQ.md`
   - `docs/USER-MANUAL.md`
   - `docs/LEARNING-OBJECTIVES.md`
   - `docs/CONTRIBUTING.md`
   - `docs/CHANGELOG.md`
   - `docs/providers/README.md`
   - `docs/providers/AZURE-OPENAI.md`
   - `docs/providers/MICROSOFT-FOUNDRY.md`
   - `docs/providers/GITHUB-MODELS.md`
   - `docs/providers/GITHUB-COPILOT.md`

## What you'll learn

This sample covers the patterns you'd need for a real agent deployment:

- Building AI agents with Microsoft Agent Framework
- Multi-agent handoff orchestration — single agent vs. 5 specialized agents
- Model Context Protocol (MCP) for adding tools without touching agent code
- Running multiple services together with Aspire
- Keeping conversation state across sessions
- Swapping LLM providers (Microsoft Foundry, Azure OpenAI, GitHub Models, GitHub Copilot)
- Deploying to Azure with `azd up`

See [learning objectives](docs/LEARNING-OBJECTIVES.md) for the full breakdown.

## Architecture

![Overall architecture](./assets/architecture.png)

The app is split into a few services:

- **Aspire** orchestrates everything (service discovery, health checks, config)
- **WebUI** is a Blazor chat interface
- **Agent** runs the interview logic via Microsoft Agent Framework
- **MCP Servers** handle document parsing (MarkItDown) and session storage (InterviewData)
- **LLM Provider** talks to Foundry, Azure OpenAI, or GitHub Models

See [architecture overview](docs/ARCHITECTURE.md) for how the pieces fit together.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [Azure Subscription](https://azure.microsoft.com/free)
- [Microsoft Foundry](https://ai.azure.com)

See [LLM provider options](docs/providers/README.md) for alternatives.

## Getting Started

### 1. Clone Repository

```bash
git clone https://github.com/Azure-Samples/interview-coach-agent-framework.git
cd interview-coach-agent-framework
```

### 2. Configure Microsoft Foundry

1. Create a new Microsoft Foundry project on Foundry Portal or command line.

   See [Foundry setup guide](docs/providers/MICROSOFT-FOUNDRY.md) for details.

### 3. Store Credentials

Use .NET user secrets to keep credentials secure:

```bash
dotnet user-secrets --file ./apphost.cs set MicrosoftFoundry:Project:Endpoint "{{MICROSOFT_FOUNDRY_PROJECT_ENDPOINT}}"
dotnet user-secrets --file ./apphost.cs set MicrosoftFoundry:Project:ApiKey "{{MICROSOFT_FOUNDRY_API_KEY}}"
```

### 4. Run the Application

Start all services with .NET Aspire:

```bash
aspire run --file ./apphost.cs
```

**What happens next:**

1. Open Aspire Dashboard (URL shown in terminal output).
1. All services start (Agent, WebUI, MCP servers, SQLite).
1. Look for ✅ "Running" status on all resources.
1. Click the **webui** endpoint to open the interview coach.

### 5. Deploy to Azure

Deploy the entire application to Azure Container Apps with one command:

```bash
# Login to Azure
azd auth login

# Provision resources and deploy
azd up
```

### 6. Clean Up Resources

When finished, remove all Azure resources:

```bash
azd down --force --purge
```

## Next Steps

### Learn

- [Learning objectives](docs/LEARNING-OBJECTIVES.md)
- [Architecture overview](docs/ARCHITECTURE.md)
- [Tutorials](docs/TUTORIALS.md)
- [FAQ](docs/FAQ.md)

### Alternative LLM providers

The default is Microsoft Foundry, but you can also use:

- [Azure OpenAI](docs/providers/AZURE-OPENAI.md) — direct AOAI integration
- [GitHub Models](docs/providers/GITHUB-MODELS.md) — free tier, good for prototyping
<!-- - [GitHub Copilot](docs/providers/GITHUB-COPILOT.md) — local dev with Copilot SDK -->

### Alternative agent mode

The default is `LlmHandOff`, but you can also use:

- [`Single`](docs/MULTI-AGENT.md#mode-1-single-agent) - single-agent mode
<!-- - [`CopilotHandOff`](docs/MULTI-AGENT.md#mode-3-multi-agent-handoff-gitHub-copilot) - multi-agent mode with GitHub Copilot -->

## Additional Resources

### Microsoft Foundry

- [What is Microsoft Foundry?](https://learn.microsoft.com/azure/ai-foundry/what-is-foundry?view=foundry)
- [Foundry Agent Service](https://learn.microsoft.com/azure/ai-foundry/agents/overview?view=foundry)

### Microsoft Agent Framework

- [Framework Documentation](https://aka.ms/agent-framework)
- [Multi-Agent Orchestration](https://learn.microsoft.com/agent-framework/user-guide/workflows/orchestrations/overview)
- [AG-UI Protocol](https://docs.ag-ui.com/introduction)

### Model Context Protocol

- [MarkItDown MCP Server](https://github.com/microsoft/markitdown/tree/main/packages/markitdown-mcp)
- [MCP Specification](https://modelcontextprotocol.io)
- [MCP Server Registry](https://github.com/modelcontextprotocol/servers)

### Aspire

- [Aspire Documentation](https://aspire.dev)
- [Integrations](https://aspire.dev/integrations/overview/)
- [Deployment](https://aspire.dev/deployment/overview/)

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](docs/CONTRIBUTING.md) for guidelines.

## License

This project is licensed under the MIT License - see [LICENSE.md](LICENSE.md) for details.

---

