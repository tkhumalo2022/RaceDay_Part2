# RaCeDay — PROG6212 POE Part 2 (recovery working copy)

**Status:** Active, isolated recovery branch `recovery/part2-20261017`. The original student Visual Studio application has not been recovered, and the official Part 2 rubric has not been verified. This working copy was built from the student's Part 2 Identity foundation and reconciled against the accessible Part 1 API/database plan. It must not be represented as the original recovered app or as a rubric-complete submission.

## What works

- ASP.NET Core MVC / .NET 10 with SQL Server EF Core and ASP.NET Identity.
- Register and sign in as Participant; verified account roles and guarded Admin / Organiser / Participant dashboards. Public registration cannot create a privileged account.
- Profile editing through both MVC and GET/PUT `/api/users/me`, restricted to the signed-in user with allowlisted fields and anti-forgery protection.
- Organiser event and category CRUD, including validation and ownership checks.
- Participant enrolment with deadline, capacity, category-event consistency and **one entry per category per participant**. SQL Server locking prevents simultaneous overbooking.
- Organiser result recording; participants can view their own results.
- Cookie-authenticated REST APIs with CSRF protection, Swagger UI, MVC pages and generic production error page.
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

Migrations are never applied automatically when the website starts. The explicit seed command creates role names but does **not** promote a user to Admin or Organiser. Grant the Organiser role only through a deliberate, authorised Identity administration procedure; do not allow public self-escalation.

### Demo walkthrough

1. Register and sign in as a Participant.
2. With a legitimately provisioned Organiser account, create an event and add 5 km and 10 km categories.
3. As Participant, browse Events, enrol in one or both categories, and inspect My entries.
4. Enter a Pending payment attempt for the exact fee. The UI and API must never claim it was Paid.
5. As Organiser, close the event and record a result for an enrolment.
6. As Participant, view My results and confirm unrelated users cannot see them.
7. In Development open `/swagger`. Sign in first via `/Identity/Account/Login`. For API writes, call `GET /api/antiforgery` and set the returned request token as the `X-CSRF-TOKEN` header (Swagger's Authorize field). Cookie auth must still be present in the same browser.

See `docs/PAYMENT_WORKFLOW.md` and `docs/DEVELOPMENT-STATUS.md` for scope and constraints.

## Outstanding before submission

**Blocking:** Obtain the official PROG6212 Part 2 rubric and reconcile this copy with any original project that becomes available. These are not confirmed requirements/marks.

- Part 1 planned JWT-based `/api/auth/register` and `/api/auth/login`; this recovery branch currently uses Identity cookie login instead. Decide only after official Part 2 instructions are available.
- Real payment gateway and trusted settlement are not implemented. Payment records remain Pending.
- Confirm the lecturer's role registration model, dashboard requirements and field names.
- Produce genuine browser, Swagger, LocalDB/SSMS and Git history evidence screenshots, then map each to verified rubric rows.
- Test deployment/release environment and production email confirmation if required.
- Nothing is pushed, merged or submitted automatically.
