# RaceDay — PROG6212 Part 3 (MVC Portal + Part 2 API)

**Part 3 review branch:** [feature/part3-mvc-review](https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review)
**Status:** Assessment review draft. The Part 3 video and ARC submission are **not** complete.
**Latest verification (9 November 2026):** 59/59 local tests passed; Debug build with warnings-as-errors succeeded with 0 warnings and 0 errors. A fresh isolated MVC → API → SQL Server check passed 28/28 core workflow assertions. GitHub Actions [run #42601758972](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42601758972) passed all four CI jobs on `b2877bb`. [9 November integration evidence](docs/evidence/Part3-Core-Integration-20261109.md) · [Earlier submission readiness checks](docs/PART3_VERIFICATION_20261107.md). The original Part 3 video, real Azure cloud and ARC submission remain unverified.

## Part 3: assessor quick navigation

- **Separate MVC app:** [src/RaCeDay.Portal](src/RaCeDay.Portal), calling the Part 2 API through `RaceDayApiClient` (no direct EF/database access).
- **Existing API/backend:** [src/RaCeDay](src/RaCeDay), including role-protected operations and private Azure Blob Storage integration.
- **Docker:** [MVC Dockerfile](src/RaCeDay.Portal/Dockerfile), [API Dockerfile](src/RaCeDay/Dockerfile), [local four-service Compose](compose.yaml) and [single-command launch guide](docs/PART3_DOCKER_COMPOSE.md). GitHub Actions PR run [#41331598518](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41331598518) passed all four jobs, including SQL Server/Azurite/API/MVC startup, on commit `62177a8`. Later commits require new CI verification. This is CI evidence, not a local Docker installation or a completed authenticated container-user workflow.
- **Tests:** [tests/RaCeDay.Tests](tests/RaCeDay.Tests), [actual QA E2E evidence](docs/evidence/Part3-E2E-20261101.txt), and [31-assertion search E2E transcript](docs/evidence/Part3-E2E-Search-20261101.txt) and [fresh 2 November isolated 34-assertion date-filter QA](docs/evidence/Part3-E2E-Date-Filters-20261102.txt). The [Part 3 review PR](https://github.com/tkhumalo2022/RaceDay_Part2/pull/3) remains draft.
- **Setup, remaining sign-offs and student demo checklist:** [Part 3 release checklist](docs/PART3_RELEASE_CHECKLIST.md) and [demonstration handover](docs/PART3_DEMO_AND_HANDOVER.md).
- **Progress verification:** Run `powershell -ExecutionPolicy Bypass -File scripts/part3-preflight.ps1` from the repository root.

**Important:** Azure SDK tests run against the Azurite emulator. A live Azure account is not yet verified. This code was developed with AI assistance and must be reviewed and attributed according to the lecturer's rules. The unlisted Part 3 YouTube presentation must be the student's authentic recording.

---

## Original Part 2 documentation (kept for traceability)

# RaCeDay â€” PROG6212 Part 2

**Student:** Thabiso Khumalo (ST10477675)
**Module:** PROG6212 Programming 2B
**Assessment:** Portfolio of Evidence, Part 2 â€” RESTful API Development

RaCeDay is an ASP.NET Core race management application for organisers and participants. Organisers manage events, categories, entries and results. Participants register, enter races and view their results.

## Submission links

- **Repository:** https://github.com/tkhumalo2022/RaceDay_Part2
- **Source code:** [ASP.NET Core project](src/RaCeDay) Â· [automated tests](tests/RaCeDay.Tests) Â· [commit history](https://github.com/tkhumalo2022/RaceDay_Part2/commits/main)
- **Presentation:** [RaCeDay Part 2 video](https://youtu.be/jytTUheNxYw)
- **Build and tests:** [Successful GitHub Actions run â€” 19 October 2026](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39265809171) (39 passed, 0 failed, 0 skipped)
- **Assessment evidence:** [Part 2 evidence index](docs/PART2_EVIDENCE_INDEX.md)

## Features

- Organiser and Participant registration, login and logout using ASP.NET Core Identity. Passwords are hashed and users cannot register themselves as Admin.
- Authorisation for protected pages and API endpoints, with a server-side user/role session and Identity authentication cookies.
- Event and race-category management for organisers, with input validation and ownership checks.
- Participant enrolments with closing-date, capacity and duplicate-entry restrictions.
- Organiser result recording and participant access to their own results.
- Profile editing, including optional JPG/PNG profile pictures (up to 2 MB), and an Admin summary dashboard.
- REST endpoints documented in Swagger/OpenAPI.
- SQL Server storage through Entity Framework Core and code-first migrations, including database checks for participant counts and payment-attempt amounts.
- Automated tests for authentication, roles, race workflows, SQL Server behaviour and validation.

**Payment scope:** Payment attempts are recorded as pending. This project does not process real payments or confirm settlements.

## Technology

C#, ASP.NET Core MVC/Web API (.NET 10), ASP.NET Core Identity, Entity Framework Core, Microsoft SQL Server 2022, Swagger/OpenAPI, xUnit and GitHub Actions.

## Build and test

Use **Windows**, **.NET SDK 10** and **SQL Server LocalDB** for the automated tests. Open `RaCeDay.slnx` in Visual Studio or run these commands from the repository root:

```powershell
dotnet restore RaCeDay.slnx
dotnet build RaCeDay.slnx -c Release --no-restore --warnaserror
dotnet test RaCeDay.slnx -c Release --no-restore --no-build
```

Tests use separate `RaCeDayTests_<GUID>` databases. They do not need the original Part 1 database.

### Run the application locally

The application needs a SQL Server connection. The checked-in configuration refers to a separate SQL Server demonstration database on the development machine. On another Windows computer, use a new local database:

```powershell
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=RaCeDayPart2_Marking;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
dotnet tool run dotnet-ef database update --configuration Release --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
dotnet run --project src/RaCeDay/RaCeDay.csproj -- --seed-roles
dotnet run --project src/RaCeDay/RaCeDay.csproj
```

The migrations create the Part 2 schema in the selected database; `--seed-roles` adds the role names Admin, Organiser and Participant. Apply migrations only to a new or backed-up database. **Do not run the Part 1 SQL reset script against an existing database.**

Register as Organiser or Participant through the website. The app runs with Swagger enabled in Development at `/swagger`.

### REST API and Swagger

Examples of available endpoints:

| Task | Endpoint |
| --- | --- |
| Register | `POST /api/auth/register` |
| Login | `POST /api/auth/login` |
| Logout | `POST /api/auth/logout` |
| Current session | `GET /api/auth/session` |
| Current profile | `GET /api/users/me` |
| Update profile | `PUT /api/users/me` |

Other endpoints cover races, categories, enrolments, results and payment attempts. Their descriptions and request/response types appear in Swagger.

Authentication uses **Identity cookies**, not JWT. For Swagger requests that change data, first call `GET /api/auth/antiforgery`, copy the returned `requestToken` into Swagger **Authorize** as `X-CSRF-TOKEN`, and refresh the token after login. Sign-in and API calls must use the same browser session.

## Marking evidence

- **Source:** `src/RaCeDay/Controllers`, `src/RaCeDay/Services/RaceWorkflow.cs` and `src/RaCeDay/Data/ApplicationDbContext.cs`
- **Migrations:** `src/RaCeDay/Data/Migrations`
- **Tests:** `tests/RaCeDay.Tests`
- **CI workflow:** [`.github/workflows/dotnet-build.yml`](.github/workflows/dotnet-build.yml)
- **CI run:** [39 tests passed, 0 failed](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39265809171)
- **Workflow screenshot:** [GitHub Actions evidence](docs/evidence/GitHub-CI-Green-20261018.png) (earlier successful run)
- **Rubric and detailed evidence:** [Evidence index](docs/PART2_EVIDENCE_INDEX.md) and [Part 2 final review](docs/PART2_FINAL_REVIEW.md)
- **Video:** https://youtu.be/jytTUheNxYw

## Database design note

Part 1 used separate `Organiser` and `Participant` tables with integer keys. Part 2 uses ASP.NET Core Identity's `AspNetUsers` and role tables with string keys. The physical schemas are therefore **not identical**, although the race-management functions are implemented. The difference is documented in [Part 1 / Part 2 database comparison](docs/PART1_PART2_DB_COMPARISON_20261019.md); lecturer acceptance of this change has not been confirmed.

This repository contains a separately developed Part 2 working implementation; it should not be represented as an exact recovery of the original local Visual Studio solution. GitHub content and passing CI do not by themselves confirm submission on ARC.

---

## PROG6212 Part 3: Separate MVC RaceDay Portal

Part 3 extends the Part 2 API with a separate ASP.NET Core MVC frontend under `src/RaCeDay.Portal`. The MVC site does **not** access SQL Server or use EF Core; event, participant, result, profile and image operations call the Part 2 API.

**Features:** organiser and participant registration/login/logout, role-aware navigation, event discovery with activity, name/location and 7/30-day filters, event and category management, enrolment, organiser results entry, participant history and profile, Azure Blob Storage event banners/profile pictures (through API only), Dockerfile, and CI checks.

**Quick start:** See [Part 3 release and QA checklist](docs/PART3_RELEASE_CHECKLIST.md) for SQL Server setup, both app launch commands, Azure Storage configuration, Docker build/run instructions, tests and disclosure notes. Start the API at `http://localhost:5001`, start the portal at `http://localhost:5250`, then open the portal in a browser. Both apps must use the **same hostname** in local development to relay session cookies.

**QA:** `dotnet build RaCeDay.slnx -c Release --warnaserror` and `dotnet test RaCeDay.slnx -c Release`. The opt-in PowerShell E2E test script uses an isolated QA database and Azurite; do not run it against your normal database. Latest local automated and live checks are documented in the checklist. GitHub Actions builds and tests on Windows and performs a Linux Docker smoke build.

**Visual evidence:** [Desktop home screenshot](docs/evidence/Part3-Desktop-RaceDay-20261031.png) Â· [Mobile home screenshot](docs/evidence/Part3-Mobile-RaceDay-20261031.png). These are genuine QA renders, not a live hosted demo.

**Part 3 demo and student handover:** [Full walkthrough and assessment checklist](docs/PART3_DEMO_AND_HANDOVER.md) · [Local submission preflight](scripts/part3-preflight.ps1). These are preparation materials, not proof of an unrecorded video.

**Submission status:** This branch is still a learner review draft, not an ARC submission. The 20-commit requirement is reached through feature, testing and evidence changes, but commit quality remains subject to lecturer review. The genuine student-recorded unlisted YouTube walkthrough, real Azure account validation and actual-phone/accessibility sign-off remain incomplete. Re-check fresh CI for the latest commit before submitting. Do not fabricate video links, screenshots, passing test claims or evidence. AI assistance was used and should be disclosed as required by the assessment brief.
