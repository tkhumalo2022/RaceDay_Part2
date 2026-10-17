# Part 1 SQL and Part 2 EF Core schema reconciliation

Prepared 17 October 2026. Compared the submitted Part 1 SQL script in `docs/part1/RaceDay_Database.sql` with `src/RaCeDay/Models/Racing.cs` and `src/RaCeDay/Data/ApplicationDbContext.cs`. This is a **gap analysis, not a claim that the schemas match**.

| Part 1 requirement | Part 2 implementation | Status |
| --- | --- | --- |
| Separate `Organiser` and `Participant` tables with INT identity keys | Identity `AspNetUsers` shares string IDs; role assignments are in `AspNetUserRoles` | DIFFERENT; explicit lecturer approval or migration redesign required |
| `Organiser_Name`, participant date of birth, created timestamps | Shared `ApplicationUser` now includes `OrganiserName`, `DateOfBirth`, `CreatedAt`, validated during role signup | CONTENT ALIGNED, PHYSICAL TABLES STILL DIFFER |
| `Event` table, owner foreign key, event date/location/type/status, closing date | `Event` table and validations exist, but owner FK uses Identity string and columns are PascalCase | PARTIALLY ALIGNED |
| `Race_Category` with `Current_Participants` and trigger maintained count | `CurrentParticipants` is now stored with SQL Server insert/update/delete trigger and backfill; lock protects capacity | BEHAVIOUR MATCHES, COLUMN NAMES DIFFER |
| Enrolment composite FK (category,event), unique participant+category and race number | Composite FK and uniqueness configured; participant FK is Identity string | PARTIALLY ALIGNED |
| Payment method/status/amount checks, unique reference and one paid payment | EF constraints and filtered indexes, plus SQL Server amount-validation trigger; API creates Pending only | DATABASE RULE ALIGNED, COLUMN NAMES DIFFER |
| Result table unique enrolment and finish-time/status checks | Corresponding indexes/checks configured | BEHAVIOUR ALIGNED, COLUMN NAMES DIFFERENT |
| Part 1 login plan returns JWT | Official Part 2 explicitly asks user/role session; ASP.NET Identity cookie session used | DOCUMENTED CONTRACT CHANGE |

## Safe resolution options

1. **Ask the lecturer before claiming Part 1 ERD parity.** The highest-scoring rubric explicitly requires a matching schema. The current recovery branch does not satisfy that strictly.
2. If exact physical parity is necessary, create new `Organiser` and `Participant` EF entities with integer IDs and one-to-one links to `AspNetUsers`, change foreign keys consistently, and write forward-only EF migrations against a **new demonstration database**. Do not rename keys or retrofit the original live database without migration tests and backup.
3. If Identity integration is allowed as an approved evolution of Part 1, retain the existing implementation and explicitly explain column/table mapping, privacy, hashing, and session behaviour in the video.
4. Never execute `docs/part1/RaceDay_Database.sql` on an existing database: its first section **drops `RaceDayDB`**. Test only on isolated new LocalDB databases.

## Evidence required before submission

- Compare approved Part 1 ERD diagram as well as SQL script against the EF migration SQL.
- Capture a new LocalDB demonstration database, `__EFMigrationsHistory`, constraints, tables, and schema diagram.
- Record signed-in Swagger create/update/delete, invalid requests, participant enrolment, result retrieval, and role rejection.
- Confirm whether ASP.NET Identity cookie session satisfies the brief's reference to server-side session.
- Record true GitHub CI outcome, twenty meaningful commits, and an unlisted YouTube walkthrough.

**Not safe to state yet:** schema is identical to Part 1, CI is green, original student Visual Studio project was recovered, YouTube video submitted, or marks guaranteed.
