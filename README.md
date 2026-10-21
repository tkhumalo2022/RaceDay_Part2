# RaCeDay — PROG6212 Part 2

**Student:** Thabiso Khumalo (ST10477675)  
**Module:** PROG6212 Programming 2B  
**Assessment:** Portfolio of Evidence, Part 2 — RESTful API Development

RaCeDay is an ASP.NET Core race management application for organisers and participants. Organisers manage events, categories, entries and results. Participants register, enter races and view their results.

## Submission links

- **Repository:** https://github.com/tkhumalo2022/RaceDay_Part2
- **Source code:** [ASP.NET Core project](src/RaCeDay) · [automated tests](tests/RaCeDay.Tests) · [commit history](https://github.com/tkhumalo2022/RaceDay_Part2/commits/main)
- **Presentation:** [RaCeDay Part 2 video](https://youtu.be/jytTUheNxYw)
- **Build and tests:** [Successful GitHub Actions run — 19 October 2026](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39265809171) (39 passed, 0 failed, 0 skipped)
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
