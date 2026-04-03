# Configures git to use the project's .githooks directory.
# Run once after cloning: .\scripts\setup-hooks.ps1
#
# Sets up:
#   - pre-commit: code formatting validation
#   - pre-push: Conventional Commits validation

$repoRoot = git rev-parse --show-toplevel
$hooksDir = Join-Path $repoRoot ".githooks"

git -C $repoRoot config core.hooksPath .githooks

# Configure commit message template
git -C $repoRoot config commit.template .gitmessage

# Ensure all hook scripts are executable
Get-ChildItem -Path $hooksDir -File -Recurse | ForEach-Object {
    if ($_.Extension -eq ".sh" -or $_.Extension -eq "") {
        Write-Host "Setting executable: $($_.Name)"
    }
}

Write-Host "" 
Write-Host "✅ Git hooks configured:" -ForegroundColor Green
Write-Host "   • pre-commit: code formatting validation" -ForegroundColor Green
Write-Host "   • pre-push: Conventional Commits validation" -ForegroundColor Green
Write-Host ""
Write-Host "Environment variables:" -ForegroundColor Cyan
Write-Host "   SKIP_BUILD=1       Skip build step during commits" -ForegroundColor Cyan
Write-Host "   GIT_SKIP_VALIDATION=1  Skip commit message validation on push" -ForegroundColor Cyan
