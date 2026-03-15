---
name: mcp-builder
description: Build and evolve MCP servers for this repository with workflow-first tool design, strict schema validation, and evaluation-driven quality checks.
---

## Instructions

Use this skill when creating or extending MCP capabilities, especially in `src/InterviewCoach.Mcp.InterviewData`.

1. Design tools around user workflows, not one-to-one API wrappers.
2. Keep architecture boundaries intact:
   - MCP persistence and tool logic stay in `InterviewCoach.Mcp.InterviewData`.
   - Do not move data/storage logic into Agent or WebUI.
3. Define strict input/output contracts and return concise, high-signal responses.
4. Write actionable error messages that tell the agent what to try next.
5. Add evaluations/tests for each new tool path before considering work complete.

For this repo, validate with:

```powershell
dotnet build InterviewCoach.slnx
dotnet test tests/InterviewCoach.Agent.Tests/InterviewCoach.Agent.Tests.csproj
```
