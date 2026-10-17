# RaCeDay Part 2 — verified development status (17 October 2026)

The original student Visual Studio application is still missing. This is an **isolated recovery branch**, `recovery/part2-20261017`, not a restoration or submission of the original application.

The official IIE 2026 PROG6212 POE Part 2 instructions were **visually inspected** in the open authenticated SharePoint Word Online tab. Relevant pages: Part 2 brief 9–11, marking rubric 17–18. See `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md` for the **100-mark allocation**. This document is protected, so visual access does not make it a normal downloadable/plaintext document.

## Locally implemented and verified

- `.NET 10` ASP.NET Core Identity MVC and API, SQL Server EF Core Code-First migrations.
- MVC public registration chooses Organiser or Participant; API has CSRF-protected JSON register/login/logout, and anonymous CSRF token endpoint. Admin self-registration is rejected; Identity hashes passwords; cookie session retains role claims.
- Role-protected Admin/Organiser/Participant dashboards, profile editing + GET/PUT API, restricted Event/Category/Enrolment/Result workflows, pending-only Payment attempts and Swagger UI.
- SQL Server LocalDB unit/integration tests cover core positive/negative cases, concurrency and role ownership. **26 tests passed** after adding API auth; Release compilation had **0 errors and 0 warnings**. CI verification has not occurred.
- New Windows GitHub Actions workflow configured to build **and run LocalDB tests** on pushes/PRs; this is only a local change, not a green CI run.
- Previous manual runtime smoke test demonstrated HTTP 200 home/login/Swagger and HTTP 401 for anonymous API access. That test predates the latest auth endpoints and must be repeated after commit.

## Confirmed official Part 2 gaps

1. The **25-mark API/database** category requires EF schema to match the approved Part 1 ERD and SQL script. Existing Identity-based model differs materially from the Part 1 separate Organiser/Participant tables, and must be reconciled or documented with lecturer approval.
2. The **10-mark GitHub/CI** criterion asks for **20 meaningful Part 2 commits**, a successful GitHub Actions build/test on every push, and a green screenshot. Local commit count is not yet enough, and the revised workflow hasn't run remotely. Do not create filler commits.
3. The **15-mark video** criterion requires a real unlisted YouTube demonstration of all endpoints, authentication, Swagger and tests, with the link in README. No video exists yet.
4. Swagger endpoint descriptions and a complete browser-authenticated API demonstration still need rubric-aligned verification. MVC browser screens and local integration tests don't substitute for the live API demonstration.
5. Confirm how the assessor interprets *server-side session*, because ASP.NET Identity encrypts role claims in an authentication cookie, while the highest rubric band literally says server-side session stores user ID and role.
6. Get the original student project if accessible; no other student's source should be imported.

## Explicit scope decisions

- Part 2 requires a REST API; Docker containerisation and a fully themed MVC interface belong primarily to **Part 3**, not Part 2.
- Part 1's endpoint plan suggested JWT; the official Part 2 brief asks for session authentication. This recovery implementation uses cookie-based Identity; document the difference.
- Payment attempts are Pending records only, not a payment gateway and not real settlement.
- Original application/database unchanged. No GitHub push, merge, deployment, ARC submission or marks claim has been made.

## Local evidence

- Latest local test output: 26 passed, 0 failed, 0 skipped (rerun/preserve TRX before final submission).
- Earlier screenshot and manual check folder: `outputs/RaceDay-evidence-20261017`, outside the Git repository.
- See `docs/SUBMISSION_EVIDENCE_CHECKLIST.md`, `README.md` and the official audit for outstanding steps.
