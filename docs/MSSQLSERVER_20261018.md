# Microsoft SQL Server 2022 verification — 18 October 2026

**Student:** Thabiso Khumalo  
**Purpose:** Preserve the Part 1 SQL Server schema and verify that the Part 2 recovery app runs on a separate **full SQL Server** database (not LocalDB).

## Server discovery

- Default SQL Server service `MSSQLSERVER`: **Running** on the student's Windows laptop.
- `sqlcmd -S localhost -E -C` connected successfully using Windows Authentication.
- Engine version: **16.0.1000.6** (SQL Server 2022).
- The Windows login had permission to create a separate database.
- LocalDB remains available for the existing isolated automated SQL Server tests.
- SQL Server Management Studio GUI was not detected; `sqlcmd` is available and was used for all SQL verification.

## Original Part 1 database — read-only

- Existing `RaceDayDB` was present and ONLINE.
- Read-only inspection of `INFORMATION_SCHEMA.TABLES` found the seven Part 1 domain tables: `Organiser`, `Participant`, `Event`, `Race_Category`, `Enrolment`, `Payment` and `Race_Result`.
- Read-only inspection of `sys.columns` confirmed legacy names and types, including `Organiser.Organiser_ID int`, `Participant.Participant_ID int`, `Event.Organiser_ID int` and `Enrolment.Participant_ID int`.
- **No EF migration, CREATE/DROP/ALTER, cleanup or data replacement operation was run against `RaceDayDB`.** Do not run `docs/part1/RaceDay_Database.sql` against a valuable database; its opening section drops `RaceDayDB`.

## Separate Part 2 demonstration database — created and migrated

- New database: `RaCeDayPart2_SQLServer_Demo_20261018`.
- Created only after checking that this name **did not already exist**.
- Set `ConnectionStrings__DefaultConnection` to use `Server=localhost` with Windows Authentication, then ran the project's restored `dotnet-ef 10.0.10` CLI.
- **All six migrations successfully applied**, including SQL Server database triggers:

```
20261017163300_InitialIdentity
20261017171651_RaceWorkflowCore
20261017173003_PaymentAttempts
20261017173210_PerCategoryEnrolmentUniqueness
20261017193435_Part1CategoryCountAndPaymentRules
20261017194257_RoleRegistrationProfileFields
```

- A SQL Server query verified 13 tables: 7 ASP.NET Identity tables, `Event`, `Race_Category`, `Enrolment`, `Payment`, `Race_Result`, and `__EFMigrationsHistory`.
- Explicit role seeding succeeded for **Admin, Organiser and Participant** role names. It did not create an Admin user.
- Read-only trigger inspection verified `trg_Enrolment_UpdateCategoryCount` (Enrolment) and `trg_Payment_ValidateAmount` (Payment).
- The application's checked-in `appsettings.json` now targets the **new separate SQL Server database**. Automated test fixtures continue to override the connection with isolated, GUID-named LocalDB databases.

## Verifiable SQL Server read-only commands

```powershell
# Inspect original domain tables without changing the original DB
sqlcmd -S localhost -E -C -d RaceDayDB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"

# Inspect migrated Part 2 database
sqlcmd -S localhost -E -C -d RaCeDayPart2_SQLServer_Demo_20261018 -Q "SELECT name FROM sys.tables ORDER BY name; SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId; SELECT Name FROM AspNetRoles ORDER BY Name"

# Inspect SQL triggers
sqlcmd -S localhost -E -C -d RaCeDayPart2_SQLServer_Demo_20261018 -Q "SELECT name, OBJECT_NAME(parent_id) AS ParentTable FROM sys.triggers WHERE name IN ('trg_Enrolment_UpdateCategoryCount','trg_Payment_ValidateAmount')"
```

## Remaining assessment issue

The two databases are **not physically equivalent**: Part 1 uses independent integer-key `Organiser` and `Participant` entities linked to other domain tables; Part 2 uses Identity `AspNetUsers` and string FKs. The app is functional with EF Core, but this does **not** fulfil a strict interpretation of 'exactly match the Part 1 ERD'. See [the reconciliation](PART1_SCHEMA_RECONCILIATION.md). Any structural redesign or migration of existing `RaceDayDB` needs a reviewed plan and backup. A lecturer may accept an Identity-based evolution, but that acceptance has **not** been obtained or claimed.

This document records actual server/database verification, **not** screenshots, a recorded video or ARC submission.
