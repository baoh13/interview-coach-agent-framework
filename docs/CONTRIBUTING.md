# Contributing to Interview Coach with Microsoft Agent Framework

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit <https://cla.opensource.microsoft.com>.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

- [Code of Conduct](#coc)
- [Issues and Bugs](#issue)
- [Feature Requests](#feature)
- [Submission Guidelines](#submit)

## <a name="coc"></a> Code of Conduct

Help us keep this project open and inclusive. Please read and follow our [Code of Conduct](https://opensource.microsoft.com/codeofconduct/).

## <a name="issue"></a> Found an Issue?

If you find a bug in the source code or a mistake in the documentation, you can help us by
[submitting an issue](#submit-issue) to the GitHub Repository. Even better, you can
[submit a Pull Request](#submit-pr) with a fix.

## <a name="feature"></a> Want a Feature?

You can *request* a new feature by [submitting an issue](#submit-issue) to the GitHub
Repository. If you would like to *implement* a new feature, please submit an issue with
a proposal for your work first, to be sure that we can use it.

- **Small Features** can be crafted and directly [submitted as a Pull Request](#submit-pr).

## <a name="submit"></a> Submission Guidelines

### <a name="submit-issue"></a> Submitting an Issue

Before you submit an issue, search the archive, maybe your question was already answered.

If your issue appears to be a bug, and hasn't been reported, open a new issue.
Help us to maximize the effort we can spend fixing issues and adding new
features, by not reporting duplicate issues.  Providing the following information will increase the
chances of your issue being dealt with quickly:

- **Overview of the Issue** - if an error is being thrown a non-minified stack trace helps
- **Version** - what version is affected (e.g. 0.1.2)
- **Motivation for or Use Case** - explain what are you trying to do and why the current behavior is a bug for you
- **Browsers and Operating System** - is this a problem with all browsers?
- **Reproduce the Error** - provide a live example or a unambiguous set of steps
- **Related Issues** - has a similar issue been reported before?
- **Suggest a Fix** - if you can't fix the bug yourself, perhaps you can point to what might be
  causing the problem (line of code or commit)

You can file new issues by providing the above information at the corresponding repository's [issues link](https://github.com/Azure-Samples/interview-coach-agent-framework/issues/new).

### <a name="submit-pr"></a> Submitting a Pull Request (PR)

Before you submit your Pull Request (PR) consider the following guidelines:

- [Search the repository](https://github.com/Azure-Samples/interview-coach-agent-framework/pulls) for an open or closed PR
  that relates to your submission. You don't want to duplicate effort.
- Make your changes in a new git fork
- **Commit your changes following [Conventional Commits](#commit-format)** (see below)
- Push your fork to GitHub
- In GitHub, create a pull request
- If we suggest changes then:
  - Make the required updates
  - Rebase your fork and force push to your GitHub repository (this will update your Pull Request):

    ```shell
    git rebase main -i
    git push -f
    ```
- Ensure all commits follow the **Conventional Commits** format — the pre-push hook will validate this automatically

## <a name="commit-format"></a> Commit Message Format

We follow the [**Conventional Commits**](https://www.conventionalcommits.org/) specification to maintain a clean, readable commit history.

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type

Must be one of the following:

- **feat** — A new feature
- **fix** — A bug fix
- **refactor** — Code change that neither fixes a bug nor adds a feature
- **perf** — Code change that improves performance
- **test** — Adding or updating tests
- **docs** — Documentation only changes
- **chore** — Changes to build process, dependencies, or tooling
- **ci** — Changes to CI/CD configuration (.github/workflows, etc.)
- **style** — Code style changes (formatting, missing semicolons, etc.) — use `prettier` or `dotnet format`
- **build** — Changes to build system or dependencies

### Scope (Optional)

The scope specifies what part of the codebase is affected:

- `agent` — Agent orchestration and workflow
- `api` — API endpoints or MCP tool definitions
- `ui` — Frontend or Blazor components
- `db` — Database schema or persistence
- `docs` — Documentation
- `skill` — Skills or agent customizations
- Leave blank for changes that affect multiple areas

### Subject

- Use the **imperative mood** ("add" not "added" or "adds")
- Do **not** capitalize the first letter
- **No period (.)** at the end
- Limit to **50 characters**
- Examples:
  - `feat(agent): add batch processing for discovery phase`
  - `fix(ui): resolve loading state on chat widget`
  - `docs: update ARCHITECTURE with multi-service diagram`

### Body (Optional but Recommended)

- Wrap at **72 characters**
- Explain **what** and **why**, not **how** (the code shows that)
- Separate from subject with a blank line
- Example:
  ```
  feat(api): add batching for user imports
  
  Improve performance by processing user imports in configurable batch
  sizes instead of one at a time. Prevents database transaction timeouts
  for large imports.
  
  - Add DiscoveryFlowLimits configuration constant
  - Implement AddAskedQuestionsBatchAsync in repository
  - Add comprehensive batch processing tests
  ```

### Footer (Optional)

Reference related issues:

```
Fixes #123
Closes #456
Related to #789
```

### Examples

**Good:**
```
feat(agent): add support for multi-turn discovery follow-ups

Enable agents to ask follow-up questions based on lead responses,
improving data collection without user friction.
```

```
fix(ui): resolve race condition in chat message rendering
```

```
chore: upgrade Microsoft.SemanticKernel to 1.5.0
```

**Bad:**
```
update stuff                    # Too vague
Fix bug                         # Capitalized, no type
WIP                            # Incomplete
feat: Add support for...       # Capitalized
```

## Local Setup & Git Hooks

### Initial Setup

After cloning the repository, run the setup script **once** to configure Git hooks:

**Windows (PowerShell):**
```powershell
.\scripts\setup-hooks.ps1
```

**Mac/Linux (Bash):**
```bash
bash scripts/setup-hooks.sh
```

This configures:
- **pre-push hook** — Validates all commits against Conventional Commits before pushing
- **Environment variables** — `GIT_SKIP_VALIDATION=1` to bypass validation locally (not recommended)

### Cleaning Up Commit History

If you have commits that don't follow the format, the pre-push hook will block the push and show you how to fix it.

**Use the git-commit-review skill for comprehensive history cleanup:**

```
/git-commit-review
```

This skill provides:
- Analysis of problematic commit patterns
- Step-by-step interactive rebase guidance
- Detection of vague messages, throwaway commits, fragmented work
- Troubleshooting for complex rebase scenarios

See `.github/skills/git-commit-review/SKILL.md` for full documentation.

### Fixing Non-Compliant Commits

If the pre-push hook blocks your commit:

```bash
# Interactive rebase to reword commits
git rebase -i origin/main

# Mark commits as 'reword', update messages to follow format, then:
git push
```

Or use the automated setup:
```bash
# Set your commit message template for guidance
git config commit.template .gitmessage
```

## Code Quality

- **Format code** before committing: `dotnet format` (C#) or format command for your language
- **Run tests** to verify nothing is broken: `dotnet test InterviewCoach.slnx`
- **Linting** — Follow codebase conventions (see `.github/instructions/`)
- **Documentation** — Update docs if you change public APIs or architecture

## Questions?

- **Commit questions?** Check out Conventional Commits: https://www.conventionalcommits.org/
- **Workflow questions?** See git-commit-review skill or PRE-PUSH-VALIDATION.md in `.github/hooks/`
- **Architecture questions?** See `docs/ARCHITECTURE.md` and `docs/MULTI-AGENT.md`

That's it! Thank you for your contribution!
