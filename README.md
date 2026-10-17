# RaCeDay — PROG6212 POE Part 2 (recovery working copy)

**Status:** Active, isolated recovery branch `recovery/part2-20261017`. The original student Visual Studio application has not been recovered. The official Part 2 rubric was visually reviewed in the protected SharePoint document (pages 9–11 and 17–18); see `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This working copy was built from the student's Part 2 Identity foundation and reconciled against the accessible Part 1 API/database plan. It must not be represented as the original recovered app or as a rubric-complete submission.

## What works

- ASP.NET Core MVC / .NET 10 with SQL Server EF Core and ASP.NET Identity.
- Register and sign in as either Organiser or Participant (MVC or JSON API); password hashing, Identity session cookies, CSRF protection and guarded Admin / Organiser / Participant dashboards. Public registration **rejects Admin**.
- Profile editing through both MVC and GET/PUT `/api/users/me`, restricted to the signed-in user with allowlisted fields and anti-forgery protection.
- Organiser event and category CRUD, including validation and ownership checks.
- Participant enrolment with deadline, capacity, category-event consistency and **one entry per category per participant**. SQL Server locking prevents simultaneous overbooking.
- Organiser result recording; participants can view their own results.
- Cookie-authenticated REST APIs including `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/logout` and `GET /api/auth/antiforgery`, plus Swagger UI, MVC pages and a generic production error page.
- Pending-only payment-attempt records with amount validation, ownership control, unique transaction references and an enforced maximum of one `Paid` row per enrolment. **No real payment processing or payment confirmation exists.**
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
dotnet ef migrations list --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
dotnet ef database update --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj
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

See `docs/PAYMENT_WORKFLOW.md` and `docs/DEVELOPMENT-STATUS.md` for scope and constraints.

## Outstanding before submission

**Blocking:** Reconcile the EF Core database with the approved Part 1 ERD/SQL and recover/compare any original project. Official rubric is now available visually; its 100-mark breakdown is in `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. This is not a grade prediction.

- Part 1 planned JWT-based authentication, while the official Part 2 brief explicitly requires user/role sessions. JSON register/login endpoints are implemented with ASP.NET Identity cookies; this contract difference must be documented and reviewed.
- Real payment gateway and trusted settlement are not implemented. Payment records remain Pending.
- Confirm the lecturer's role registration model, dashboard requirements and field names.
- Achieve at least **20 authentic, meaningful Part 2 commits** (never filler commits), run the new GitHub Actions workflow and collect a green CI screenshot. Local workflow is configured but has not run remotely.
- Produce genuine browser, Swagger, LocalDB/SSMS and Git history evidence screenshots and an **unlisted YouTube walkthrough**, then add the video link to README.
- Test deployment/release environment and production email confirmation if required.
- Nothing is pushed, merged or submitted automatically.
