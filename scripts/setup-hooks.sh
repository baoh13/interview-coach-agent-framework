#!/usr/bin/env bash
# Configures git to use the project's .githooks directory.
# Run once after cloning: bash scripts/setup-hooks.sh
#
# Sets up:
#   - pre-commit: code formatting validation
#   - pre-push: Conventional Commits validation

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
HOOKS_DIR="$REPO_ROOT/.githooks"

git -C "$REPO_ROOT" config core.hooksPath .githooks

# Configure commit message template
git -C "$REPO_ROOT" config commit.template .gitmessage

# Make all hook scripts and validation scripts executable
chmod +x "$HOOKS_DIR/pre-commit" || true
chmod +x "$HOOKS_DIR/pre-push" || true
chmod +x "$HOOKS_DIR/validate-conventional-commits.sh" || true

echo ""
echo "✅ Git hooks configured:"
echo "   • pre-commit: code formatting validation"
echo "   • pre-push: Conventional Commits validation"
echo ""
echo "Environment variables:"
echo "   SKIP_BUILD=1              Skip build step during commits"
echo "   GIT_SKIP_VALIDATION=1     Skip commit message validation on push"
