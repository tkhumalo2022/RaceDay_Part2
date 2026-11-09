# RaceDay Part 3 — API and MVC integration check (9 November 2026)

## What I checked

This check ran the **actual RaceDay API and separate MVC portal** against a new, isolated SQL Server LocalDB database. The test script used local HTTP URLs only:

- API: `http://127.0.0.1:5111`
- MVC portal: `http://127.0.0.1:5361`
- Test database: `RaceDayPart3QA_Smoke_784e5ef6` (not the Part 1 or normal development database)
- Source branch: `feature/part3-mvc-review`, commit `b2877bb`

I first applied the existing EF Core migrations to this **new** database and seeded only the role names. The test registered random organiser and participant QA accounts.

For this run I used a temporary copy of `scripts/qa-portal-e2e.ps1` that stopped **before the Azurite image-upload section**. The checked-in script was not modified.

## Results: 28 core workflow checks passed

- Organiser registration, authenticated dashboard and event form
- Event creation and returned event ID
- Category creation through the API
- Participant registration and authentication
- Name/location searches and 7-day/30-day date filters
- Display of an eligible category and enrolment button
- Participant enrolment and profile editing through the API
- Organiser enrolment list and event editing
- Recording a finished race result
- Participant dashboard showing finishing time and position
- HTTP 403 when a participant tries to open an organiser-only screen

All 28 `PASS:` assertions from the core E2E script succeeded. The API and portal processes were stopped after the test.

## Other evidence on the same source version

- Local Debug build: zero warnings and zero errors (9 November)
- Local automated suite: 59 passed, zero failed (9 November)
- GitHub Actions: [four-job PR run #42601758972](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42601758972) passed at commit `b2877bb`, including SQL integration tests, local Azurite SDK tests, Docker image smoke and four-service Compose startup/readiness

## What this does NOT prove

- **Real Azure cloud storage:** this local run skipped banner/profile-image upload because Azurite is not installed on this laptop. GitHub's separate Blob test uses the Azurite emulator, not an Azure subscription.
- **Full container user journey:** Docker is not installed locally. CI checks image build, HTTP routes and service readiness, not an authenticated end-to-end action through a deployed container.
- **Public hosting or cross-domain login:** the API and MVC portal relay cookies in same-host local development. Public HTTPS cookie/domain design needs additional work and verification before deployment.
- **Actual phone, keyboard and screen-reader testing:** not part of this run.
- **Video or ARC submission:** neither is confirmed by these tests.

The first local health-check attempt used the wrong route for the API (`/Account/Register`) and stopped before workflow QA. I corrected the **temporary test setup** to check the API's Swagger endpoint; the clean second run completed successfully. This was not a backend code defect.

This record was prepared with AI assistance based on an actual local execution. The student still needs to review the work, understand the API flow, record an authentic demonstration and submit personally in line with the lecturer's rules.
