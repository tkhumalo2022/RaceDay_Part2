# Part 2 development status — 17 October 2026

This is an isolated continuation of the Identity foundation, **not the recovered original application**. Original source and official rubric remain unavailable.

## Verified implementation

- Consistent `ApplicationUser` / `ApplicationDbContext`, custom registration and login.
- SQL Server `InitialIdentity` migration and model snapshot; custom profile columns included.
- Reviewable idempotent SQL in `InitialIdentity.sql`. Web startup never applies migrations.
- Explicit `--seed-roles` command creates Admin, Organiser and Participant roles idempotently. It does not grant privileges to accounts.
- Protected role dashboard pages and routing. These are account dashboards, not completed event/statistical dashboards.
- Current-user profile editing with validation, anti-forgery protection, and an allowlisted view model. Role, email and arbitrary user ID fields cannot be changed through this form.
- Real SQL Server LocalDB integration tests exercise registration, password rejection/success, role injection prevention, role seeding, anonymous and wrong-role access, profile updates and CSRF rejection.

## Requirements checklist

Mark allocations are **unknown**, not zero. This table tracks the user's requested work; it must not be presented as the official marking rubric.

| Requested area | Official marks | Status | Evidence still needed |
| --- | --- | --- | --- |
| Registration/login | Unknown | Local SQL integration tests pass | Lecturer/rubric alignment; screenshots; duplicate-email and lockout test expansion |
| ApplicationUser/DbContext | Unknown | Consistent; migration tested | Compare original if recovered |
| SQL Server EF migrations | Unknown | Initial migration generated and executed on new test databases | Demonstration database setup and submission screenshots |
| Admin/Organiser/Participant roles | Unknown | Seeded and authorization tested | Official role-provisioning rules; privileged account onboarding |
| Role dashboards and profile | Unknown | Protected account dashboards; basic editable profile | Event statistics, profile images, exact required fields |
| Events/race categories | Unknown | Not implemented | Rubric and current Part 1 schema reconciliation |
| Enrolments/results | Unknown | Not implemented | Workflow rules, ownership/capacity tests |
| API/Swagger | Unknown | Not implemented | Confirm required endpoints and JWT/cookie contract |
| Validation/error handling/tests | Unknown | 13 tests pass; partial coverage | Broader failure cases and domain tests; production error route |
| Evidence/documentation | Unknown | Recovery and development reports; SQL backup | Official rubric, screenshots/video, final evidence mapping |

## Recovery and rubric findings

The original 125-file backup remains intact. The excluded other student's repository and `C:\Users\BB` were not used.

Additional searches examined Recycle Bin metadata, accessible Visual Studio history/configuration, browser download records and ZIP entry names under the user's main folders. No original project was found. The Recycle Bin contained two ICE Task 4 documents, not the missing project.

Chrome history confirms former downloads `Mfundo-RaceDay-Starter.zip` and `Learning_About _DEVELOPMENT_GUIDELINE(POE-PROG6212-GUIDE).pdf`; both are absent at their recorded paths. The former is not accepted as Thabiso's original work. No ZIP containing matching project files was found in the scanned folders.

Recovered course links:

- [Part 2 materials](https://mystudies.iie.edu.za/d2l/le/lessons/95437/units/6681391)
- [Assessment guide](https://mystudies.iie.edu.za/d2l/le/lessons/95437/topics/6830036)
- [POE guide](https://mystudies.iie.edu.za/d2l/le/lessons/95437/topics/6919536)

The browser rejected ARC access because permission was declined. No alternative access route or encryption bypass was attempted. The local POE DOCX remains encrypted.

## Safe local commands

Open `RaCeDay.slnx` in Visual Studio, or run from this repository:

```powershell
dotnet tool restore
dotnet restore
dotnet build -c Release --warnaserror
dotnet test -c Release
```

Tests require Windows SQL Server LocalDB. Each integration run creates a new `RaCeDayTests_<GUID>` database containing synthetic accounts and retains it for inspection. Tests never migrate or delete an existing application database. Test passwords are synthetic fixtures only.

For manual application use, choose a **new isolated database** and set its connection explicitly. Review `docs/InitialIdentity.sql` before applying it. Back up existing data and obtain approval before destructive schema changes.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=RaCeDayRecovery_NEW_UNIQUE_NAME;Trusted_Connection=True;TrustServerCertificate=True'
dotnet ef database update --project src/RaCeDay
dotnet run --project src/RaCeDay -- --seed-roles
dotnet run --project src/RaCeDay
```

The configured original database was not altered. A COPY_ONLY/CHECKSUM backup of the last successful test database is stored in the sibling `outputs/database-backups` directory and passed SQL Server `RESTORE VERIFYONLY WITH CHECKSUM`. This verifies backup readability, not a full restore exercise.

No remote push, merge, deployment or ARC submission occurred. Production email confirmation/recovery requires a real email sender; production readiness is not claimed.
