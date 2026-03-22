---
name: update-issue-status
description: Update the Github issue in context to the next workflow stage using Github CLI (for example, To Do -> In Progress).
user-invocable: false
---

# Update Github Issue Status Skill

## Description
Moves the Github issue in context to the next workflow stage using GitHub CLI (`gh`).

## When to Use
Use this skill when:
- User asks to move/transition/update a Github issue status
- User asks to move an issue in context to the next stage
- User gives a specific transition (for example: "move ISSUE-123 to In Progress")

## Capabilities
- Resolves the issue in context from user input or workspace context
- Assigns the issue to the current authenticated user with `gh issue edit --add-assignee "@me"`
- Transitions issue state with `gh issue move`
- Optionally adds a transition comment
- Verifies the new status after transition
- Provides clear error messages when transition is invalid

## Prerequisites
- GitHub CLI (`gh`) is installed and authenticated
- User has permission to transition the issue
- `gh auth status` has already been completed. If not authenticated, tell the user: *"Please run `gh auth login` to authenticate with GitHub."*

## Workflow

### Step 1: Resolve the issue in context
Determine the issue key in this order:
1. Explicit issue key from the user request (preferred)
2. Issue key in current branch name (for example `ISSUE-123-short-description`)
3. Issue key from active issue folder (for example `issues/ISSUE-123/`)

If no issue key can be determined, ask the user for the exact issue key.

### Step 2: Determine target status
- Assign the issue to the current user if not already assigned.

Use:

```bash
gh issue edit {ISSUE_KEY} --repo {OWNER}/{REPO} --add-assignee "@me"
```

### Step 3: Run transition command
Use:

```bash
gh issue move {ISSUE_KEY} "{TARGET_STATE}" --repo {OWNER}/{REPO}
```

Optional with comment:

```bash
gh issue move {ISSUE_KEY} "{TARGET_STATE}" --repo {OWNER}/{REPO} --comment "Starting implementation"
```

### Step 4: Verify result
Confirm updated status:

```bash
gh issue view {ISSUE_KEY} --repo {OWNER}/{REPO}
```

If transition failed, return actionable reason and next step.

## Examples

### Example A: Explicit transition
User: "Move ISSUE-123 to In Progress"

Command:
```bash
gh issue edit ISSUE-123 --repo {OWNER}/{REPO} --add-assignee "@me"
gh issue move ISSUE-123 "In Progress" --repo {OWNER}/{REPO}
```

### Example B: Context-based next stage
User: "Move the issue in context to next stage"

Actions:
1. Resolve issue key from context (for example `ISSUE-123`)
2. Assign issue to current user (`@me`)
3. Detect current status (`To Do`)
4. Transition to `In Progress`

Command:
```bash
gh issue edit ISSUE-123 --repo {OWNER}/{REPO} --add-assignee "@me"
gh issue move ISSUE-123 "In Progress" --repo {OWNER}/{REPO}
```

## Error Handling
- **Issue not found**: Ask user to confirm issue key and project access.
- **Invalid transition**: Ask user for exact allowed target state for that workflow.
- **Permission denied**: Ask user to request transition permission in Github.
- **CLI/auth error**: Ask user to re-run setup/auth (via `github-cli-setup`).

## Success Output
After success, report:
- Issue key
- Previous status (if available)
- New status
- Confirmation command result summary

Example:
`✅ ISSUE-123 moved from To Do to In Progress`

````