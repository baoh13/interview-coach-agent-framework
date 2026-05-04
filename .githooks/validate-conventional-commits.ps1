# .githooks/validate-conventional-commits.ps1
#
# Validates that all commits follow Conventional Commits format.
# Format: type(scope): subject or type: subject
# 
# Valid types: feat, fix, refactor, perf, test, docs, chore, ci, style, build

param(
    [string]$Remote = "origin"
)

$ValidTypes = @("feat", "fix", "refactor", "perf", "test", "docs", "chore", "ci", "style", "build")
$Pattern = "^(" + ($ValidTypes -join "|") + ")(\(.+\))?:\s.+"

# Get the current branch
$Branch = git rev-parse --abbrev-ref HEAD
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Detached HEAD - validating just the current commit" -ForegroundColor Yellow
    $Commits = @(git rev-list --max-count=1 HEAD)
} else {
    # Try to get commits ahead of remote
    $RemoteBranch = "$Remote/$Branch"
    
    # Check if remote branch exists
    $RemoteExists = $null
    try {
        $RemoteExists = git rev-parse $RemoteBranch 2>$null
    } catch {
        # Remote doesn't exist
    }
    
    if ($RemoteExists) {
        # Remote branch exists
        $Commits = @(git rev-list "$RemoteBranch..HEAD")
    } else {
        # Try to find a base branch
        $BaseFound = $false
        
        foreach ($base in @("$Remote/main", "$Remote/master")) {
            try {
                $test = git rev-parse $base 2>$null
                if ($LASTEXITCODE -eq 0) {
                    $Commits = @(git rev-list "$base..HEAD")
                    $BaseFound = $true
                    break
                }
            } catch {
                # Try next base
            }
        }
        
        if (-not $BaseFound) {
            Write-Host "⚠️  Can't find remote base branch - validating all local commits" -ForegroundColor Yellow
            $Commits = @(git rev-list HEAD)
        }
    }
}

if ($Commits.Count -eq 0 -or ($Commits.Count -eq 1 -and [string]::IsNullOrWhiteSpace($Commits[0]))) {
    Write-Host "✅ No commits to push" -ForegroundColor Green
    exit 0
}

$FailedCount = 0
$FailedList = @()

Write-Host "🔍 Validating commit messages against Conventional Commits..." -ForegroundColor Cyan
Write-Host ""

foreach ($Commit in $Commits) {
    if ([string]::IsNullOrWhiteSpace($Commit)) {
        continue
    }
    
    $Message = git log -1 --format=%B $Commit | Select-Object -First 1
    $Short = git log -1 --format=%h $Commit
    
    if ($Message -notmatch $Pattern) {
        Write-Host "❌ $Short`: $Message" -ForegroundColor Red
        $FailedCount++
        $FailedList += @{
            Short = $Short
            Message = $Message
        }
    } else {
        Write-Host "✅ $Short`: $Message" -ForegroundColor Green
    }
}

Write-Host ""
if ($FailedCount -gt 0) {
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host "❌ Validation failed: $FailedCount invalid commit(s)" -ForegroundColor Red
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Valid types: $($ValidTypes -join ', ')" -ForegroundColor Yellow
    Write-Host "Format:      type(scope): subject" -ForegroundColor Yellow
    Write-Host "Example:     feat(api): add batch processing" -ForegroundColor Yellow
    Write-Host ""
    exit 1
} else {
    Write-Host "✅ All commits follow Conventional Commits format" -ForegroundColor Green
    exit 0
}
