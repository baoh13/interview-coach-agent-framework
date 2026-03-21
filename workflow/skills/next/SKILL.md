---
name: next
description: Analyze Github issues to discover your next tasks. Shows assigned tickets, available work, code review and opportunities. Invoke with /next command. Use when user asks "What should I work on next?", "Find available issues", "Show my sprint tickets", "What needs review?", "What's upcoming in backlog?"
tools:
    [
        "github-issues/*",
        "search",
        "agent",
        "microsoft-docs/*"
    ]
user-invocable: false
---

## When to Use

Use this skill when:
- User types `/next` command
- User asks "what should I work on next?"
- User wants to see their assigned issues
- User wants to find available issues
- User wants to know which issues need code review
- User wants to see upcoming backlog priorities ~~

## Capabilities
- Shows your assigned issues
- Lists available unassigned issues
- Identifies issues in code review status (for review contributions)
- Displays high-priority issues
- Provides issue summaries, statuses, and priorities
- Outputs formatted markdown report

# Pre-flight
- Ensure you hae access to the `github` cli tool. Check using `gh auth status`.
- User must be authenticated with Github CLI
- Access to the Github project (default: {PROJECT_NAME} project)

# Available Commands

Given the command line a moment to fetch the latest data from Github, it will show a spinner whilst it does this:

Get existing issues summary:
`gh issue list --repo {PROJECT_NAME} --state open --json number,title,state,labels,assignees`


# Display Summary
Show inline summary with:
- Count of assigned issues
- Count of available issues
- Count of review opportunities
- Top 3 recommendations for what to work on next
- Link to full report file

## Usage Examples

**User:** `/next`

**Output:**
```
🎯 Analyzing your Github project...

📋 My Assigned Issues: 2
✅ AO-123 - Implement user validation [In Progress] {High}
📝 AO-456 - Fix login bug [To Do] {Medium}

🆓 Available Issues: 3
📝 AO-789 - Add API rate limiting [To Do] {High}
📝 AO-790 - Update documentation [To Do] {Low}
📝 AO-791 - Refactor auth service [To Do] {Medium}

👀 Code Review Opportunities: 1
🔍 AO-321 - Update password reset flow [Code Review] - @johndoe

💡 Recommendations:
1. Continue AO-123 (In Progress)
2. Pick up AO-789 (High Priority, unassigned)
3. Review AO-321 (Unblock @johndoe)

📄 Full report saved to: tickets/next-tasks-2026-02-13.md
```

# Preference

Deliver all content directly within the chat window using standard Markdown formatting. Do not generate downloadable files (.md, .txt, etc.) or use code blocks for prose. If the response is long, break it into sections within this conversation.