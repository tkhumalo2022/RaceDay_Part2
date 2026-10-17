# RaceDay Part 2: Identity setup

This is an **isolated foundation**, not the complete PROG6212 Part 2 project.
The student's existing Visual Studio solution has not yet been located or merged.

## What matches the lecturer's ARC reference

- `ApplicationUser : IdentityUser` has FirstName, LastName, Gender and a future profile-image path.
- `ApplicationDbContext : IdentityDbContext<ApplicationUser>` uses SQL Server.
- Login, registration, role seeding and the navbar all reference `ApplicationUser`.
- Public registration adds only the **Participant** role; Admin/Organiser must be granted separately.
- Identity role names: Admin, Organiser and Participant.
- The sample ARC `RunWayy` namespace has **not** been copied into RaCeDay.
- Lecturer's `Pages.zip` is reference material only and is **not** published here.

## Required before running registration

1. Install .NET SDK 10 and a SQL Server instance (Windows LocalDB is configured by default).
2. Install EF CLI using `dotnet tool install --global dotnet-ef --version 10.0.10`.
3. Run `dotnet ef migrations add InitialIdentity --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj --output-dir Data/Migrations`.
4. Run `dotnet ef database update --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj`.
5. Run `dotnet run --project src/RaCeDay/RaCeDay.csproj`. In Development, the app applies migrations and seeds the three roles.
6. Confirm registration, login, logout, invalid passwords and role assignment in the actual database.

**Note:** The default DB name is `RaCeDayPart2Identity`, separate from Part 1.
Do not run migrations against your original RaceDay database without reviewing its schema.

**Security:** Email confirmation is relaxed only in Development; production requires
an email sender. There is no hard-coded admin password. Never self-assign Admin
or Organiser through public registration.

## Not yet done

- Integrate the actual existing student's project and preserve prior features.
- Generate/review/commit SQL Server EF migrations.
- Test registration, email sender, identity storage, and protected-role flows.
- Implement Events, Categories, Enrolments, Results and Payments aligned to Part 1.
- Implement the planned API authentication approach and Swagger.
- Add unit/integration tests and actual screenshots for submission.
