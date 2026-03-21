---
name: repository-management
description: Manage repositories in multi-repo environments by onboarding, analyzing, and maintaining the REPOSITORIES.md catalog. Use when asked to add/remove/update repositories, onboard new repositories from GitHub, or manage the REPOSITORIES.md file. Automatically clones repos, analyzes their purpose using documentation (README, copilot-instructions, CLAUDE, AGENTS, docs), and updates REPOSITORIES.md with domain context, type (UI/API/backend/infrastructure/library), technologies, and purpose to help other agents determine where changes should be made across multiple repositories.
---

# Repository Manager

Manage multi-repository environments by maintaining a REPOSITORIES.md catalog that helps other agents understand the purpose, domain, and technology of each repository.

## Core Operations

### 1. Onboard Repository (Complete Flow)

When asked to "onboard [org/repo]" or "add repository [org/repo] from GitHub", follow this complete workflow:

**Step 1: Clone Repository**

```bash
cd /repos
git clone https://github.com/[org]/[repo].git [repo-name]
```

**Step 2: Analyze Repository**

Use a subagent to explore the repository and extract metadata:

```
Launch explore agent with prompt:
"Analyze the repository at repos/[repo-name] to extract:

1. **Repository Purpose**: What does this repository do? What domain/business area?
2. **Repository Type**: Is this a UI/frontend, API/backend service, infrastructure/IaC, shared library, or utilities?
3. **Technologies**: What programming languages, frameworks, and key technologies?
4. **When to Modify**: When would a developer need to make changes to this repository? What types of features/bugs?
5. **Documentation Found**: Which files did you find? (README.md, .github/copilot-instructions.md, CLAUDE.md, AGENTS.md, docs/*)

Read these files in priority order:
- README.md (always start here)
- .github/copilot-instructions.md
- CLAUDE.md or .claude.md
- AGENTS.md or .agents.md  
- docs/ directory files

Provide a structured summary suitable for REPOSITORIES.md."
```

**Step 3: Update REPOSITORIES.md**

Edit `repos/REPOSITORIES.md` to add a new entry using this template:

```markdown
### [Repository Title]

**Name:** `[repo-name]`  
**Type:** `[backend|frontend|infrastructure|library|utilities]`  
**Description:** [One-line description of purpose]  
**Purpose:** [When to modify this repo - what features/changes belong here]  
**Domain:** [Business domain/area - e.g., account onboarding, payments, user management]  
**Technologies:** [Language, Framework, Key Tech]  
**Documentation:** `repos/[repo-name]/README.md`  

---
```

Insert the new entry before the "## Repository Types" section, keeping entries alphabetically organized.

**Example Complete Entry:**

```markdown
### Onboarding API

**Name:** `onboarding-api`  
**Type:** `backend`  
**Description:** REST API for account onboarding flow  
**Purpose:** Modify when adding/changing user registration endpoints, verification logic, profile setup flows, or onboarding business rules  
**Domain:** Account Onboarding  
**Technologies:** Java, Spring Boot, PostgreSQL, Redis  
**Documentation:** `repos/onboarding-api/README.md`  

---
```

### 2. Add Repository (Already Cloned)

When the repository is already in `/repos` but not in REPOSITORIES.md:

1. Skip cloning
2. Follow Steps 2-3 from Onboard flow

### 3. Remove Repository

When asked to "remove [repo-name]":

**Step 1: Remove from REPOSITORIES.md**

Edit `repos/REPOSITORIES.md` to delete the entry for the repository (including the `---` separator).

**Step 2: Optionally Remove Directory**

Ask the user if they want to delete the repository directory:

```bash
rm -rf repos/[repo-name]
```

**Important:** Always confirm before deleting directories.

### 4. Update Repository Entry

When asked to "update [repo-name] details" or specific fields need updating:

1. Read current entry from REPOSITORIES.md
2. Ask user what to update (or apply requested changes)
3. Edit the entry in REPOSITORIES.md with new information

## REPOSITORIES.md Structure

The file follows this format:

```markdown
# Repository Context

[Intro text]

## Repositories

### [Repository Name]

**Name:** `repo-name`  
**Type:** `backend|frontend|infrastructure|library|utilities`  
**Description:** Brief description  
**Purpose:** When to make changes here  
**Domain:** Business domain  
**Technologies:** Tech stack  
**Documentation:** `repos/repo-name/README.md`  

---

[More repositories...]

## Repository Types

- **backend**: APIs, services, microservices
- **frontend**: Web applications, user interfaces  
- **infrastructure**: IaC, deployment configs, DevOps
- **library**: Shared code, components, utilities
- **utilities**: Tools, scripts, helper services
```

## Documentation Priority

When analyzing repositories, read documentation in this priority order:

1. **README.md** - Start here, usually has overview
2. **.github/copilot-instructions.md** - Copilot-specific context
3. **CLAUDE.md** or **.claude.md** - Claude-specific instructions
4. **AGENTS.md** or **.agents.md** - AI agent instructions
5. **docs/** directory - Additional documentation

Extract:
- Purpose and domain from overviews
- Type from architecture/structure descriptions
- Technologies from setup/prerequisites sections
- When to modify from contributing/development sections

## Best Practices

1. **Use Subagents**: Always use explore agents to analyze repositories - they can read multiple files and synthesize information effectively
2. **Be Thorough**: Read multiple documentation files to get complete picture
3. **Domain Context**: Focus on "when to modify" - this helps other agents route work correctly
4. **Keep Organized**: Maintain alphabetical order in REPOSITORIES.md
5. **Verify Cloning**: Ensure git clone succeeds before analysis
6. **Preserve Format**: Maintain consistent formatting with existing entries
