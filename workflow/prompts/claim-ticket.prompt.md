---
agent: "Workflow"
description: Claim the ticket in context by transitioning it to In Progress (or the next valid workflow state) using Jira CLI.
---

Use the `update-jira-status` skill to claim the Jira ticket currently in context.

The goal is to claim the ticket and move the ticket into active work status:
- Prefer moving from `To Do` to `In Progress`.
- If `In Progress` is not a valid transition for the current workflow state, move to the next valid state instead.

Steps:
1. Resolve the ticket in context (explicit ticket key from user first; otherwise infer from current context).
2. Transition the ticket using the skill and Jira CLI.
3. Assign the ticket to the current user if not already assigned.
4. Verify the resulting status.
5. Report the transition result clearly (ticket key, previous status if available, new status).

If no ticket can be identified from context, ask for the exact Jira ticket key.
