# RaCeDay — PROG6212 POE Part 2 (recovery working copy)

**Student:** Thabiso Khumalo

**Status:** Active, isolated recovery branch `recovery/part2-20261017`. The original student Visual Studio application has not been recovered. The official Part 2 rubric was visually reviewed in the protected SharePoint document (pages 9–11 and 17–18); see `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This working copy was built from the student's Part 2 Identity foundation and reconciled against the accessible Part 1 API/database plan. It must not be represented as the original recovered app or as a rubric-complete submission.

## What works

- ASP.NET Core MVC / .NET 10 with SQL Server EF Core and ASP.NET Identity.
- Register and sign in as either Organiser or Participant (MVC or JSON API); role-specific details (date of birth for Participants, organisation name for Organisers), password hashing, Identity session cookies, CSRF protection and guarded dashboards. Public registration **rejects Admin**.
- Profile editing through both MVC and GET/PUT `/api/users/me`, restricted to the signed-in user with allowlisted fields and anti-forgery protection.
- Organiser event and category CRUD, including validation and ownership checks.
- Participant enrolment with deadline, capacity, category-event consistency and **one entry per category per participant**. SQL Server locking prevents simultaneous overbooking. A SQL trigger now maintains stored `CurrentParticipants` exactly as the Part 1 `Registered`-only rule requires.
- Organiser result recording; participants can view their own results.
- Cookie-authenticated REST APIs including `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout`, `GET /api/auth/antiforgery` and `GET /api/auth/session`. A **server-side memory-backed session** also stores the signed-in user ID and role, as specified by the Part 2 marking rubric; ASP.NET Identity claims remain the source of authorization. Logout clears both. Swagger UI, MVC pages and the generic error page are available.
- Pending-only payment-attempt records with both API and **SQL-trigger** entry-fee amount validation, ownership control, unique transaction references and at most one `Paid` record per enrolment. **No real payment processing or confirmation exists.**
- Automated SQL Server LocalDB integration tests for roles, Identity, forms, routes, business rules, concurrency and payment records.

## Requirements

Windows with .NET SDK 10, SQL Server LocalDB, and the `dotnet-ef` CLI (10.x). All commands below run in this repository root. Open `RaCeDay.slnx` in Visual Studio or a shell.

```powershell
dotnet tool restore
dotnet restore
dotnet build RaCeDay.slnx -c Release --warnaserror
dotnet test RaCeDay.slnx -c Release
```

Tests create isolated `RaCeDayTests_<GUID>` databases with synthetic data and retain them for inspection. They must **never** run against the original RaceDay database.

### Demonstration database (do not use an existing database)

Use a distinct name, inspect migrations, and explicitly apply them. This example must not be reused to overwrite student work.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=RaCeDayRecoveryDemo_NEW;Trusted_Connection=True;TrustServerCertificate=True'
dotnet ef migrations list --configuration Release --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
dotnet ef database update --configuration Release --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
dotnet run --project src/RaCeDay/RaCeDay.csproj -- --seed-roles
dotnet run --project src/RaCeDay/RaCeDay.csproj
```

Migrations are never applied automatically when the website starts. The explicit seed command creates role names and does **not** create any accounts or grant Admin. Users select Organiser or Participant at public registration; **Admin cannot be self-selected**.

### Demo walkthrough

1. Register and sign in as a Participant.
2. Register as an Organiser and create an event with 5 km and 10 km categories.
3. As Participant, browse Events, enrol in one or both categories, and inspect My entries.
4. Enter a Pending payment attempt for the exact fee. The UI and API must never claim it was Paid.
5. As Organiser, close the event and record a result for an enrolment.
6. As Participant, view My results and confirm unrelated users cannot see them.
7. In Development open `/swagger`. For API register/login obtain a token from anonymous `GET /api/auth/antiforgery`, then set `X-CSRF-TOKEN` in Swagger's Authorize field. After signing in, refresh this token via `GET /api/auth/antiforgery` or `GET /api/antiforgery` before authenticated writes. The Identity cookie must be in the same browser session.

See `docs/PAYMENT_WORKFLOW.md`, `docs/SCHEMA_PARITY_ADDENDUM.md`, `docs/SUBMISSION_VIDEO_SCRIPT.md` and `docs/DEVELOPMENT-STATUS.md` for requirements and evidence. This is an unsubmitted recovery copy for **Thabiso Khumalo**.

## Outstanding before submission

**Blocking:** Reconcile the EF Core database with the approved Part 1 ERD/SQL and recover/compare any original project. Official rubric is now available visually; its 100-mark breakdown is in `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This is not a grade prediction.

- Part 1 planned JWT-based authentication, while Part 2 asks for user/role sessions. RaCeDay uses an Identity cookie plus a server-side memory-backed session carrying the user ID and role; no JWT is issued. The memory store is suitable for single-instance coursework and would require a distributed store for scaled production.
- Real payment gateway and trusted settlement are not implemented. Payment records remain Pending.
- Confirm the lecturer's role registration model, dashboard requirements and field names.
- Verify that the **20 local commits represent meaningful Part 2 work**, push the branch only with approval, run the new GitHub Actions workflow and collect its genuine green CI screenshot. Local workflow is configured but has not run remotely.
- Produce genuine browser, Swagger, LocalDB/SSMS and Git history evidence screenshots and an **unlisted YouTube walkthrough**, then add the video link to README.
- Test deployment/release environment and production email confirmation if required.
- Nothing is pushed, merged or submitted automatically.
