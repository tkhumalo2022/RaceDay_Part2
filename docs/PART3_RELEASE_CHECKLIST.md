# PROG6212 Part 3 — RaceDay quality and handover
**Student:** Thabiso Khumalo | **Status:** Working draft for learner review, not final ARC submission.

## Actual original-rubric verification — 9 November 2026

- Read and checked the actual **PART 3** criterion table in the open institutional `PROG6212_POE.docx`. It explicitly requires a Blob image URL saved in the database; see [updated actual-rubric audit](PART3_RUBRIC_AUDIT_20261109.md).
- Added `Part3PersistBlobImageUrls` EF Core migration with separate `Event.BannerBlobUrl` and `AspNetUsers.ProfileBlobUrl` columns. Private images are uploaded **MVC → API → Azure SDK** and served through the API; the database URLs are stripped of SAS query parameters.
- On a new isolated database `RaceDayPart3RubricQA_20261109`: migrations succeeded, Release build **0 errors/0 warnings**, automated tests **59/59**, real organiser/participant workflow **40/40**, and direct SQL confirms **both stored URL columns populated**. [40-pass transcript](evidence/Part3-Rubric-BlobUrl-Persistence-QA-20261109.txt) / [sanitised SQL proof](evidence/Part3-Sql-Blob-Url-Check-20261109.txt).
- New authenticated **40-step** Docker CI job **passed in run #42646700257 on code commit `2448cfa`**, all 4 jobs green, with 40 PASS lines downloaded from the CI artifact. Check again after future commits. Student video, actual Azure cloud and ARC submission are not yet done.

## Earlier verification — 9 November 2026

- Current Part 3 code at `242374a`: Release solution build succeeded with **0 warnings/0 errors**; **59/59 local tests** passed. Local full organiser/participant/SQL/Azurite browser-session QA passed **38/38 assertions**, including direct Create/Edit banner input and multipart event-edit+banner save. [Raw local transcript](evidence/Part3-Rubric-Workflow-20261109.txt).
- [GitHub Actions PR run #42640996423](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42640996423) passed **all four jobs** at `242374a`. The `compose-integration` job executed the **authenticated 38-step workflow inside the complete four-service Docker stack**. Its downloaded artifact contains **38 PASS lines** and a QA COMPLETE record.
- See [rubric-by-rubric marking evidence and remaining sign-offs](PART3_RUBRIC_AUDIT_20261109.md). Original Part 3 video, live Azure cloud proof, real-device sign-off, full original marking rubric extraction, and ARC submission are **not** established by these checks.

## Earlier verification — 7 November 2026
- See [PART3_VERIFICATION_20261107.md](PART3_VERIFICATION_20261107.md): freshly rerun **58/58 local tests**, Debug **build with 0 warnings/errors**, **16/16 preflight paths**, and exact-source CI green. Real cloud, actual phone, original Part 3 video, lecturer rubric and ARC submission remain unverified.

## Previous verification — 5 November 2026
- See [PART3_VERIFICATION_20261105.md](PART3_VERIFICATION_20261105.md) for the new evidence-backed checkpoint: local **58/58 tests passed**, preflight's **16/16 required paths present**, and [GitHub Actions run #41501949381](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41501949381) green on commit `f025338` (**four of four jobs**). The branch is **22 commits ahead of main**; meaningfulness is for human review.
- This confirms test/CI health, **not** final submission, local Docker, real Azure Storage, actual-phone accessibility, production authentication, original student demo, or ARC submission.

## Earlier verified development evidence — 2 November 2026
- [GitHub Actions PR run #41331598518](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41331598518) **passed all four jobs** at branch commit `62177a8`: Windows build/tests, Azure SDK/Azurite, Docker smoke and four-service startup. The companion push run #41331593808 passed its three applicable jobs, with Compose skipped by design. Neither run covers later local edits; authenticated container journeys and real Azure remain unverified.
- The branch contains MVC result-form validation, improved contrast/focus states, and race discovery with name/location, Run/Walk/Cycle activity, and **Any date / Next 7 days / Next 30 days** date-window filters that preserve current selections. On 2 November, the QA script was updated to use adjustable **loopback-only** API/MVC endpoints and relative event dates, making isolated retesting reproducible without editing the normal demo database.
- On 2 November the updated local Debug build had **0 warnings, 0 errors** and **58 tests passed, 0 failed, 0 skipped**, including 3 new date-window tests. A fresh end-to-end run against isolated LocalDB `RaceDayPart3QA_20261102_DateFiltersV5`, API port 5101, MVC port 5351 and Azurite passed **34 of 34 workflow assertions**. The actual transcript is in [Part3-E2E-Date-Filters-20261102.txt](evidence/Part3-E2E-Date-Filters-20261102.txt). The earlier 31-assertion run is preserved in [Part3-E2E-20261102.txt](evidence/Part3-E2E-20261102.txt). These are local emulator tests, not live Azure proof.
- After pushing the latest date-filter implementation and QA evidence, confirm a new green CI run for the resulting commit. Physical-phone, keyboard/screen-reader, live Azure, authenticated container-user journeys, student video and ARC sign-offs remain separate and unverified.


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
- **CI verified 5 November:** all four GitHub Actions jobs passed at `f025338` in [run #41501949381](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41501949381). Recheck against HEAD after any new commit. Docker is not installed locally, and authenticated deployed workflows remain unverified.
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
