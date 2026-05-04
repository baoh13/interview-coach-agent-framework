#!/bin/bash
# .githooks/validate-conventional-commits.sh
#
# Validates that all commits follow Conventional Commits format.
# Format: type(scope): subject or type: subject
# 
# Valid types: feat, fix, refactor, perf, test, docs, chore, ci, style, build

VALID_TYPES="feat|fix|refactor|perf|test|docs|chore|ci|style|build"
PATTERN="^(${VALID_TYPES})(\(.+\))?: .+"

# Get the list of commits to be pushed
# For a push, we get commits not yet in the remote branch
REMOTE="${1:-origin}"
BRANCH=$(git rev-parse --abbrev-ref HEAD)

# If this is a detached HEAD or we can't determine the branch, validate HEAD
if [ "$BRANCH" = "HEAD" ]; then
    echo "⚠️  Detached HEAD - validating just the current commit"
    COMMITS=$(git rev-list --max-count=1 HEAD)
else
    # Get commits that are ahead of the remote
    if git rev-parse "${REMOTE}/${BRANCH}" > /dev/null 2>&1; then
        # Remote branch exists
        COMMITS=$(git rev-list "${REMOTE}/${BRANCH}..HEAD")
    else
        # Remote branch doesn't exist yet - validate all commits
        if git rev-parse "${REMOTE}/main" > /dev/null 2>&1; then
            COMMITS=$(git rev-list "${REMOTE}/main..HEAD")
        elif git rev-parse "${REMOTE}/master" > /dev/null 2>&1; then
            COMMITS=$(git rev-list "${REMOTE}/master..HEAD")
        else
            echo "⚠️  Can't find remote base branch - validating all local commits"
            COMMITS=$(git rev-list HEAD)
        fi
    fi
fi

if [ -z "$COMMITS" ]; then
    echo "✅ No commits to push"
    exit 0
fi

FAILED_COMMITS=0
FAILED_LIST=""

echo "🔍 Validating commit messages against Conventional Commits..."
echo ""

while IFS= read -r COMMIT; do
    MESSAGE=$(git log -1 --format=%B "$COMMIT" | head -1)
    SHORT=$(git log -1 --format=%h "$COMMIT")
    
    if ! [[ $MESSAGE =~ $PATTERN ]]; then
        echo "❌ $SHORT: $MESSAGE"
        FAILED_COMMITS=$((FAILED_COMMITS + 1))
        FAILED_LIST="$FAILED_LIST\n  $SHORT: $MESSAGE"
    else
        echo "✅ $SHORT: $MESSAGE"
    fi
done <<< "$COMMITS"

echo ""
if [ $FAILED_COMMITS -gt 0 ]; then
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "❌ Validation failed: $FAILED_COMMITS invalid commit(s)"
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo ""
    echo "Valid types: $VALID_TYPES"
    echo "Format:      type(scope): subject"
    echo "Example:     feat(api): add batch processing"
    echo ""
    exit 1
else
    echo "✅ All commits follow Conventional Commits format"
    exit 0
fi
