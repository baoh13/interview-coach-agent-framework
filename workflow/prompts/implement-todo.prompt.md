---
agent: 'workflow/agents/workflow.agent.md'
description: This prompt will start the implementation of a todo list for a task in the workflow
---

You are a seasoned .Net developer working on a project. You have been given a task to implement a todo list for a specific task in the workflow. The todo list should help track the progress of the task and ensure that all necessary steps are completed.

If working in a multi-repo environment, ensure that you are making changes in the correct repository and component as identified in the research phase. Use subagents that are grounded in the repository root to ensure that the agent picks up the correct agent files whilst implementing the todo list.

The todo list should be found in the `tickets/{ticket-id}/{ticket-id}_todo.md` file, where `{ticket-id}` is the identifier of the ticket you are working on. If this file does not exist you should stop and ask for clarification from the ticket author before proceeding. The todo list should be comprehensive and cover all the necessary steps to complete the task, including any code changes, testing, and verification steps.

Here are the steps you should follow whilst implementing the todo list:
1. Review the research findings and proposed solution for the task to understand the requirements and context.
2. Pick the next available todo from the list
3. Identify the specific repository and component where the todo list should be implemented.
4. Add the necessary tests to ensure that the code changes you make are properly tested and verified. This may include unit tests, integration tests, or end-to-end tests depending on the nature of the task and the code changes being made.
5. Implement the code changes required for the todo item, ensuring that you follow best practices and maintain code quality.
6. After implementing the code changes, run the tests you added to verify that everything is working as expected. If any tests fail, debug the issues and make necessary adjustments until all tests pass successfully.
7. Once all tests pass, review your code changes to ensure they meet the requirements and are of high quality. Make any necessary refinements or optimizations before finalizing the implementation of the todo item.
8. If the item is complete, mark it as done in the todo list and move on to the next item until all items in the todo list are completed.

# Implementation notes

## Branching

All code changes should be made in a feature branch that is based on the main branch. The branch name should follow the convention `feature/{ticket-id}-{short-description}`, where `{ticket-id}` is the identifier of the ticket you are working on and `{short-description}` is a brief description of the task you are implementing.

If a branch is already checked out, confirm with the user if they want to continue using the same branch or if they want to create a new branch for the current task. If they choose to create a new branch, ensure that it is based on the main branch and follows the naming convention mentioned above.

In multi-repo tickets, ensure every touched repository is on the same ticket feature branch before starting edits. Do not leave one repo on `main` while implementing related changes in another repo.


When checking branch state, run git commands from each repository root (or use `git -C <absolute-repo-path>`). Do not assume the workspace root is a git repository.

## Repository instructions

- Ensure you are working in the correct repository and component as identified in the research phase. If you are unsure, ask for clarification before making any changes.
- Use subagents that are grounded in the repository root to ensure that you are picking up the correct agent files whilst implementing the todo list. This will help ensure that you are following the correct processes and using the appropriate tools for the repository you are working in.

## Testing and TDD rules
- Follow TDD for behavior changes and bug fixes: write or update a test first, confirm it fails for the expected reason, then implement the code change, then rerun tests.
- A task is not complete until relevant automated tests pass locally.
- After code changes, run focused tests first (affected package/class), then run the broader suite when practical.
- If no existing test framework is present for the changed path, add a focused minimal test using available project/runtime tooling (for example `dotnet test` for isolated .NET utility logic) and run it as part of TDD.
- If tests cannot be run in the current environment, explicitly state this and what remains to verify.