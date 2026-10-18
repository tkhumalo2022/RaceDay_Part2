# RaCeDay — PROG6212 POE Part 2 (recovery working copy)

**Student:** Thabiso Khumalo

**Status:** Active, isolated recovery branch `recovery/part2-20261017`. The original student Visual Studio application has not been recovered. The official Part 2 rubric was visually reviewed in the protected SharePoint document (pages 9–11 and 17–18); see `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This working copy was built from the student's Part 2 Identity foundation and reconciled against the accessible Part 1 API/database plan. It must not be represented as the original recovered app or as a rubric-complete submission.

## What works

- ASP.NET Core MVC / .NET 10 with SQL Server EF Core and ASP.NET Identity.
- Register and sign in as either Organiser or Participant (MVC or JSON API); role-specific details (date of birth for Participants, organisation name for Organisers), password hashing, Identity session cookies, CSRF protection and guarded dashboards. Public registration **rejects Admin**.
- Profile editing through both MVC and GET/PUT `/api/users/me`, restricted to the signed-in user with allowlisted fields and anti-forgery protection.
- Organiser event and category CRUD, including validation and ownership checks.
- Participant enrolment with deadline, capacity, category-event consistency and **one entry per category per participant**. Organiser-owned entry views now show participant names. SQL Server locking prevents simultaneous overbooking. A SQL trigger maintains stored `CurrentParticipants` using the Part 1 `Registered`-only rule.
- Organiser result recording; participants can view their own results.
- Cookie-authenticated REST APIs including `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/antiforgery` and `GET /api/auth/session`. A **server-side memory-backed session** also stores the signed-in user ID and role, as specified by the Part 2 marking rubric; ASP.NET Identity claims remain the source of authorization. Logout clears both. Swagger UI, MVC pages and the generic error page are available.
- Pending-only payment-attempt records with both API and **SQL-trigger** entry-fee amount validation, ownership control, unique transaction references and at most one `Paid` record per enrolment. **No real payment processing or confirmation exists.**
- Automated SQL Server LocalDB integration tests for roles, Identity, forms, routes, business rules, concurrency and payment records.

## GitHub and verified Part 2 CI

- **Student:** Thabiso Khumalo
- **Part 2 source branch:** [recovery/part2-20261017](https://github.com/tkhumalo2022/RaceDay_Part2/tree/recovery/part2-20261017). The default `main` branch is deliberately unchanged.
- **Verified GitHub Actions build and test:** [Successful run #39137366482 (18 October 2026)](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39137366482) on Windows, including the updated organiser-entry view and its test. The earlier [run #39068333340](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39068333340) separately recorded 32 passing tests.
- **Genuine screenshot:** [GitHub Actions success evidence](docs/evidence/GitHub-CI-Green-20261017.png). The GitHub run also retains the `raceday-test-results` artifact.
- This branch contained **23 real commits** at the green CI run. Their individual relevance must be judged by the assessor; no filler commits have been added.
- **Presentation video:** Not recorded/uploaded yet. Add Thabiso Khumalo's actual **unlisted YouTube video URL** here before submitting on ARC.

## Requirements

Windows with .NET SDK 10, **Microsoft SQL Server 2022** (default instance for the demonstration), SQL Server LocalDB (for the automated integration tests), and the local `dotnet-ef` CLI (10.x). All commands below run in this repository root. Open `RaCeDay.slnx` in Visual Studio or a shell.

```powershell
dotnet tool restore
dotnet restore
dotnet build RaCeDay.slnx -c Release --warnaserror
dotnet test RaCeDay.slnx -c Release
```

Tests create isolated `RaCeDayTests_<GUID>` databases with synthetic data and retain them for inspection. They must **never** run against the original RaceDay database.

### Microsoft SQL Server 2022 demonstration database (separate from Part 1)

**Verified on the student's Windows SQL Server instance on 18 October 2026:** `localhost` (the default `MSSQLSERVER` instance), Windows Authentication, `RaCeDayPart2_SQLServer_Demo_20261018`. All six forward EF Core migrations have been applied to that newly created database; `Admin`, `Organiser`, and `Participant` role names were seeded, and both SQL triggers were verified. The original `RaceDayDB` was inspected **read-only** and must not be overwritten.

The checked-in `appsettings.json` points the application to this separate full SQL Server database, rather than LocalDB. If running on another computer, change the server/database safely or override with `ConnectionStrings__DefaultConnection`. **Never point EF migrations at the original `RaceDayDB` without a separately reviewed migration and backup.**

```powershell
# From the repository root on Windows; use your own NEW database name if needed.
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=RaCeDayPart2_SQLServer_Demo_20261018;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True'
dotnet tool run dotnet-ef migrations list --configuration Release --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
# Already complete on Thabiso's machine; only apply after checking the target database:
dotnet tool run dotnet-ef database update --configuration Release --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
dotnet run --project src/RaCeDay/RaCeDay.csproj -- --seed-roles
dotnet run --project src/RaCeDay/RaCeDay.csproj
```

SQL verification, read-only:

```powershell
sqlcmd -S localhost -E -C -d RaCeDayPart2_SQLServer_Demo_20261018 -Q "SELECT name FROM sys.tables ORDER BY name; SELECT MigrationId FROM dbo.__EFMigrationsHistory ORDER BY MigrationId; SELECT Name FROM dbo.AspNetRoles ORDER BY Name"
```

See [SQL Server 2022 verification](docs/MSSQLSERVER_20261018.md) for checked evidence and the remaining mismatch with Part 1.

Migrations are never applied automatically when the website starts. The explicit seed command creates role names and does **not** create any accounts or grant Admin. Users select Organiser or Participant at public registration; **Admin cannot be self-selected**.

### Demo walkthrough

1. Register and sign in as a Participant.
2. Register as an Organiser and create an event with 5 km and 10 km categories.
3. As Participant, browse Events, enrol in one or both categories, and inspect My entries.
4. Enter a Pending payment attempt for the exact fee. The UI and API must never claim it was Paid.
5. As Organiser, close the event and record a result for an enrolment.
6. As Participant, view My results and confirm unrelated users cannot see them.
7. In Development open `/swagger`. For API register/login obtain a token from anonymous `GET /api/auth/antiforgery`, then set `X-CSRF-TOKEN` in Swagger's Authorize field. After signing in, refresh this token via `GET /api/auth/antiforgery` or `GET /api/antiforgery` before authenticated writes. The Identity cookie must be in the same browser session.

See the [Part 2 rubric evidence index](docs/PART2_EVIDENCE_INDEX.md), `docs/PAYMENT_WORKFLOW.md`, `docs/SCHEMA_PARITY_ADDENDUM.md`, `docs/SUBMISSION_VIDEO_SCRIPT.md` and `docs/DEVELOPMENT-STATUS.md` for requirements and evidence. This is an unsubmitted recovery copy for **Thabiso Khumalo**.

## Outstanding before submission

**Blocking:** Reconcile the EF Core database with the approved Part 1 ERD/SQL and recover/compare any original project. Official rubric is now available visually; its 100-mark breakdown is in `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This is not a grade prediction.

- Part 1 planned JWT-based authentication, while Part 2 asks for user/role sessions. RaCeDay uses an Identity cookie plus a server-side memory-backed session carrying the user ID and role; no JWT is issued. The memory store is suitable for single-instance coursework and would require a distributed store for scaled production.
- Real payment gateway and trusted settlement are not implemented. Payment records remain Pending.
- Confirm the lecturer's role registration model, dashboard requirements and field names.
- The Part 2 branch was pushed with permission and its GitHub Actions workflow has passed remotely (32/32 tests on run #39068333340); verify that the commit history meets the lecturer's qualitative definition of 20 meaningful Part 2 commits.
- Produce genuine browser, Swagger, SQL Server/SSMS and Git history evidence screenshots and an **unlisted YouTube walkthrough**, then add the video link to README.
- Test deployment/release environment and production email confirmation if required.
- Only the approved Part 2 recovery branch has been pushed. It has **not** been merged into `main` and nothing has been submitted to ARC.
