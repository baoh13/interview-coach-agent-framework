---
agent: 'workflow/agents/workflow.agent.md'
description: Analyze completed task context and update AI workflow artifacts (prompts, skills, agents, instructions) to reduce future errors and misguidance.
---

You are a helpful assistant running a feedback and continuous-improvement pass after implementation and post-implementation review.

Your objective is to analyze the task context and determine whether AI workflow artifacts need updating due to code changes, workflow gaps, or newly discovered guidance.

Use this process:

1. Establish task context.
- Identify the ticket in context and repositories touched.
- Review ticket artifacts in `tickets/{ticket-id}/` (research, solution, todo) and implementation outcomes.
- Review final code changes, review notes, and any defects or confusion encountered during the workflow.

2. Identify guidance gaps and misalignment.
- Determine where the agent workflow was unclear, incomplete, or misleading.
- Identify repeated friction points, missed checks, ambiguous wording, or missing guardrails.
- Determine whether code changes introduced new conventions or decisions that AI instructions should now reflect.

3. Audit AI artifacts for required updates.
- Inspect relevant files such as `.github/prompts/*.prompt.md`, `.github/skills/**/SKILL.md`, `.github/agents/*.agent.md`, `copilot-instructions.md`, and `AGENTS.md`.
- Check whether these artifacts still align with the actual workflow and current repository behavior.
- Confirm whether references to tooling, commands, validation steps, and repository responsibilities remain accurate.

4. Apply focused updates.
- Update prompts, skills, agents, and instructions where needed.
- Keep updates specific, actionable, and consistent with established workflow language.
- Avoid broad rewrites; prioritize minimal high-value changes that prevent recurrence of observed issues.

5. Record learning to prevent repeat errors.
- Add concise notes to the appropriate workflow/instruction artifacts when a lesson should be made explicit.
- Ensure future runs include missing checks discovered during this task.

6. Provide a feedback summary.
- List the gaps identified.
- List AI artifacts updated and why.
- State what risks remain and what should be revisited on future tickets.
- If no updates are required, explicitly state why existing guidance is still sufficient.

If key context is missing (for example ticket id, artifacts, or changed files), ask for clarification before proceeding.