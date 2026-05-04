# Pre-Push Hook: Conventional Commits Validation

## Overview

The **pre-push hook** automatically validates all commits being pushed to ensure they follow the [Conventional Commits](https://www.conventionalcommits.org/) standard.

This hook runs **before pushing** to the remote, preventing non-compliant commits from being shared with the team.

## How It Works

1. **Detects commits to be pushed** — Finds all local commits not yet in the remote branch
2. **Validates each commit message** — Checks format: `type(scope): subject`
3. **Blocks push if any commit fails** — Shows which commits need fixing
4. **Provides clear remediation** — Suggests using `git rebase -i` to fix messages

### Validation Rules

**Valid Format:**
```
type(scope): subject
type: subject
```

**Valid Types:** `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `ci`, `style`, `build`

**Examples:**
- ✅ `feat(api): add batch processing for user imports`
- ✅ `fix(ui): resolve loading state bug in chat`
- ✅ `chore: update dependencies`
- ❌ `update stuff`
- ❌ `wip`
- ❌ `Fix bug` (lowercase required)

## Setup

### First Time Setup

After cloning the repository, run the setup script **once**:

**Windows (PowerShell):**
```powershell
.\scripts\setup-hooks.ps1
```

**Mac/Linux (Bash):**
```bash
bash scripts/setup-hooks.sh
```

This configures Git to use the project's `.githooks` directory.

## Usage

### Normal Workflow

Push as usual — validation happens automatically:
```bash
git push
```

**If validation passes:** ✅ Push succeeds
```
🔍 Validating commit messages against Conventional Commits...
✅ abc1234: feat(api): add batch processing
✅ def5678: fix(ui): resolve loading state bug
✅ All commits follow Conventional Commits format
```

**If validation fails:** ❌ Push is blocked
```
❌ abc1234: wip
❌ def5678: update stuff
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
❌ Validation failed: 2 invalid commit(s)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Valid types: feat, fix, refactor, perf, test, docs, chore, ci, style, build
Format:      type(scope): subject
Example:     feat(api): add batch processing
```

### Fixing Non-Compliant Commits

When validation fails:

1. **Interactive rebase:**
   ```bash
   git rebase -i origin/main
   ```
   (Or replace `main` with your base branch)

2. **In the editor, mark commits as `reword`:**
   ```
   pick abc1234 wip
   pick def5678 update stuff
   ```
   Change to:
   ```
   reword abc1234 wip
   reword def5678 update stuff
   ```

3. **Reword each commit** with the proper format:
   ```
   feat(api): add batch processing
   fix(ui): resolve loading state bug
   ```

4. **Try pushing again:**
   ```bash
   git push
   ```

## Bypassing Validation

**Not recommended**, but possible for local testing:

```bash
GIT_SKIP_VALIDATION=1 git push
```

⚠️ **Only use this when you plan to squash/amend commits before the final push.**

## Troubleshooting

### "I pushed and now realize my commits don't match the format"

Don't panic — you can still fix it:

1. **Amend the most recent commit:**
   ```bash
   git commit --amend
   # Fix the message, save, and exit
   git push --force-with-lease
   ```

2. **Or rebase to fix multiple commits:**
   ```bash
   git rebase -i origin/main
   # Mark commits as 'reword', fix them, and force-push
   git push --force-with-lease
   ```

### "The hook isn't running"

Verify setup:
```bash
git config core.hooksPath
# Should output: .githooks

ls -la .githooks/
# Should show pre-commit and pre-push with execute permissions
```

If not set up, run the setup script again.

### "I need to push a commit that violates the format"

Before pushing non-compliant commits, you **must** squash or amend them:

```bash
git rebase -i origin/main
```

The team depends on clean commit history. If you're unsure about the format, ask in the team chat or check [Conventional Commits](https://www.conventionalcommits.org/).

## Integration with CI/CD

This pre-push hook is **local enforcement**. The team's CI/CD pipeline may also validate commits via GitHub Actions or similar tools.

**Both the local hook and CI validation should pass** before merging to main.

## Related Documentation

- **Conventional Commits:** https://www.conventionalcommits.org/
- **git-commit-review skill:** See `.github/skills/git-commit-review/SKILL.md` for detailed cleanup workflows
- **CONTRIBUTING.md:** Team guidelines on commit standards and code review
