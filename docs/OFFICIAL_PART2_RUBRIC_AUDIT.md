# RaCeDay PROG6212 — official Part 2 rubric audit (17 October 2026)

**Source:** Lecturer's 2026 `PROG6212_POE.docx` in the authenticated IIE SharePoint Word Online tab on the student's laptop. The document is protected against Word Online accessibility-text extraction, but Part 2 pages 9–11 and Part 2 marking rubrics on pages 17–18 were visually inspected. Screenshots are kept outside the Git repo under `outputs/RaceDay-evidence-20261017/ARC-detail-*.png` for private reference; do not publish the lecturer's copyrighted document.

**Scope:** Part 2, *RESTful API Development* (100 marks). Part 3's Docker/MVC requirements are separate, not Part 2's compulsory deliverables.

| Official Part 2 marking criterion | Max marks | Verified in isolated local recovery branch | Unmet or not verified |
| --- | ---: | --- | --- |
| Correct submission | 5 | README and setup commands exist | No official GitHub link submitted, no CI screenshot or unlisted video URL |
| GitHub and CI/CD | 10 | Git repository exists; Windows LocalDB build/test workflow prepared | Minimum **20 meaningful Part 2 commits** unproven; revised CI has not run on GitHub; green CI screenshot absent |
| Authentication and role-based access | 20 | Organiser and Participant selectable via MVC, API register/login/logout now exist, Identity hashes passwords, cookie authentication, server-side user-ID/role session and protected roles tested | Identity-authorisation and session storage now locally tested; confirm browser evidence for all protected API routes |
| API endpoints and database | 25 | ASP.NET Core + SQL Server EF Core migrations; events, categories, enrolment, results, own profile and pending-only payment attempts | **Must reconcile EF schema with approved Part 1 ERD and SQL exactly.** Current EF Identity schema differs from Part 1's separate organiser and participant tables. Cookie session differs from JWT mentioned in the student's Part 1 endpoint plan |
| Swagger integration | 10 | Live Swagger UI and JSON work; endpoints visible | Demonstrate each endpoint in same browser, ensure useful operation descriptions and request/response schemas |
| Unit testing | 15 | 32 locally passing automated tests, including SQL-trigger checks and role-specific registration | Must pass in a real GitHub Actions run; green CI screenshot absent |
| Video presentation | 15 | None | Unlisted YouTube walkthrough demonstrating code, endpoints, auth, Swagger, unit tests; README link |
| **Total** | **100** | **No estimated grade** | |

**Official requirements on pages 9–11**: Build ASP.NET Core Web API with Code-First SQL Server EF Core, matching Part 1 ERD/SQL; register as Organiser or Participant; hashed passwords; login and session maintaining user/role; unauthenticated and cross-role access denied; endpoint functionality for auth, own profile, organiser event CRUD and categories, participant enrolment and organiser view, organiser results capture and participant results view; all endpoints visible/testable/described in browser Swagger; tests for success and failure in login, event management, role permissions and enrolment; GitHub Actions builds and runs tests on every push; 20 meaningful commits for this part; green CI screenshot and README, unlisted YouTube demo, ARC GitHub link.

**Important caveats**
- Official rubric places special emphasis on *Part 1 schema parity*. EF Identity replacing separate `Organiser`/`Participant` tables is an architectural deviation that may lose marks unless reconciled or accepted by the lecturer.
- The Part 1 API plan called for JWT while the Part 2 rubric describes session management; current implementation deliberately uses Identity cookie-based auth. Document this contract change rather than pretend JWT is implemented.
- This is the recovery branch, not the still-unlocated original student Visual Studio solution. No merge or submission without approval.
- Part 3 asks for Docker, themed MVC UI and more; **do not treat those as Part 2 grading requirements**.

## Priorities by assessed impact

1. Reconcile **25-mark schema/endpoints** against the approved Part 1 ERD and SQL, explain or correct deviations, and demonstrate all routes in Swagger.
2. Make CI **actually run** on GitHub with passing SQL Server integration tests and preserve green screenshot; meet 20 authentic commits, never create filler commits.
3. Obtain a legitimate full application demo/video, add unlisted YouTube URL and README evidence (15 marks).
4. Capture new screenshot evidence for session+role auth and the API; the latest isolated HTTP flow passed 21 checks including server-side session reads, and Swagger exposed 18 API paths.
