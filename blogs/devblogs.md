# Build an AI Interview Coach with Microsoft Agent Framework, MCP, and Aspire

Building AI agents is getting easier. Deploying them as part of a real application — with multiple services, persistent state, and production-grade infrastructure — is where things get complicated.

We built an open-source Interview Coach sample to show how [Microsoft Agent Framework](https://aka.ms/agent-framework), [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry), [Model Context Protocol (MCP)](https://modelcontextprotocol.io/), and [Aspire](https://aspire.dev) fit together in a production-style application. It's a working interview simulator where an AI coach walks you through behavioral and technical questions, then delivers a summary of your performance.

This post walks through the patterns we used and why they matter for your own agent projects.

Here's the link to visit the [Interview Coach demo app](https://aka.ms/agentframework/interviewcoach).

## Why Microsoft Agent Framework?

If you've been building AI agents with .NET, you've probably used Semantic Kernel, AutoGen, or both. [Microsoft Agent Framework](https://aka.ms/agent-framework) is the next step — built by the same teams, combining what worked from both projects into a single framework.

It brings AutoGen's straightforward agent abstractions together with Semantic Kernel's enterprise features: session-based state management, type safety, middleware, and telemetry. On top of that, it adds graph-based workflows for explicit multi-agent orchestration.

For .NET developers, this means:

- **One framework instead of two** — no more choosing between Semantic Kernel and AutoGen. Agent Framework unifies them.
- **Familiar patterns** — agents are built with dependency injection, `IChatClient`, and the same hosting model you use for ASP.NET apps.
- **Production-ready from day one** — built-in support for OpenTelemetry, middleware pipelines, and Aspire integration.
- **Multi-agent orchestration** — sequential workflows, concurrent execution, handoff patterns, and group chat are all first-class features.

The Interview Coach sample shows these capabilities in a real application, not just a Hello World.

## Why Microsoft Foundry?

AI agents need more than a model — they need infrastructure. [Microsoft Foundry](https://learn.microsoft.com/azure/foundry/what-is-foundry) is Azure's unified platform for building and managing AI applications, and it's the recommended backend for Microsoft Agent Framework.

Foundry gives you a single portal for:

- **Model access** — a catalog of models from OpenAI, Meta, Mistral, and others, all accessible through one endpoint
- **Content safety** — built-in moderation and PII detection so your agents don't go off the rails
- **Cost-optimized routing** — automatically route requests to the best model for the job
- **Evaluation and fine-tuning** — measure agent quality and improve it over time
- **Enterprise governance** — identity management, access control, and compliance features via Entra ID and Microsoft Defender

For the Interview Coach, Foundry provides the model endpoint that powers the agents. Because the agent code uses the `IChatClient` interface, Foundry is just a configuration choice — but it's the one that gives you the most production tooling out of the box.

The sample also supports [GitHub Models](https://github.com/marketplace/models) as a free alternative for prototyping.

## What does the Interview Coach do?

The Interview Coach is a conversational AI that runs a mock job interview. You provide a resume and a job description, and the agent takes it from there:

1. **Intake** — collects your resume and the target job description
2. **Behavioral interview** — asks STAR-method questions tailored to your experience
3. **Technical interview** — asks role-specific technical questions
4. **Summary** — generates a detailed performance review with actionable feedback

You interact with it through a Blazor web UI that streams responses in real time.

## Architecture at a glance

The application is split into several services, all orchestrated by Aspire:

- **WebUI** — a Blazor chat interface for the interview conversation
- **Agent** — the interview logic, built on Microsoft Agent Framework
- **MarkItDown MCP Server** — parses resumes (PDF, DOCX) into markdown via Microsoft's MarkItDown
- **InterviewData MCP Server** — a custom .NET MCP server that stores sessions in SQLite
- **LLM Provider** — Microsoft Foundry (recommended) or GitHub Models for prototyping

![Overall architecture](../assets/architecture.png)

Aspire handles service discovery, health checks, and telemetry. Each component runs as a separate process, and you start the whole thing with a single command.

## Pattern 1: Pluggable LLM providers via IChatClient

The agent code doesn't know or care which LLM it's talking to. Every agent is built on the `IChatClient` interface from [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai), so the model provider is a configuration choice — not a code change.

The Interview Coach supports two providers:

- **Microsoft Foundry** (recommended) — production-grade, with content safety, model routing, and enterprise governance
- **GitHub Models** — free tier, useful for prototyping without an Azure subscription

Switching is a one-line config change in `apphost.settings.json`:

```jsonc
{
  // To use Microsoft Foundry as an LLM provider
  "LlmProvider": "MicrosoftFoundry"
}
```

Or pass it as a command-line flag:

```bash
# To use Microsoft Foundry as an LLM provider
aspire run --file ./apphost.cs -- --provider MicrosoftFoundry

# To use GitHub Models as an LLM provider
aspire run --file ./apphost.cs -- --provider GitHubModels
```

Under the hood, the Aspire app host uses a provider factory that wires up the right resource based on the provider setting. The agent project receives an `IChatClient` through dependency injection and never references a specific provider SDK directly. This means you can prototype on GitHub Models for free, then switch to Foundry for production without touching agent code. And even when a new provider is available, it can be easily adopted through the provider factory.

## Pattern 2: Multi-agent handoff

The most interesting pattern in this sample is the multi-agent handoff architecture. Instead of one agent doing everything, the interview is split across five specialized agents:

| Agent | Role | Tools |
|-------|------|-------|
| **Triage** | Routes messages to the right specialist | None (pure routing) |
| **Receptionist** | Creates sessions, collects resume and job description | MarkItDown + InterviewData |
| **Behavioral Interviewer** | Conducts behavioral questions using the STAR method | InterviewData |
| **Technical Interviewer** | Asks role-specific technical questions | InterviewData |
| **Summarizer** | Generates a comprehensive interview summary | InterviewData |

In the handoff pattern, one agent transfers full control of the conversation to the next. The receiving agent takes over entirely. This is different from "agent-as-tools," where a primary agent calls others as helpers but retains control.

Here's how the handoff workflow is wired up:

```csharp
var workflow = AgentWorkflowBuilder
               .CreateHandoffBuilderWith(triageAgent)
               .WithHandoffs(triageAgent, [receptionistAgent, behaviouralAgent, technicalAgent, summariserAgent])
               .WithHandoffs(receptionistAgent, [behaviouralAgent, triageAgent])
               .WithHandoffs(behaviouralAgent, [technicalAgent, triageAgent])
               .WithHandoffs(technicalAgent, [summariserAgent, triageAgent])
               .WithHandoff(summariserAgent, triageAgent)
               .Build();
```

The happy path is sequential: Receptionist ➡️ Behavioral ➡️ Technical ➡️ Summarizer. Each specialist hands off directly to the next. If something goes off-script, agents fall back to Triage for re-routing.

The sample also includes a single-agent mode for simpler deployments, so you can compare the two approaches side by side.

## Pattern 3: MCP for tool integration

Tools in this project don't live inside the agent. They live in their own MCP (Model Context Protocol) servers, which means:

- **Reusable** — the same MarkItDown server could be used by a completely different agent project
- **Independent** — tool teams and agent teams can develop and deploy separately
- **Language-agnostic** — MarkItDown is a Python server; the agent is .NET. MCP bridges the gap.

The agent discovers tools at startup through MCP clients and passes them to the appropriate agents:

```csharp
var receptionistAgent = new ChatClientAgent(
    chatClient: chatClient,
    name: "receptionist",
    instructions: "You are the Receptionist. Set up sessions and collect documents...",
    tools: [.. markitdownTools, .. interviewDataTools]);
```

Each agent only gets the tools it needs — Triage gets none (it just routes), interviewers get session access, the Receptionist gets document parsing plus session access. This follows the principle of least privilege.

## Pattern 4: Aspire orchestration

Aspire ties everything together. The app host defines the service topology — which services exist, how they depend on each other, and what configuration they receive. You get:

- **Service discovery** — services find each other by name, not hardcoded URLs
- **Health checks** — the Aspire dashboard shows the status of every component
- **Distributed tracing** — OpenTelemetry is wired up through shared service defaults
- **One-command startup** — `aspire run --file ./apphost.cs` launches everything

For deployment, `azd up` pushes the entire application to Azure Container Apps.

## Get started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Azure Subscription](https://azure.microsoft.com/free)
- [Microsoft Foundry](https://ai.azure.com) project (or use [GitHub Models](https://github.com/marketplace/models) for free prototyping)

### Run it locally

```bash
git clone https://github.com/Azure-Samples/interview-coach-agent-framework.git
cd interview-coach-agent-framework

# Configure credentials
dotnet user-secrets --file ./apphost.cs set MicrosoftFoundry:Project:Endpoint "<your-endpoint>"
dotnet user-secrets --file ./apphost.cs set MicrosoftFoundry:Project:ApiKey "<your-key>"

# Start all services
aspire run --file ./apphost.cs
```

Open the Aspire Dashboard, wait for all services to show ✅ Running, and click the WebUI endpoint to start your mock interview.

### Deploy to Azure

```bash
azd auth login
azd up
```

That's it. Aspire and `azd` handle the rest. Once you complete, you can safely delete all the resources by running:

```bash
azd down --force --purge
```

## What you'll learn from this sample

After working through the Interview Coach, you'll have hands-on experience with:

- **Microsoft Foundry** — using Azure's unified AI platform as the model backend
- **Microsoft Agent Framework** — building single-agent and multi-agent systems
- **Handoff orchestration** — splitting complex workflows across specialized agents
- **MCP** — creating and consuming tool servers independently of agent code
- **Aspire** — orchestrating multi-service applications with observability built in
- **Instruction design** — writing prompts that produce consistent, structured behavior
- **Azure deployment** — shipping everything with `azd up`

## Try it out

The full source is on GitHub: [Azure-Samples/interview-coach-agent-framework](https://aka.ms/agentframework/interviewcoach)

If you're new to Microsoft Agent Framework, start with the [framework documentation](https://aka.ms/agent-framework) and the [Hello World sample](https://aka.ms/dotnet/agent-framework/helloworld). Then come back to the Interview Coach to see how those building blocks come together in a real application.

We'd love to hear what you build with these patterns. [Open an issue in the repo](https://github.com/Azure-Samples/interview-coach-agent-framework/issues) or reach out to the team.

## What's next?

We're adding other integration scenarios with Microsoft Agent Framework such as [Microsoft Foundry Agent Service](https://learn.microsoft.com/agent-framework/agents/providers/azure-ai-foundry?pivots=programming-language-csharp), [GitHub Copilot](https://learn.microsoft.com/agent-framework/agents/providers/github-copilot?pivots=programming-language-csharp) and [A2A](https://learn.microsoft.com/en-us/agent-framework/integrations/a2a?pivots=programming-language-csharp), which will be available soon. Stay tuned!

## Resources

- [Microsoft Agent Framework documentation](https://aka.ms/agent-framework)
- [Introducing Microsoft Agent Framework preview](https://devblogs.microsoft.com/dotnet/introducing-microsoft-agent-framework-preview/)
- [Microsoft Agent Framework Reaches Release Candidate](https://devblogs.microsoft.com/foundry/microsoft-agent-framework-reaches-release-candidate/)
- [Microsoft Foundry documentation](https://learn.microsoft.com/azure/foundry/what-is-foundry)
- [Microsoft Foundry Agent Service](https://learn.microsoft.com/en-us/azure/foundry/agents/overview)
- [Microsoft Foundry Portal](https://ai.azure.com)
- [Model Context Protocol specification](https://modelcontextprotocol.io)
- [Aspire documentation](https://aspire.dev)
