# PROG6212 RaceDay Part 2 — Coverage & Recovery Audit

**Student repository:** [RaceDay_Part2](https://github.com/tkhumalo2022/RaceDay_Part2)  
**Baseline:** [Part 1 API endpoint plan](https://github.com/tkhumalo2022/PROG6212-Part-1-RaceDay-Final/blob/main/docs/RaceDay_API_Endpoint_Plan.md)  
**Working branch:** `feat/identity-foundation`  
**Audit date:** 17 October 2026

> This audit compares the *new foundation branch* against the student's Part 1 plan. It is **not** a statement that the original, still-unlocated Visual Studio project is missing these features. The lecturer's official Part 2 assessment/rubric document has **not** been recovered, so do not treat these priorities as an official marks breakdown.

## Current verified evidence

- [x] Fresh GitHub Part 2 repository created on `tkhumalo2022`.
- [x] Draft PR #1 preserves the Identity foundation separately from `main`.
- [x] .NET 10 ASP.NET Core MVC and Identity foundation committed.
- [x] GitHub Actions restore and Release build succeeded: [Run #39044458229](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39044458229).
- [x] `ApplicationUser`, `IdentityDbContext<ApplicationUser>`, role seeding code and registration/login pages included.
- [ ] Original RaCeDay Visual Studio solution recovered and compared.
- [ ] EF Core SQL Server migration generated, applied and verified in SSMS.
- [ ] Account registration/login/logout and user role assignment demonstrated against a real DB.
- [ ] Official Part 2 marking rubric checked item by item.
- [ ] Core race workflows, API, tests and submission evidence verified.

## Endpoint coverage against the student's Part 1 plan

Legend: **Foundation** means some related infrastructure is committed, **Not implemented in new branch** means no controller/endpoint exists in the foundation, and **Not runtime tested** means code is present but not yet verified end to end.

| Area | Part 1 planned endpoints | New foundation status | What counts as proof |
| --- | --- | --- | --- |
| Authentication | POST `/api/auth/register`, POST `/api/auth/login` | **Not implemented** as API; cookie-based Razor registration/login pages exist, not runtime tested | Correct status codes, security, test registrations, rejection of invalid credentials |
| Profile | GET, PUT `/api/users/me` | **Not implemented in new branch** | Own profile read/update, isolation between users |
| Events | GET `/api/events`, GET/PUT/DELETE `/api/events/{id}`, POST `/api/events` | **Not implemented in new branch** | Organiser CRUD, participant browse, ownership checks |
| Categories | GET/POST `/api/events/{eventId}/categories`, PUT/DELETE `/api/categories/{id}` | **Not implemented in new branch** | Validation, organiser ownership, category/event consistency |
| Enrolments | POST `/api/events/{eventId}/enrolments`, GET `/api/enrolments/me`, GET `/api/events/{eventId}/enrolments` | **Not implemented in new branch** | Participant enrol, prevent duplicate entries, role/ownership check |
| Results | POST `/api/enrolments/{enrolmentId}/results`, GET `/api/results/me`, GET `/api/events/{eventId}/results` | **Not implemented in new branch** | Organiser capture; participant sees own results |
| Payments | POST/GET `/api/enrolments/{enrolmentId}/payments` | **Not implemented in new branch** | Correct amount, enrolment ownership, reject duplicate paid transactions |

**Total:** 21 planned routes in the student's Part 1 design. If the lecturer's official Part 2 instructions diverge, follow the **official brief** and document any change from the Part 1 plan.

## Recommended order to protect marks

1. **Recover before replacing.** Find the original `.sln`/`.csproj` or a ZIP. Inspect current code, DB context, migration history and Git remotes. Work on a new integration branch with a backup; do **not** copy a third party's coursework or force-push.
2. **Read the original Part 2 brief.** Obtain the actual ARC `PROG6212_POE.docx` and extract each rubric row and official mark weight. Focus on assessed functionality, not extra design.
3. **Make the database work.** Confirm SQL Server connection, entity models, Identity schema, migrations and original RaceDay tables. Avoid destructive migrations or overwriting the Part 1 database.
4. **Secure authentication.** Ensure every Identity service uses the same user type, roles are seeded, unauthorised routes fail correctly and public registration cannot create Admin accounts. Keep the API authentication strategy consistent with the assessed requirement.
5. **Complete one functional path at a time.** Organiser creates an event/category → Participant enrols → Organiser records a result → Participant views it. Implement and test each planned API route; add payments only as specified.
6. **Prove the work.** Unit/integration tests, Swagger/endpoint screenshots, database evidence, meaningful Git commits, green CI, README and walkthrough. Never claim a test was run unless it was.
7. **Review and merge.** Reconcile original solution and foundation; verify the integrated build and regression tests before merging PR #1 or any successor PR.

## Source recovery status

- The local device previously returned no accessible `RaCeDay.sln` or `RaCeDay.csproj` in the main user folders.
- Connected Google Drive searches for `RaCeDay`, `RaceDay` and `PROG6212` returned no matching original project.
- The original project may be on a different device/account or not yet pushed. Do not infer ownership from similarly named public GitHub repositories.
- Laptop Remote Desktop Commander reached its usage limit. The uncommitted local starter copy is not verified or identical to this GitHub branch.
- ARC lecturer `Pages.zip` (Identity scaffolding) was downloaded earlier to the laptop but was used for reference only; do not publish its contents as original work.

## Verification log

| Evidence | Result | Next step |
| --- | --- | --- |
| CI restore and compile | **PASS** on feature branch | Keep green after integration |
| EF migration + SQL Server | **NOT TESTED** | Generate and inspect migration, run on isolated DB |
| Login, register, role assignment | **NOT TESTED** | Exercise success and failure paths |
| API / Swagger endpoint coverage | **NOT IMPLEMENTED on this branch** | Compare original app before adding new code |
| Complete POE rubric | **NOT AVAILABLE** | Retrieve source document from ARC |

**Important:** This is a recovery and coverage checklist; it does not justify a claimed percentage or grade.
