# analyze-commits.ps1 - Analyze commit history and identify issues
# Usage: powershell -File analyze-commits.ps1 -BaseBranch main
#
# Example: .\analyze-commits.ps1 -BaseBranch main

param(
    [string]$BaseBranch = "main"
)

Write-Host "=== Analyzing commits on current branch vs $BaseBranch ===" -ForegroundColor Cyan
Write-Host ""

# Count commits
$commitCount = (git rev-list --count "$BaseBranch..HEAD") -as [int]
Write-Host "📊 Total commits ahead of $BaseBranch`: $commitCount" -ForegroundColor Cyan
Write-Host ""

# Show commit list
Write-Host "📜 Commits (newest first):" -ForegroundColor Cyan
git log --oneline "$BaseBranch..HEAD"
Write-Host ""

# Detect problematic patterns
Write-Host "🔍 Identifying issues..." -ForegroundColor Yellow
Write-Host ""

$issues = 0

# Check for throwaway commits
$throwaway = (git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^\w+ (wip|tmp|temp|fix|test|debug)$' -List).Count
if ($throwaway -gt 0) {
    Write-Host "⚠️  Found $throwaway throwaway commit(s):" -ForegroundColor Yellow
    git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^\w+ (wip|tmp|temp|fix|test|debug)$'
    $issues++
    Write-Host ""
}

# Check for vague messages
$vague = (git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^\w+ (adding|update|changes|modify|add)' -List).Count
if ($vague -gt 0) {
    Write-Host "⚠️  Found $vague commit(s) with vague messages:" -ForegroundColor Yellow
    git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^\w+ (adding|update|changes|modify|add)'
    $issues++
    Write-Host ""
}

# Check for revert/fixup commits
$reverts = (git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '(revert|fixup|squash)' -List).Count
if ($reverts -gt 0) {
    Write-Host "⚠️  Found $reverts revert/fixup commit(s):" -ForegroundColor Yellow
    git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '(revert|fixup|squash)'
    $issues++
    Write-Host ""
}

# Check for Conventional Commits compliance
$conventional = (git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^[a-f0-9]+ (feat|fix|refactor|perf|test|docs|chore|ci|style|build)' -List).Count
$total = $commitCount
if ($conventional -lt $total) {
    Write-Host "⚠️  Conventional Commits compliance: $conventional/$total commits" -ForegroundColor Yellow
    Write-Host "    Commits NOT following convention:" -ForegroundColor Yellow
    git log --oneline "$BaseBranch..HEAD" | Select-String -Pattern '^[a-f0-9]+ (feat|fix|refactor|perf|test|docs|chore|ci|style|build)' -NotMatch
    $issues++
    Write-Host ""
}

# Summary
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
if ($issues -eq 0) {
    Write-Host "✅ History looks clean! Ready to merge." -ForegroundColor Green
} else {
    Write-Host "❌ Found $issues category(ies) of issues. Consider running:" -ForegroundColor Red
    Write-Host "   git rebase -i $BaseBranch" -ForegroundColor Yellow
}
