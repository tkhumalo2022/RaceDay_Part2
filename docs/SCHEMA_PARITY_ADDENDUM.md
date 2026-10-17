# Part 1 schema reconciliation — implemented additions

Developer: **Thabiso Khumalo**. This document supplements `PART1_SCHEMA_RECONCILIATION.md`, not a claim of identical schemas.

The original Part 1 SQL and approval instructions are in `docs/part1`. **Do not run that SQL against valuable data:** it explicitly drops `RaceDayDB`.

## New forward-only EF Core migrations

- `Part1CategoryCountAndPaymentRules`: adds a `CurrentParticipants` column with constraints to `Race_Category`, backfills existing registered entries, and uses the SQL trigger `trg_Enrolment_UpdateCategoryCount` to keep that count current on insert/update/delete. Only entries with status `Registered` count, exactly as in Part 1. Adds `trg_Payment_ValidateAmount` to reject a payment amount different from the category fee **even if data is inserted directly into SQL Server**. EF Core is configured to work with the triggers.
- `RoleRegistrationProfileFields`: adds `DateOfBirth`, `OrganiserName`, and `CreatedAt` to `AspNetUsers`. New Participant registrations require a valid date of birth; new Organiser registrations require an organisation name. Existing accounts remain usable. It does **not** reduce the existing phone number column's length, avoiding truncation risk.

## Verified behavior

- SQL Server integration tests `Part1CategoryCountsAreStoredAndUpdatedBySqlServerTrigger` and `Part1PaymentTriggerRejectsIncorrectAmountsEvenOutsideApi` verify real trigger updates and direct SQL rejection.
- New validation test `RegistrationRequiresRoleSpecificDetails` checks missing role-specific details are rejected.
- All tests operate on newly named isolated LocalDB databases. They do not migrate `RaceDayDB` or the original student's app DB.

## Additional Part 1-compatible input validation (17 October)
- Participant `Gender` follows Part 1's permitted values: `Male`, `Female`, `Other`, or `Prefer not to say`. Leaving it blank remains optional. MVC registration/profile and API registration/profile all enforce this allowlist; MVC forms provide the corresponding choices.
- A new integration test rejects an invalid registration value, accepts `Prefer not to say`, and rejects an invalid profile update. A separate test covers whitespace-only event names, locations and category names.
- The isolated validation branch passed **34 LocalDB tests** and a Release build with **zero warnings/errors**; this statement is local evidence only, not a claim about a new GitHub CI run.
- **Still different physically:** `AspNetUsers.Gender` has application-level validation, but it does not have the `CK_Participant_Gender` SQL CHECK constraint of Part 1. The separate role tables/keys also remain unresolved.

## Remaining physical schema differences

- Part 1 stores separate `Organiser` and `Participant` tables with numeric IDs. Part 2 uses a shared `AspNetUsers` table with ASP.NET Identity string IDs, plus `AspNetUserRoles`. The new user fields reduce content gaps but **do not** make the physical schemas identical.
- Legacy Event and Enrolment FKs use numeric role-table IDs, while EF uses Identity user string IDs.
- Part 1 uses snake_case SQL column names for domain entities, while EF Core currently maps PascalCase properties.
- Part 1 sample SQL stores `CurrentParticipants` as a trigger-maintained count of registrations; this now matches behaviorally and physically, except column naming.
- Part 1 suggests JWT auth in its endpoint plan. Official Part 2 requires sessions; our API uses Identity cookie sessions, documented in README.

The largest remaining 25-mark rubric risk is **exact ERD/SQL physical correspondence**. Before replacing relationship keys and migrating real data, seek lecturer approval for the Identity design or complete a separate, tested redesign with numeric role tables and explicit data backfill. No grade percentage is warranted without assessment.
