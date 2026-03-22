---
name: ticket-researching
description: Analyze Github issues to determine if they contain sufficient information for implementation. Use when user requests to 'research issue ABC-123 and propose solution' or similar phrases indicating they want to prepare for implementation by researching the issue.
user-invocable: false
---

# Research Issue Skill

## Description
Analyzes Github issues to assess whether they contain sufficient information to propose a solution and propose that solution.

## When to Use
Use this skill when:
- User requests issue research (e.g., "Research issue ABC-123 and propose solution")
- User wants to determine if an issue is ready for creating a solution
- User wants to validate issue completeness before assigning work

## Capabilities
- Fetches Github issues (or uses already-fetched issues)
- Analyzes issues for implementation readiness
- Identifies missing critical information
- Generates structured `RESEARCH.md` report with findings
- Considers multi-repository context
- Assesses technical scope across frontend, backend, database, and infrastructure

## Prerequisites
- GitHub CLI must be installed and configured (for fetching issues)
- Access to the issues directory structure
- Repository context information (either in skill assets or repository documentation)

## Workflow

### Step 1: Obtain Issue Data
1. Check if issue exists in `issues/{ISSUE_NUMBER}/` directory
2. If exists, read the issue data from JSON and plain text files
3. If not exists, stop processing and prompt the user to use available prompts and skills to fetch the issue.

### Step 2: Analyze Issue for Sufficiency
Evaluate the issue against the following criteria:

#### Critical Information (MUST HAVE):
- **Clear Problem Statement**: What issue/feature is being addressed?
- **Acceptance Criteria**: How do we know when it's done?
- **Target Repository/Repositories**: Which codebase(s) need changes?
- **User Story or Use Case**: Who benefits and how?
- **Technical Context**: Enough detail to understand the scope. Can we identify the technical components involved within the scoped repo?

#### Important Information (SHOULD HAVE):
- **Dependencies**: Related issues, systems, or external services
- **Technical Constraints**: Performance, security, compliance requirements
- **UI/UX Requirements**: Mockups, wireframes, or clear descriptions (for frontend work)
- **Data Model Changes**: Schema changes, migrations needed (for backend/database work)
- **API Contracts**: Endpoints, request/response formats (for API work)

#### Nice to Have:
- **Test Scenarios**: Edge cases and expected behaviors
- **Non-functional Requirements**: Logging, monitoring, error handling
- **Rollback Strategy**: How to undo if deployment fails

### Step 3: Repository Context Assessment
1. Check if issue specifies target repositories
2. If repositories mentioned, validate they exist in the `repos/` directory
3. Look for repository-specific documentation:
   - Check for `CONTRIBUTING.md`, `DEVELOPMENT.md`, or similar in each repo
   - Check for architecture documentation
4. If repository context is unclear, flag as missing information

### Step 4: Generate Analysis Report
If issue LACKS sufficient information:
- **STOP** (do not proceed to breakdown)
- Generate a report listing:
  - ✅ What information IS present
  - ❌ What information is MISSING (be specific)
  - 💡 Suggested questions to ask the issue author
  - 🔗 Which sections of the issue need clarification
- Present this to the user
- Recommend updating the Github issue before proceeding

If issue HAS sufficient information:
- Proceed to Step 5

If no, STOP and report that the issue is not sufficient for solution proposal. Ask the user to clarify or update the issue with the missing information before proceeding.

### Step 5: Generate RESEARCH.md

1. Save the audit results to: `issues/{ISSUE_NUMBER}/{ISSUE_NUMBER}_research.md`
2. Report success with file location
3. Provide brief summary of the breakdown and ask the user for clarification of any assumptions or uncertainties before proceeding to solution proposal. Use a list of questions to confirm understanding and clarify any ambiguities. Update the `RESEARCH.md` with any additional information obtained from the user.

## Output Examples

### Insufficient Information Example:
```
❌ Issue ONBOARD-1234 lacks sufficient information for breakdown

✅ Present:
  - Problem statement
  - User story

❌ Missing Critical Information:
  - Target repository not specified (which codebase needs changes?)
  - No acceptance criteria defined
  - Technical approach unclear (frontend? backend? both?)

💡 Suggested Questions:
  1. Which repository/repositories need to be modified?
  2. What are the specific acceptance criteria for this feature?
  3. Is this a frontend, backend, or full-stack change?
  4. Are there any API contract changes required?

🔗 Recommendation:
  Update the Jira ticket with the missing information before proceeding with implementation planning.
```

### Sufficient Information Example:
```
✅ Ticket ONBOARD-1234 has sufficient information for breakdown

📋 Generated PRD saved to: tickets/ONBOARD-1234/ONBOARD-1234_prd.xml

Summary:
  - 5 implementation tasks identified
  - Affects 2 repositories: onboarding-api, onboarding-ui
  - Components: Backend API (3 tasks), Frontend UI (2 tasks)
  - Estimated complexity: Moderate
  - Requires database migration

The PRD is ready for handoff to a junior developer or coding agent.
```

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

### For Ticket Authors:
- Always specify target repositories in the ticket
- Include clear acceptance criteria
- Provide technical context and constraints
- Link related tickets and dependencies

### For This Skill:
- Be strict about sufficiency checks (better to flag issues early)
- Break down tasks to appropriate granularity for junior developers
- Include verification methods for each acceptance criterion
- Consider cross-repository dependencies
- Flag potential risks or complex areas that may need senior review

## Error Handling

- **Ticket not found**: Try fetching with jira-ticket-fetcher skill
- **Fetch fails**: Report error and stop
- **Insufficient information**: Do NOT continue, report gaps instead
- **Repository not found**: Flag in analysis report
- **Ambiguous scope**: Request clarification from user
