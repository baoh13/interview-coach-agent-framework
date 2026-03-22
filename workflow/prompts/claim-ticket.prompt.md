---
agent: 'workflow/agents/workflow.agent.md'
description: Claim the issue in context by assigning it to yourself and transitioning it to In Progress (or the next valid workflow state) using Github CLI. Use when user requests 'Claim issue', 'Assign to me', 'Start working on this', or similar phrases indicating they want to take ownership of the issue and begin work.
---

Use the `update-issue-status` skill to claim the Github issue currently in context.

The goal is to claim the issue and move the issue into active work status:
- Prefer moving from `To Do` to `In Progress`.
- If `In Progress` is not a valid transition for the current workflow state, move to the next valid state instead.

Steps:
1. Resolve the issue in context (explicit issue number from user first; otherwise infer from current context).
2. Transition the issue using the skill and Github CLI.
3. Assign the issue to the current user if not already assigned.
4. Verify the resulting status.
5. Report the transition result clearly (issue number, previous status if available, new status).

If no issue can be identified from context, ask for the exact Github issue number.
