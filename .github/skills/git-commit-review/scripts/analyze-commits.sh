#!/bin/bash
# analyze-commits.sh - Analyze commit history and identify issues
# Usage: bash analyze-commits.sh [BASE_BRANCH]
#
# Example: bash analyze-commits.sh main

BASE_BRANCH="${1:-main}"

echo "=== Analyzing commits on current branch vs $BASE_BRANCH ==="
echo ""

# Count commits
COMMIT_COUNT=$(git rev-list --count $BASE_BRANCH..HEAD)
echo "📊 Total commits ahead of $BASE_BRANCH: $COMMIT_COUNT"
echo ""

# Show commit list
echo "📜 Commits (newest first):"
git log --oneline $BASE_BRANCH..HEAD
echo ""

# Detect problematic patterns
echo "🔍 Identifying issues..."
echo ""

ISSUES=0

# Check for throwaway commits
THROWAWAY=$(git log --oneline $BASE_BRANCH..HEAD | grep -iE '^\w+ (wip|tmp|temp|fix|test|debug)$' | wc -l)
if [ "$THROWAWAY" -gt 0 ]; then
    echo "⚠️  Found $THROWAWAY throwaway commit(s):"
    git log --oneline $BASE_BRANCH..HEAD | grep -iE '^\w+ (wip|tmp|temp|fix|test|debug)$'
    ISSUES=$((ISSUES + 1))
    echo ""
fi

# Check for vague messages
VAGUE=$(git log --oneline $BASE_BRANCH..HEAD | grep -iE '^\w+ (adding|update|changes|modify|fix|add)' | wc -l)
if [ "$VAGUE" -gt 0 ]; then
    echo "⚠️  Found $VAGUE commit(s) with vague messages:"
    git log --oneline $BASE_BRANCH..HEAD | grep -iE '^\w+ (adding|update|changes|modify|fix|add)'
    ISSUES=$((ISSUES + 1))
    echo ""
fi

# Check for revert/fixup commits
REVERTS=$(git log --oneline $BASE_BRANCH..HEAD | grep -iE '(revert|fixup|squash)' | wc -l)
if [ "$REVERTS" -gt 0 ]; then
    echo "⚠️  Found $REVERTS revert/fixup commit(s):"
    git log --oneline $BASE_BRANCH..HEAD | grep -iE '(revert|fixup|squash)'
    ISSUES=$((ISSUES + 1))
    echo ""
fi

# Check for Conventional Commits compliance
CONVENTIONAL=$(git log --oneline $BASE_BRANCH..HEAD | grep -E '^[a-f0-9]+ (feat|fix|refactor|perf|test|docs|chore|ci|style|build)' | wc -l)
TOTAL=$((COMMIT_COUNT))
if [ "$CONVENTIONAL" -lt "$TOTAL" ]; then
    echo "⚠️  Conventional Commits compliance: $CONVENTIONAL/$TOTAL commits"
    echo "    Commits NOT following convention:"
    git log --oneline $BASE_BRANCH..HEAD | grep -vE '^[a-f0-9]+ (feat|fix|refactor|perf|test|docs|chore|ci|style|build)'
    ISSUES=$((ISSUES + 1))
    echo ""
fi

# Summary
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
if [ "$ISSUES" -eq 0 ]; then
    echo "✅ History looks clean! Ready to merge."
else
    echo "❌ Found $ISSUES category(ies) of issues. Consider running:"
    echo "   git rebase -i $BASE_BRANCH"
fi
