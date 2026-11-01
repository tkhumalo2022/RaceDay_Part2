param(
    [switch]$RunBuild,
    [switch]$RunTests
)
# Read-only assessment preflight except for optional local build/test output.
# Never prints configuration secrets, edits academic evidence or submits to ARC.
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Push-Location $root
try {
    Write-Host '=== RaceDay Part 3 Submission Preflight ==='
    $expected = @(
        'src/RaCeDay.Portal/RaCeDay.Portal.csproj',
        'src/RaCeDay.Portal/Dockerfile',
        'src/RaCeDay/Dockerfile',
        'compose.yaml',
        'docs/PART3_DOCKER_COMPOSE.md',
        'scripts/ci-compose-smoke.sh',
        'src/RaCeDay.Portal/Services/RaceDayApiClient.cs',
        'src/RaCeDay/Services/AzureRaceImageStore.cs',
        '.github/workflows/dotnet-build.yml',
        'docs/PART3_RELEASE_CHECKLIST.md',
        'docs/PART3_DEMO_AND_HANDOVER.md',
        'tests/RaCeDay.Tests/PortalResultFormTests.cs',
        'tests/RaCeDay.Tests/PortalEventDiscoveryTests.cs',
        'docs/evidence/Part3-E2E-Search-20261101.txt'
    )
    foreach ($file in $expected) {
        $ok = Test-Path $file
        Write-Host ('{0} required file: {1}' -f $(if ($ok) {'PASS'} else {'FAIL'}), $file)
    }
    $current = (git branch --show-current).Trim()
    $count = [int]((git rev-list --count main..HEAD).Trim())
    Write-Host "Current branch: $current"
    Write-Host "Part 3 branch commits ahead of main: $count (rubric minimum: 20 meaningful)"
    if ($count -lt 20) { Write-Warning 'Commit count is below 20. Only actual meaningful development commits qualify.' }
    if ((git status --porcelain)) { Write-Warning 'Working tree has uncommitted files; review changes before submission.' }
    if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
        Write-Warning 'Docker is unavailable locally. Four-service startup passed CI at 018faf5, but the latest commit and authenticated container workflows still require independent verification.'
    }
    if (-not [Environment]::GetEnvironmentVariable('AzureBlob__ConnectionString')) {
        Write-Warning 'AzureBlob__ConnectionString is not configured for THIS process. Never commit it to Git.'
    }
    Write-Host 'Manual: verify latest GitHub Actions, actual phone/keyboard check, original YouTube link, real Azure cloud upload, and ARC submission.'
    if ($RunBuild) {
        Write-Host 'Running independent Debug build to avoid locking active Release QA processes...'
        dotnet build RaCeDay.slnx -c Debug --warnaserror
        if ($LASTEXITCODE -ne 0) { throw 'Debug build did not pass.' }
    }
    if ($RunTests) {
        Write-Host 'Running local unit and SQL Server tests...'
        dotnet test RaCeDay.slnx -c Debug --no-restore --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw 'Tests did not pass.' }
    }
    Write-Host 'Preflight finished; manual evidence remains authoritative.'
}
finally { Pop-Location }
