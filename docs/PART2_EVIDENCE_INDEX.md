# PROG6212 Part 2 — Marking evidence

**Student:** Thabiso Khumalo (ST10477675)  
**Project:** RaCeDay  
**Repository:** https://github.com/tkhumalo2022/RaceDay_Part2  
**Source branch:** [recovery/part2-20261017](https://github.com/tkhumalo2022/RaceDay_Part2/tree/recovery/part2-20261017)

The table below links the Part 2 assessment areas to source code and test evidence. The marks shown are the rubric's maximum marks, not marks awarded.

| Assessment area | Max | Where to find evidence |
| --- | ---: | --- |
| Submission | 5 | [README](../README.md), [presentation video](https://youtu.be/jytTUheNxYw), source branch |
| GitHub and CI/CD | 10 | [GitHub Actions workflow](../.github/workflows/dotnet-build.yml), commit history, [latest successful run](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39265809171) |
| Authentication and role access | 20 | `src/RaCeDay/Areas/Identity`, `AuthApiController.cs`, `SessionIdentity.cs`, `DashboardController.cs`, `IdentityFlowTests.cs` |
| REST API and database | 25 | `RaceApiController.cs`, `UsersApiController.cs`, `RaceWorkflow.cs`, `ApplicationDbContext.cs`, EF migrations and database tests |
| Swagger | 10 | `Program.cs`, XML API documentation and Swagger-related tests |
| Automated testing | 15 | `tests/RaCeDay.Tests`; CI log dated 19 October 2026: **39 passed, 0 failed, 0 skipped** |
| Video presentation | 15 | [RaCeDay Part 2 presentation](https://youtu.be/jytTUheNxYw) (accessibility and contents to be checked by the student) |

## Verified build and test result

- **Workflow:** Build and test RaCeDay Part 2
- **Run:** [#39265809171](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39265809171)
- **Date:** 19 October 2026
- **Job:** `build-and-test` — successful
- **Tests:** 39 passed, 0 failed, 0 skipped
- **Test report:** `raceday-test-results` artifact in GitHub Actions
- **Source commit tested:** `41b1651a96e9062d82a1c0abf8f08af920c7334e`
- **Earlier genuine screenshot:** [GitHub Actions evidence](evidence/GitHub-CI-Green-20261018.png)

Additional documentation-only changes should have their own passing workflow run before final marking.

## Code review path

1. Open [README](../README.md) for requirements and setup instructions.
2. Inspect `src/RaCeDay/Controllers`, `src/RaCeDay/Services/RaceWorkflow.cs` and `src/RaCeDay/Data/ApplicationDbContext.cs`.
3. Review `src/RaCeDay/Data/Migrations` and the tests under `tests/RaCeDay.Tests`.
4. Run `dotnet build RaCeDay.slnx -c Release --warnaserror` and `dotnet test RaCeDay.slnx -c Release` on Windows with .NET 10 and SQL Server LocalDB.
5. In Development, open `/swagger` and test the documented endpoints. A CSRF token is required for write requests.

## Important technical limitations

- **Part 1 database schema:** The current Part 2 database uses ASP.NET Identity users and roles instead of the separate integer-key Organiser and Participant tables used in Part 1. This is a physical schema difference and has not been approved by the lecturer. See [database comparison](PART1_PART2_DB_COMPARISON_20261019.md).
- **Login protocol:** The application uses Identity cookies and server-side session state, rather than the JWT approach outlined in the Part 1 API plan.
- **Payments:** Payment-attempt records do not represent completed gateway transactions.
- **Project origin:** The uploaded work is a separate Part 2 implementation. It has not been established as an exact copy of the original local Visual Studio solution.
- **Submission:** An accessible GitHub repository or YouTube link does not confirm submission to ARC.

The final mark and acceptance of the Part 1 schema change are the lecturer's decisions.
