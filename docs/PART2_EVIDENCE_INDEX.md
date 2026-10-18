# PROG6212 Part 2 — evidence index (18 October 2026)

**Student:** Thabiso Khumalo (ST10477675)  
**Repository:** https://github.com/tkhumalo2022/RaceDay_Part2  
**Working branch:** `recovery/part2-20261017` (NOT merged into `main` and NOT submitted on ARC).

This index maps the visually inspected official Part 2 rubric to **real code and checks**. It is not a grade claim. The original student Visual Studio solution remains unrecovered; the work here is a backed-up recovery project.

| Part 2 rubric criterion | Max | Evidence in this branch | Limitation |
| --- | ---: | --- | --- |
| Submission | 5 | This README; setup, branch and CI links | ARC GitHub link not submitted; unlisted presentation URL not yet added |
| GitHub / CI | 10 | `.github/workflows/dotnet-build.yml`; GitHub commits and real green [run #39137366482](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39137366482); [earlier authentic screenshot](evidence/GitHub-CI-Green-20261017.png) | Only assessor can judge which commits qualify as meaningful |
| Authentication and role access | 20 | `AuthApiController.cs`, Identity pages, `SessionIdentity.cs`, `DashboardController.cs`; security tests in `IdentityFlowTests.cs` | Uses Identity cookie + in-memory server-side user/role session, not Part 1's proposed JWT |
| API and database | 25 | `RaceApiController.cs`, `UsersApiController.cs`, `RaceWorkflow.cs`, `ApplicationDbContext.cs`, forward migrations, SQL triggers, `CoreWorkflowTests.cs` and `DatabaseTests.cs`; organiser-owned entries now include participant names | **Strict Part 1 physical schema parity remains unresolved**: separate integer-key Organiser/Participant tables versus Identity string FKs. See [schema reconciliation](PART1_SCHEMA_RECONCILIATION.md) |
| Swagger | 10 | `Program.cs` with XML comments, Swagger UI and CSRF Authorize instructions; `SwaggerDescribesTheWorkflowEndpoints` integration test | Screen-recorded browser demonstration still needed |
| Tests | 15 | Fresh local Release suite: 32 passed, 0 failed on 18 October before participant-name improvement; newest [green CI](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39137366482) includes updated code and enrolment-name assertion | Store/show genuine test output and CI in presentation |
| Video | 15 | [student-led video plan](SUBMISSION_VIDEO_SCRIPT.md) | **No video recorded or uploaded, and no URL is present.** |

## Reviewer quick path

1. Review [README](../README.md), then inspect `src/RaCeDay/Controllers` for API actions and `src/RaCeDay/Services/RaceWorkflow.cs` for shared owner, capacity and validation rules.
2. Inspect `src/RaCeDay/Data/Migrations` for Code-First EF schema and database triggers, and `docs/part1/RaceDay_Database.sql` for the original Part 1 schema. **Do not execute** the Part 1 SQL file on valuable data: it drops `RaceDayDB`.
3. On a Windows computer with SQL Server LocalDB and .NET 10, run `dotnet restore`, `dotnet build RaCeDay.slnx -c Release --warnaserror`, then `dotnet test RaCeDay.slnx -c Release`. Integration tests use new GUID-named databases, not the original database.
4. Run the app using the isolated demonstration-database instructions in README, sign in and open Development `/swagger`. Obtain `/api/auth/antiforgery` CSRF tokens for Swagger writes.
5. Follow the [recording script](SUBMISSION_VIDEO_SCRIPT.md), showing a real Organiser creating an event/category, a Participant enrolling, organiser seeing the participant name, a cross-role 403, results, LocalDB, and CI.

## Required before final submission

- Resolve/document with lecturer whether the **Part 1 ERD/physical SQL schema difference** is acceptable; do not call it identical. Do not perform destructive migration of an existing database.
- Record and upload Thabiso's actual demonstration to YouTube as **Unlisted**, then place the URL in README.
- Capture genuine Swagger/database/screenshots as required by the rubric.
- Review the correct branch and confirm all personal details and assessment instructions; submit the GitHub URL through ARC personally. Do not claim ARC submission until it has been confirmed.
- The desktop may have a local copy at `C:\Users\tkhum\Documents\Codex\2026-10-17\referenced-chatgpt-conversation-this-is-an\outputs\RaceDay-recovery`. Pull from the correct branch when remote access resumes; never overwrite uncommitted local changes.
