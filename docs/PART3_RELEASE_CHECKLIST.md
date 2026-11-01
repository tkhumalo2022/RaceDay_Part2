# PROG6212 Part 3 — RaceDay quality and handover
**Student:** Thabiso Khumalo | **Status:** Working draft for learner review, not final ARC submission.

## Current verified development evidence — 1 November 2026
- [GitHub Actions run #41327840762](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41327840762) **passed all four jobs** on commit `018faf5`: Windows solution build/tests, Azure Blob SDK against Azurite, Docker image/route smoke, and four-service SQL Server + Azurite + API + MVC startup/HTTP readiness. This does **not** prove an authenticated container user journey or live Azure cloud deployment.
- After that CI run, local development added: MVC result-form time/position validation and four unit tests; accessible contrast/focus/mobile navigation adjustments; independently testable race discovery filters with four new tests; and search-result count/clear-filter/selected-state feedback.
- Latest local Debug build: **0 warnings, 0 errors**. Latest local test run: **55 passed, 0 failed, 0 skipped**, up from 47 before these changes. The existing isolated 31-assertion live QA transcript remains [Part3-E2E-Search-20261101.txt](evidence/Part3-E2E-Search-20261101.txt); it has **not** been rerun after the new UI edits.
- The newer branch commits need a **fresh green CI run**. Physical-phone, keyboard/screen-reader, live Azure, student video and ARC sign-offs remain separate and unverified.


## Implemented
- Separate ASP.NET Core MVC frontend at `src/RaCeDay.Portal`, with `RaceDayApiClient`. It has no direct EF Core/database access.
- Participant and Organiser registration/login/logout, API session validation, role-aware navigation.
- Upcoming-event discovery with Run/Walk/Cycle filter, event details and available categories.
- Organiser dashboard: event management, category CRUD, enrolment list, result publishing, event enrolment and finisher summary.
- Participant dashboard: enrolment, race status, result history, finish time and position out of actual event finishers; profile edit.
- Azure Blob SDK storage **only in the Part 2 API** for event banners and participant profile pictures. Container is private; API authorises upload and retrieval.
- Bounded PNG/JPEG validation using file signatures and 3 MB limit; no client-controlled blob paths. Azure configuration is never checked into Git.
- MVC Dockerfile and GitHub Actions workflow building/testing both applications, plus a Docker smoke-test job.
- Keyboard focus indication, skip link, form validation, responsive layout and reduced-motion support.

## Verified locally on 31 October 2026
- Release solution build: zero warnings, zero errors.
- Final local automated suite: **46 passed, 0 failed, 0 skipped** including a real Azure SDK round trip against Azurite configured with `--skipApiVersionCheck`.
- Integrated browser-level test script: organiser and participant registration, account sessions, event creation/update, categories, enrolment, profile edit, organiser results, participant result history, and role restriction.
- Local Azurite storage emulator accepts event banner and profile image uploads **via the MVC -> API -> Blob chain**; both images can be fetched through authenticated MVC proxy endpoints.
- Isolated SQL Server QA database `RaceDayPart3QA_20261031_v2`. Original Part 2 demo DB is not changed.
- Desktop and narrow-screen Chrome screenshots were captured from the running QA portal and saved to `docs/evidence/Part3-Desktop-RaceDay-20261031.png` and `docs/evidence/Part3-Mobile-RaceDay-20261031.png`; visual review identified and corrected a narrow-screen headline and navigation overflow.
- GitHub Actions Linux Docker image build and login/register smoke checks passed on run #41239761962. Windows CI integration tests were still running when this note was written.
- No production Azure account, storage credential, cloud URL or external deployment was used in these checks.

## Additional verified checks — 1 November 2026
- Updated Debug solution build: **0 errors, 0 warnings**.
- Updated local automated tests: **47 passed, 0 failed, 0 skipped** (an added corrupted/oversized-blob test is exercised in the dedicated Azurite CI job).
- GitHub Actions run [41323346941](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41323346941) passed **Windows build/tests, Docker route smoke and Azure SDK/Azurite integration**.
- Created a fresh isolated database `RaceDayPart3QA_20261101` and launched separate QA API/portal processes at loopback ports 5003/5350 with Azurite.
- An end-to-end PowerShell QA run passed **28 real workflow assertions**, including the organiser-banner form's event ID, reverse-role HTTP 403, and PNG upload/retrieval. Evidence is saved in [Part3-E2E-20261101.txt](evidence/Part3-E2E-20261101.txt).
- Fixed incorrect Azure-cloud language in local upload confirmation. Emulator checks are described as storage tests, not as proof of a real Azure subscription.
- Real Azure cloud credentials and public hosting have **not** been configured or claimed.
- Added an opt-in [four-service Docker Compose demo](PART3_DOCKER_COMPOSE.md), isolated local database migrations and [API Dockerfile](../src/RaCeDay/Dockerfile). A local clean build and 47 tests passed; the demo database init command succeeded in Development and was explicitly rejected in Production. A complete container-stack run has not been executed on this laptop because Docker Desktop is unavailable.
- Added race-name/location search with preserved Run/Walk/Cycle filters and responsive controls. A new isolated full-workflow QA run passed **31 assertions**; [Part3-E2E-Search-20261101.txt](evidence/Part3-E2E-Search-20261101.txt) is the genuine console transcript.


## Remaining real-world sign-off
- Configure an authorised **real** Azure Storage account securely (env var `AzureBlob__ConnectionString`) and verify cloud round-trip; local Azurite is not proof of cloud account setup.
- Review UI on actual phone, keyboard and screen reader; optional improvements after feedback.
- Confirm **all four** GitHub Actions jobs green for the latest branch commit. The preceding four-service integration passed on `018faf5`; Docker is not installed locally.
- Verify production HTTPS, domain/routing, session cookie scope and secure deployment for two apps before making a public deployment; current cookie relay is for same-host local development only.
- Create original video demo with student explanation and unlisted YouTube URL; do not invent video evidence or a screenshot.
- Verify **at least 20 genuinely meaningful Part 3 commits** before ARC submission and explain their changes; commit count alone does not establish marking acceptance.
- Reconcile remaining Part 1/2 schema mismatch with the lecturer if required.
- Submit the correct GitHub repository URL to ARC personally. No ARC submission was performed.

## Local development
Run in project root:
```powershell
dotnet restore RaCeDay.slnx
dotnet build RaCeDay.slnx -c Release --warnaserror
dotnet tool restore
```
Before running the API, configure a **separate development SQL Server database**, e.g.:
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=RaceDayPart3QA_20261031_v2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project src/RaCeDay --startup-project src/RaCeDay
dotnet run --project src/RaCeDay --no-launch-profile -- --seed-roles
dotnet run --project src/RaCeDay --no-launch-profile -- --urls http://localhost:5001
```
Open a second PowerShell terminal:
```powershell
dotnet run --project src/RaCeDay.Portal --no-launch-profile -- --urls http://localhost:5250
```
Open **http://localhost:5250**. Both apps must use the same hostname for local Identity cookie forwarding.

For private Blob storage, set environment variables on the API process:
```powershell
$env:AzureBlob__ConnectionString = "<obtain from your authorised Azure account, do not commit>"
$env:AzureBlob__Container = "raceday-images"
```
Azure container is created private; do not grant anonymous access.
For local emulator only, use `UseDevelopmentStorage=true` and start Azurite.

## Docker
```powershell
docker build -f src/RaCeDay.Portal/Dockerfile -t raceday-portal .
docker run --rm -p 8080:8080 -e RaCeDayApi__BaseUrl=http://host.docker.internal:5001/ raceday-portal
```
The API backend must be reachable from the container, and API/portal domains need an appropriate secure cookie deployment strategy. The GitHub Actions Docker smoke job tests the image independently of a live backend.

## QA script
The QA-only PowerShell script is `scripts/qa-portal-e2e.ps1`. It creates test users and test race records, so run **only** against the isolated QA SQL database with:
`$env:RACEDAY_QA_APPROVED="YES"`. It relies on Azurite, plus the already-running QA API and portal bound to 127.0.0.1:5001 and :5250.

## Academic integrity
This code was substantially assisted by AI. Review and explain its design, tests, security approach, API contracts, code snippets and deployment plan in your own words. Disclose your AI assistance according to the IIE PROG6212 brief. Only present actual tests, builds, screenshots and demo links.
