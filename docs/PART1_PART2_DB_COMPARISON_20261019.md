# PROG6212 Part 2 — SQL Server schema check (19 October 2026)

Student: Thabiso Khumalo
Code branch: `recovery/part2-20261017`

I compared the **actual live table and column metadata** in SQL Server on `localhost`, using read-only queries, not just the EF models. These databases were checked:

- Part 1 original: `RaceDayDB` (existing; not modified).
- Part 2 demonstration: `RaCeDayPart2_SQLServer_Demo_20261018` (existing; not modified).

## What already agrees

| Part 1 table | Part 2 table | Status |
| --- | --- | --- |
| Event | Event | Table exists; event data and date rules work |
| Race_Category | Race_Category | Table exists; participant count and limits supported |
| Enrolment | Enrolment | Table exists; enrolment-to-event/category links enforced |
| Payment | Payment | Table exists; amount, method, status and references supported |
| Race_Result | Race_Result | Table exists; result time/status constraints supported |

The five tables do **not** have identical columns. The Part 2 tables use EF Core property names such as `EventName`, `CategoryId`, `PaymentMethod`, not the Part 1 SQL names `Event_Name`, `Category_ID`, `Payment_Method`. Part 1 generally uses `varchar` for labels; Part 2 typically uses `nvarchar`.

## Important structural differences

| Part 1 (physically present) | Part 2 (physically present) | What changes |
| --- | --- | --- |
| `Organiser.Organiser_ID INT` | `AspNetUsers.Id NVARCHAR(450)` plus `AspNetUserRoles` | No separate `Organiser` table; primary key and identity architecture differ |
| `Participant.Participant_ID INT` | `AspNetUsers.Id NVARCHAR(450)` plus `AspNetUserRoles` | No separate `Participant` table; primary key and identity architecture differ |
| `Event.Organiser_ID INT` | `Event.OrganiserId NVARCHAR(450)` | Foreign key targets Identity user, not the legacy Organiser table |
| `Enrolment.Participant_ID INT` | `Enrolment.ParticipantId NVARCHAR(450)` | Foreign key targets Identity user, not the legacy Participant table |
| `Event.Created_At DATETIME2` | No `Event.CreatedAt` column | Timestamp field is missing from Part 2 event records |
| `Enrolment.Enrolment_ID INT` | `Enrolment.Id INT` | Same underlying type, different physical column name |
| `Race_Result.Result_ID INT` | `Race_Result.Id INT` | Same underlying type, different physical column name |
| Original project prescribed a token/JWT login design | Part 2 uses ASP.NET Core Identity cookies + role/session | Authentication protocol changed with the rubric's session requirement |

Part 2 has ASP.NET Core Identity tables for secure hashed passwords, roles and lockout. The highest Part 2 API/database band expects the database to match Part 1's ERD and SQL script, so **do not claim the two physical schemas match**.

## Safer ways to resolve this

1. **If the lecturer approves an Identity-based evolution of Part 1**: keep the existing tested approach, explain that `AspNetUsers` replaces separate credential/profile storage and that user roles are linked in `AspNetUserRoles`. The database rules for events, categories, enrolments, payments and results are carried over.
2. **If exact Part 1 physical alignment is mandatory**: add an `Organiser` and `Participant` profile entity with `INT IDENTITY` keys, link each to one `AspNetUsers.Id` account, and change event/enrolment foreign keys consistently. Also plan column mappings and migration of existing records, not just new tables. Create a backed-up **new demonstration database** for this experiment and test it before touching any existing database.
3. Do not run the original Part 1 SQL script against `RaceDayDB`. It contains `DROP DATABASE RaceDayDB`.

## What to show in the presentation

- SQL Server version, Part 2 database and EF migrations.
- Five racing-domain tables, seven Identity tables, roles, SQL triggers and foreign-key relationships.
- Explain the Part 1 change plainly, but do not imply the lecturer has approved it.
- Show the working application and 35 local passing tests and GitHub Actions separately. Local tests use isolated LocalDB, not the persistent Part 2 demonstration database.

## Read-only metadata query

Run this only with `RaceDayDB` or `RaCeDayPart2_SQLServer_Demo_20261018` selected:

```sql
SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS DataType,
       c.max_length AS MaxLength
FROM sys.tables t
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Organiser','Participant','AspNetUsers',
                 'Event','Race_Category','Enrolment','Payment','Race_Result')
ORDER BY t.name, c.column_id;
```

No migrations, schema or user accounts were changed during this check.
