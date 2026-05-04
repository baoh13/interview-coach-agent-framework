# Commit History Examples & Patterns

## Clean History (Ready to Merge)

```
8a02249 docs: update architecture and configuration
daa94d8 feat: implement batch processing and Baoh Assistant workflow
326bdca chore: add CI/CD infrastructure, agents, skills, hooks, and workflows
```

**Characteristics:**
- ✅ Each commit has a single, clear purpose
- ✅ Messages follow Conventional Commits format
- ✅ No throwaway or vague commits
- ✅ 3 commits for a complete feature: easy to review, understand, and revert if needed

---

## Messy History (Needs Cleanup)

```
5032d96 feat: Implement batch processing for asked questions
fcc62e0 wip                          ← Throwaway
7763287 Implement Baoh Assistant...  ← Vague message
919198f wip                          ← Throwaway
27491f9 Add user stories...          ← Could be combined with above
118351a Add shared project           ← Fragmented work
e946dd0 adding chat component        ← Vague message
7e7b18e plugin                       ← Incomplete message
9bfe009 hooks                        ← Vague
d92852f memory                       ← Vague
de68b32 ci: add GitHub Actions...    ← Good format, but grouped with vague
```

**Issues:**
- ❌ Multiple single-word or vague commits (error-prone pattern)
- ❌ Inconsistent commit message format
- ❌ Related work split across multiple files (7 commits for what should be 2-3)
- ❌ "wip" commits indicate incomplete work that should be squashed

---

## Squash Plan Example

**Before:**
```
5 commits: fragmented Baoh Assistant setup work
2 commits: vague CI/CD infrastructure
```

**Squash Strategy:**
```
pick   de68b32 ci: add GitHub Actions...
squash d92852f memory
squash 9bfe009 hooks
squash 7e7b18e plugin
squash e946dd0 adding chat component
squash 118351a Add shared project
pick   27491f9 Add user stories and architectural decisions for Baoh Assistant
squash 919198f wip
pick   7763287 Implement Baoh Assistant plan...
pick   5032d96 feat: Implement batch processing...
squash fcc62e0 wip
```

**After interactive rebase:**
```
326bdca chore: add CI/CD infrastructure, agents, skills, hooks, and workflows
27491f9 feat: add user stories for Baoh Assistant
7763287 feat: implement Baoh Assistant plan for non-repeating questions
daa94d8 feat: implement batch processing for asked questions
```

---

## Conventional Commits Reference

| Type | When | Example |
|------|------|---------|
| `feat` | New feature | `feat(api): add user authentication` |
| `fix` | Bug fix | `fix(chat): resolve message ordering bug` |
| `docs` | Documentation only | `docs: update API reference` |
| `chore` | Maintenance, deps, tooling | `chore: update dependencies` |
| `ci` | CI/CD changes | `ci: add GitHub Actions workflow` |
| `test` | Tests only | `test: add unit tests for parser` |
| `refactor` | Code restructure (no behavior change) | `refactor(api): simplify error handling` |
| `perf` | Performance improvements | `perf: optimize database queries` |
| `style` | Code style, formatting | `style: format with prettier` |
| `build` | Build system changes | `build: upgrade webpack config` |

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Rules:**
- Subject line: max 50 characters, lowercase, no period
- Scope: optional but recommended (area of code affected)
- Body: wrap at 72 characters, explain WHY not WHAT
- Footer: reference issues (Fixes #123)

---

## Troubleshooting Scenarios

### Scenario: Conflict During Rebase

**Symptom:**
```
CONFLICT (modify/delete): src/file.ts deleted in HEAD and modified in abc1234
error: could not apply abc1234... feat: some feature
```

**Solution:**
1. Edit the conflicted file(s) to resolve
2. If you want the file: `git add src/file.ts`
3. If you want to delete it: `git rm src/file.ts`
4. Run `git rebase --continue`

### Scenario: Forgot Which Commits to Squash

**Symptom:**
```
You're in the middle of rebasing but forgot your plan.
```

**Solution:**
1. Check `.git/rebase-merge/done` for completed steps
2. Check `.git/rebase-merge/git-todo` for remaining steps
3. Or abort and start over: `git rebase --abort`

### Scenario: Need to Split a Squashed Commit Back Out

**Symptom:**
```
You over-squashed and now need to separate concerns.
```

**Solution:**
1. Run `git rebase -i HEAD~3` (adjust number as needed)
2. Mark the over-squashed commit as `edit`
3. When paused: `git reset HEAD~1` (undo the squash)
4. `git add` individual files/hunks
5. `git commit` separate messages
6. `git rebase --continue`

---

## Safety Checks Before Pushing

### Pre-Push Checklist

- [ ] Ran tests: `dotnet test` / `npm test`
- [ ] Verified history: `git log --oneline [BASE].` 
- [ ] Checked diff: `git diff [BASE]` looks complete
- [ ] No debug code or console.logs left
- [ ] Linter passes: `dotnet format --verify` / `npm run lint`

### Safe Push Commands

```bash
# Safest: normal push (if not yet pushed)
git push

# Safe if branch already pushed: prevent accidental overwrites
git push --force-with-lease

# Risky: only if force-push is unavoidable
git push --force
```

**Recommendation:** Always use `--force-with-lease`, never bare `--force`.
