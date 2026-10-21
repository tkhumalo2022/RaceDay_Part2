# RaceDay — verified Identity setup (recovery branch)

This file replaces an earlier starter-only checklist which incorrectly implied migrations and roles were auto-created on startup.

## Implemented

- `ApplicationUser : IdentityUser` has FirstName, LastName, Gender and ProfileImagePath.
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` uses SQL Server.
- Registration and login use `ApplicationUser` consistently.
- Roles: Admin, Organiser, Participant. **Public registration lets users choose Organiser or Participant only.** Admin is never accepted through public registration; API and MVC flows are tested.
- In Development, email confirmation is not required; outside Development it is required, and a real email sender must be added before production.
- Role dashboards, profile editing, anti-forgery checks and SQL-backed integration tests exist.

## Setup

From the project root:

```powershell
dotnet restore
dotnet build RaCeDay.slnx -c Release
dotnet test RaCeDay.slnx -c Release
```

For manual demonstration, select a **new** LocalDB database, follow the step-by-step environment variable and EF migration commands in `README.md`, then run `--seed-roles` and the application. Do not apply migrations to `RaceDayDB`, `RaCeDayPart2Identity`, or another existing database without a reviewed backup. Migrations never run during normal web startup.

## Scope caveat

The original student solution has not been obtained. The official Part 2 rubric was visually inspected and mapped in `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md`. The Part 1 plan mentions JWT, but this implementation deliberately uses cookie-based Identity sessions to match the official Part 2 session requirement. JSON register/login/logout endpoints exist. Pending payment attempts are not real payments.
