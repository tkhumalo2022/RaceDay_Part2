# RaceDay — verified Identity setup (recovery branch)

This file replaces an earlier starter-only checklist which incorrectly implied migrations and roles were auto-created on startup.

## Implemented

- `ApplicationUser : IdentityUser` has FirstName, LastName, Gender and ProfileImagePath.
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` uses SQL Server.
- Registration and login use `ApplicationUser` consistently.
- Roles: Admin, Organiser, Participant. **Public registration assigns Participant only.** Other roles require authorised administration.
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

The original student solution and official Part 2 rubric have not been obtained. The Part 1 plan mentions JWT login and registration API endpoints; this current recovery branch uses cookie-based Identity and has not implemented those two API auth endpoints. Payment attempts are Pending-only, not real payments. See `docs/PAYMENT_WORKFLOW.md`.
