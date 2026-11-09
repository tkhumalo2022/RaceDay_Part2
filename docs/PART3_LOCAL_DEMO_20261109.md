# RaceDay Part 3 — verified student laptop demo (9 November 2026)

**Verified:** Thabiso's Windows laptop with .NET 10, local SQL Server, separate MVC and API, and Azurite 3.37.0. The local demo database `RaceDayPart3StudentDemo_20261109` was newly created and migrated; no original Part 2 database was modified.

## Current local URLs

**New rubric URL-persistence QA (separate from the original local demo):** `http://localhost:5351` for MVC and `http://localhost:5101/swagger` for API, backed by isolated SQL database `RaceDayPart3RubricQA_20261109`. The genuine end-to-end test passed **40/40** and the Azure SDK stored actual private Blob URLs in the two new database columns. See the [actual rubric audit](PART3_RUBRIC_AUDIT_20261109.md) and [SQL evidence](evidence/Part3-Sql-Blob-Url-Check-20261109.txt). These temporary test processes may stop after this review; restart from the documentation if needed. The following URLs refer to the older separate demo environment.

- **MVC (show this in video):** http://localhost:5250
- **Part 2 REST API Swagger:** http://localhost:5001/swagger
- **Local Azurite Blob:** http://127.0.0.1:10000 (not a user-facing browser site)

The three processes were started on 9 November. Re-running the following commands is for **after restarting the computer or closing the local services**. Run the commands from the RaceDay project root unless shown otherwise.

### Window 1 — local Azure-compatible emulator

```powershell
New-Item -ItemType Directory -Force "$env:USERPROFILE\Documents\RaceDayPart3_DemoBlob_20261109" | Out-Null
npx --yes azurite --location "$env:USERPROFILE\Documents\RaceDayPart3_DemoBlob_20261109" --blobHost 127.0.0.1 --blobPort 10000 --queueHost 127.0.0.1 --queuePort 10001 --tableHost 127.0.0.1 --tablePort 10002 --skipApiVersionCheck --silent
```

**Important:** `--skipApiVersionCheck` is required with this Azure SDK and Azurite build. Without it, upload returns HTTP 503 from the API because Azurite rejects SDK API version `2026-10-06` (the emulator itself returns HTTP 400). The default emulator key is a public test key; do not use it with real customer or student data.

### Window 2 — Part 2 API

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
$env:ConnectionStrings__DefaultConnection='Server=localhost;Database=RaceDayPart3StudentDemo_20261109;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
$env:AzureBlob__ConnectionString='UseDevelopmentStorage=true'
$env:AzureBlob__Container='raceday-images'
dotnet run --project src/RaCeDay --no-launch-profile -- --urls http://localhost:5001
```

### Window 3 — Part 3 MVC

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
dotnet run --project src/RaCeDay.Portal --no-launch-profile -- --urls http://localhost:5250
```

Keep all three windows running while filming. Do not share `localhost` as a public cloud link.

## Actual QA results — same day

- .NET 10 Release solution build: **0 errors, 0 warnings**.
- Automated local solution test suite: **59/59 passed**.
- Azure Blob SDK specific tests against local Azurite: **2/2 passed** after emulator restart.
- Updated 9 November rubric workflow: **38/38 PASS** through MVC → API → SQL Server → private Azurite, including banner field checks and an event edit with bundled banner upload. [Actual 38-check transcript](evidence/Part3-Rubric-Workflow-20261109.txt).
- Earlier real HTTP/browser-session QA script: **34/34 PASS** for organiser and participant registration, API session, event/category management, filtered discovery, enrolment, profile edit, results, role restrictions, **event banner upload/retrieval**, and **profile picture upload/retrieval**. See [actual 34-pass transcript](evidence/Part3-Full-Local-QA-20261109.txt).
- Latest cloud CI source check (at `242374a`): [all four GitHub Actions jobs successful, including 38/38 authenticated container checks](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42640996423).

For a further QA run against **only this isolated database**, with the above processes running:

```powershell
$env:RACEDAY_QA_APPROVED='YES'
powershell -ExecutionPolicy Bypass -File scripts/qa-portal-e2e.ps1 -PortalBaseUrl http://localhost:5250 -ApiBaseUrl http://localhost:5001
```

The script adds random QA accounts and events. This is valid local emulator evidence; it is **not** a live Azure-cloud upload, a public production site, a recorded video, or an ARC submission.
