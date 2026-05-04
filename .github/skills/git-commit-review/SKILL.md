---
name: git-commit-review
description: 'Review commit history for cleanliness and identify commits that should be squashed. Use when preparing to merge a feature branch, cleaning up commit history, or following Conventional Commits standards.'
argument-hint: 'Specify target base branch (default: main) and desired outcome'
---

# Git Commit Review & Cleanup

## When to Use

- **Before merging** feature branches to main/develop
- **Preparing PRs** with messy or fragmented commit history
- **Adopting Conventional Commits** on messy branches
- **Spring cleaning** on long-running development branches
- **Team code review** to ensure clean history standards

## What This Skill Does

Systematically analyzes branch history, identifies problematic patterns, and offers automated squashing to produce clean, Conventional Commits-compliant history.

## Key Problems Detected

| Issue | Pattern | Fix |
|-------|---------|-----|
| **Throwaway commits** | "wip", "temp", "fix", single words | Squash into related feature commit |
| **Vague messages** | "adding X", "update", "changes" | Rewrite with proper Conventional Commits format |
| **Fragmented work** | 5+ sequential commits for one feature | Combine into 1-3 logical commits |
| **Reverts/fixups** | "revert X", "fix previous", "fix tests" | Squash into the commit being fixed |
| **Merge commits** | "Merge branch..." in history | Rebase to flatten history |

## Procedure

### 1. Analyze Current History

**Steps:**
1. Run `git log --oneline [BASE_BRANCH]..HEAD` to list unpushed commits
2. Identify problematic patterns from the table above
3. Document which commits should be squashed and why

**Example output to look for:**
```
5032d96 feat: Implement batch processing
fcc62e0 wip                          ← throwaway
7763287 Implement Baoh Assistant     ← vague
919198f wip                          ← throwaway
27491f9 Add user stories             ← ok
118351a Add shared project           ← part of fragmented set
e946dd0 adding chat component        ← vague
```

### 2. Create Squash Plan

**Interactive approach (recommended):**
1. Run `git rebase -i [BASE_BRANCH]`
2. Change `pick` to `squash` (or `s`) for commits to combine
3. Reorder commits so related work is grouped
4. Let git guide you through merge conflict resolution

**Automated approach (bulk squashing):**
1. Identify the N commits to squash (count from `git log --oneline`)
2. Run `git rebase -i HEAD~N`
3. Mark commits with `squash` in the editor
4. Write clean combined message following Conventional Commits format

### 3. Write Clean Commit Messages

Use **Conventional Commits** format:

```
<type>(<scope>): <subject>
<blank line>
<body>
```

**Allowed types:** `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `ci`, `style`, `build`

**Examples:**
- ✅ `feat(api): add batch processing for user imports`
- ✅ `fix(ui): resolve loading state bug in chat widget`
- ❌ `update stuff`
- ❌ `wip`

### 4. Verify & Resolve Conflicts

During interactive rebase:
1. If conflicts occur, resolve manually or use `git merge --ours`/`--theirs`
2. Run `git add .` to mark resolved
3. Run `git rebase --continue` to proceed
4. If stuck, run `git rebase --abort` to start over (safe)

### 5. Validate Final History

**Steps:**
1. Run `git log --oneline [BASE_BRANCH]..HEAD` to verify cleanliness
2. Run `git log -p HEAD~[N]..HEAD` to spot-check commit contents
3. Run tests: `npm test` / `dotnet test` to ensure nothing broke
4. Use `git diff [BASE_BRANCH]` to verify final changeset matches expectations

**Success criteria:**
- ✅ No "wip", "temp", "fix", or vague commits
- ✅ Each commit has a clear, descriptive message
- ✅ Related work is grouped logically (1-5 commits for medium features)
- ✅ All tests pass
- ✅ Diff against base branch is clean and complete

### 6. Push (if authorized)

```bash
# If branch is already pushed, force-push carefully
git push origin [BRANCH_NAME] --force-with-lease

# Otherwise, safe push
git push
```

## Troubleshooting

### "I got conflicts during rebase"
- Resolve conflicts in affected files
- Run `git add .` to mark them resolved
- Run `git rebase --continue` to proceed
- If too messy, `git rebase --abort` to reset

### "I only want to squash a few commits, not all"
- Count commits to include: `git log --oneline origin/[BASE]..HEAD | wc -l`
- Run `git rebase -i HEAD~[NUMBER]` to rebase only those
- Or run `git rebase -i origin/[BASE]` to rebase everything

### "My history is on origin/ and I can't push"
- Ensure you have force-push permissions
- Use `--force-with-lease` (prevents accidents): `git push --force-with-lease`
- Check with team before force-pushing shared branches

### "I squashed but the final history still looks fragmented"
- You may have over-squashed different concerns into one commit
- Consider splitting back out: `git rebase -i [BASE]` and mark commits as `edit`
- Or reverse: `git reset --soft [BASE]` and re-stage logical groups

## Related Skills & Workflows

- [**Conventional Commits Standard**](https://www.conventionalcommits.org/) — Key reference for commit format
- [**Interactive Rebase Guide**](https://git-scm.com/book/en/v2/Git-Tools-Rewriting-History) — Deep dive on `git rebase -i`
- Test-driven development skill → Often pairs with clean commits
- Code review skill → Reviewers often ask for history cleanup before merge
