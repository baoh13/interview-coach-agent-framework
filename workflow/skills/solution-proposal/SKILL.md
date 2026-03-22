---
name: solution-proposal
description: Given the ticket and research, generate a structured solution proposal. Use when user requests to 'propose solution for ticket ABC-123' or similar phrases indicating they want a solution generated from a Github issue.
user-invocable: false
---

# Solution Proposal Skill

## Description
Generates a structured solution proposal based on Github issues and research findings.

## When to Use
Use this skill when:
- User requests a solution to an issue (e.g., "Propose solution for issue ABC-123")
- User wants to determine if an issue has a solution ready to go
- User needs a solution generated from a Github issue
- User wants to validate implementation readiness before assigning work

## Capabilities
- Assesses technical scope across frontend, backend, database, and infrastructure
- Generates structured `SOLUTION.md` proposal if issue is sufficient

## Prerequisites
- Access to the issues directory structure
- Repository context information (either in skill assets or repository documentation)

## Workflow

### Step 1: Research and Issue 
1. Get the issue information from `issues/{ISSUE_NUMBER}/` directory
2. Get the research information from `issues/{ISSUE_NUMBER}/{ISSUE_NUMBER}_research.md`
3. If not exists, stop processing and prompt the user to use available prompts and skills to fetch the issue and build the research.

### Step 2: Generate SOLUTION.md
1. Based on the issue information, research and repository context generate a structured solution proposal that includes:
   - High-level approach
   - A breakdown of implementation tasks
   - Identification of which repositories and components are affected
   - Any potential risks or complexities
   - Include file change proposals, code snippets, architectural diagrams, or other relevant artifacts to illustrate the solution
2. Save the solution proposal to: `issues/{ISSUE_NUMBER}/{ISSUE_NUMBER}_solution.md`
3. Report success with file location
4. Provide brief summary of the proposed solution 

## Output Examples


## Multi-Repository Context

This skill works with a multi-repository environment where:
- Repositories are located in: `repos/`
- Repository metadata is defined in: `repos/REPOSITORIES.md`
- Each repository may have documentation about its purpose and architecture
- Tickets should specify which repositories are affected

The skill will read `repos/REPOSITORIES.md` to understand which repositories exist and their characteristics.

Example `repos/REPOSITORIES.md` structure:

```markdown
### Onboarding API

**Name:** `onboarding-api`  
**Type:** `backend`  
**Description:** REST API for account onboarding  
**Technologies:** Java, Spring Boot, PostgreSQL  
**Documentation:** `repos/onboarding-api/README.md`  
```

## Best Practices

### For This Skill:
- Be strict about explaining sufficiency what changes are goinhg to be made and why
- Break down tasks to appropriate granularity for junior developers
- Include verification methods for each acceptance criterion
- Consider cross-repository dependencies
- Flag potential risks or complex areas that may need senior review

## Error Handling

- **Insufficient information**: Do NOT generate SOLUTION.md, report gaps instead
- **Repository not found**: Flag in analysis report
- **Ambiguous scope**: Request clarification from user
