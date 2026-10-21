# Part 2 pre-submission check for RaCeDay.
# No SQL writes. Without -RunTests, this script only reads project files,
# Git, SQL Server metadata and local HTTP responses.
# With -RunTests, the test suite creates fresh, separate LocalDB test databases.
param([switch]$RunTests)

$ErrorActionPreference = 'Continue'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$demoDatabase = 'RaCeDayPart2_SQLServer_Demo_20261018'
$workingBranch = 'recovery/part2-20261017'

function Result([string]$label, [bool]$ok, [string]$detail) {
    $status = if ($ok) { 'PASS' } else { 'NEEDS WORK' }
    Write-Output ('[{0}] {1} - {2}' -f $status, $label, $detail)
}
Write-Output 'RaCeDay PROG6212 Part 2 - Pre-submission check'
Write-Output ('Checked at: ' + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'))
Write-Output ('Source: ' + $root)
Write-Output ''

$branch = (& git -C $root branch --show-current 2>$null) -join ''
Result 'Correct Part 2 branch' ($branch -eq $workingBranch) $branch
$working = @(& git -C $root status --porcelain 2>$null)
Result 'Git working tree' ($working.Count -eq 0) ("Uncommitted changes: " + $working.Count)
$commit = ((& git -C $root log -1 --format='%h %s' 2>$null) -join '')
Write-Output ('Latest commit: ' + $commit)
$originMain = (& git -C $root rev-parse --verify origin/main 2>$null) -join ''
if ($LASTEXITCODE -eq 0) {
    $ahead = ((& git -C $root rev-list --count 'origin/main..HEAD') -join '').Trim()
    Result 'Commit count beyond main' ([int]$ahead -ge 20) ("$ahead commits; lecturer still decides which are meaningful")
} else {
    Result 'Commit count beyond main' $false 'origin/main is unavailable locally. Check GitHub history.'
}
Result 'GitHub Actions workflow' (Test-Path (Join-Path $root '.github\workflows\dotnet-build.yml')) 'Workflow file exists'
Result 'Genuine previous CI screenshot' (Test-Path (Join-Path $root 'docs\evidence\GitHub-CI-Green-20261018.png')) 'Stored under docs/evidence'
Write-Output 'Latest independently verified green feature CI: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39147081224'

$cfg = Get-Content (Join-Path $root 'src\RaCeDay\appsettings.json') -Raw
Result 'SQL Server demo configured' ($cfg.Contains($demoDatabase)) $demoDatabase
$sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
if ($sqlcmd) {
    $query = 'SET NOCOUNT ON; SELECT COUNT(*) FROM dbo.__EFMigrationsHistory; SELECT COUNT(*) FROM dbo.AspNetRoles; SELECT COUNT(*) FROM dbo.AspNetUsers;'
    $rows = @(& sqlcmd -S localhost -E -C -d $demoDatabase -h -1 -W -Q $query 2>&1)
    $counts = @($rows | ForEach-Object { $_.ToString().Trim() } | Where-Object { $_ -match '^\d+$' })
    if ($LASTEXITCODE -eq 0 -and $counts.Count -ge 3) {
        Result 'EF Core migrations' ($counts[0] -eq '6') ($counts[0] + ' installed')
        Result 'Identity roles' ($counts[1] -eq '3') ($counts[1] + ' role names in SQL Server')
        Result 'Registered demo accounts' ([int]$counts[2] -ge 2) ($counts[2] + ' accounts. Prepare Organiser and Participant, plus optional Admin.')
    } else {
        Result 'SQL Server demo database' $false 'Unable to read migration, role and user counts'
    }
} else { Result 'SQL Server tools' $false 'sqlcmd is not installed or not in PATH' }

try {
    $homeResponse = Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5236/' -TimeoutSec 4
    Result 'Running RaCeDay site' ($homeResponse.StatusCode -eq 200) ('HTTP ' + $homeResponse.StatusCode)
} catch { Result 'Running RaCeDay site' $false ($_.Exception.Message + '. Start RaCeDay on port 5236 before recording.') }
try {
    $swagger = Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5236/swagger/v1/swagger.json' -TimeoutSec 4
    $document = $swagger.Content | ConvertFrom-Json
    $paths = @($document.paths.PSObject.Properties).Count
    Result 'Swagger description' ($swagger.StatusCode -eq 200 -and $paths -ge 15) ("HTTP $($swagger.StatusCode), $paths paths")
} catch { Result 'Swagger description' $false 'Start in Development to use /swagger' }

$testsFolder = Join-Path $root 'tests\RaCeDay.Tests\TestResults'
$trx = Get-ChildItem $testsFolder -Filter '*.trx' -File -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($null -ne $trx) {
    try {
        [xml]$xml = Get-Content $trx.FullName -Raw
        $counters = $xml.TestRun.ResultSummary.Counters
        $passed = [int]$counters.passed
        $failed = [int]$counters.failed
        Result 'Latest local test report' ($passed -ge 35 -and $failed -eq 0) ("$passed passed, $failed failed - $($trx.Name)")
    } catch {
        Result 'Latest local test report' $false 'Could not read latest .trx file'
    }
} else { Result 'Latest local test report' $false 'Run dotnet test RaCeDay.slnx -c Release' }

$readme = Get-Content (Join-Path $root 'README.md') -Raw
$videoLink = $readme -match '(https?://(?:www\.)?(?:youtu\.be/|youtube\.com/watch\?))'
Result 'Unlisted video link in README' $videoLink $(if ($videoLink) { 'Present; verify link and privacy manually' } else { 'Not added yet' })
Result 'Part 1 schema parity' $false 'NOT VERIFIED: Original integer Organiser/Participant tables differ from Identity schema. Get lecturer approval or migration plan.'
Result 'ARC submission confirmed' $false 'Cannot be verified locally. Submit and confirm in ARC yourself.'

if ($RunTests) {
    Write-Output ''
    Write-Output 'Running build and isolated LocalDB tests...'
    Push-Location $root
    try {
        & dotnet build RaCeDay.slnx -c Release --warnaserror --nologo
        $buildCode = $LASTEXITCODE
        Result 'Release build' ($buildCode -eq 0) ("Exit code $buildCode")
        if ($buildCode -eq 0) {
            & dotnet test RaCeDay.slnx -c Release --no-build --nologo
            Result 'Test run' ($LASTEXITCODE -eq 0) ("Exit code $LASTEXITCODE")
        }
    } finally { Pop-Location }
}
Write-Output ''
Write-Output 'Open docs\PART2_FINAL_REVIEW.md and fix each unticked rubric requirement.'
