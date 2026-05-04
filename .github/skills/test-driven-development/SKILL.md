---
name: test-driven-development
description: Implement features and bug fixes using strict red-green-refactor with this repository's .NET test workflow.
---

## Instructions

Use this skill before writing production code for any feature or fix.

Core rule: **no production code without a failing test first**.

1. **RED**: add one focused failing test for the behavior change.
2. **GREEN**: write minimal code to make that test pass.
3. **REFACTOR**: improve structure while keeping tests green.
4. Repeat in small increments.

Repository test commands:

```powershell
dotnet test InterviewCoach.slnx
```

Focused loop for agent behavior changes:

```powershell
dotnet test tests/InterviewCoach.Agent.Tests/InterviewCoach.Agent.Tests.csproj
```

Before finishing, ensure:
- the new test failed for the expected reason before implementation
- all relevant tests pass
- behavior changes are reflected in docs when applicable
