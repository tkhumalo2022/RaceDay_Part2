# RaCeDay PROG6212 Part 2 — submission evidence checklist

This is an evidence checklist mapped to the visually inspected official 100-mark Part 2 rubric, not a grade estimate. See `docs/OFFICIAL_PART2_RUBRIC_AUDIT.md` for the exact marking categories.

## Tests and source control

- [ ] Save release build output with 0 errors and warnings (include date, command, SDK version).
- [ ] Save full automated test run with number passed/failed/skipped and its `.trx` output.
- [ ] Capture `git status`, meaningful commit history and remote branch status.
- [ ] Confirm no personal secrets, connection passwords, synthetic accounts or private student documents are in the Git commit.

## SQL Server / EF Core

- [ ] Create **new** LocalDB demonstration database and apply the reviewed migration chain: InitialIdentity, RaceWorkflowCore, PaymentAttempts, PerCategoryEnrolmentUniqueness.
- [ ] Capture EF migration history, Identity roles, Event, Race_Category, Enrolment, Race_Result and Payment tables from SSMS/SQL.
- [ ] Demonstrate category capacity, foreign keys, participant/category uniqueness and paid-payment uniqueness.
- [ ] Never run the destructive Part 1 reset script against a valuable database.

## Website and Swagger walkthrough

- [x] Organiser **and** Participant registration/login verified by automated tests and a 21-check synthetic-account HTTP walkthrough (invalid password and blocked Admin signup covered in automated tests).
- [x] Server-side user-ID/role session mirrors the Identity authentication and clears on logout, verified in tests. Production-grade distributed session infrastructure remains out of scope.
- [ ] Protected Admin/Organiser/Participant dashboard and forbidden cross-role access.
- [ ] GET/PUT `/api/users/me` reads and updates the current profile without changing roles, email or another account.
- [ ] Organiser creates/edits race and 2 categories; another organiser cannot edit them.
- [ ] Participant browses event and enrols in two *different* categories; a repeated entry for the *same* category is rejected.
- [ ] Show capacity full and registration closing restrictions.
- [ ] Organiser closes event and records results; Participant sees only own result.
- [ ] Participant records a Pending payment attempt for the exact entry fee; wrong fee rejected, other user denied, and untrusted client cannot mark Paid.
- [ ] Swagger `/swagger` open, sign in with cookie, obtain CSRF token and demonstrate relevant endpoints.
- [ ] Capture useful screenshots with the URL/window visible and captions.

## Unverified / not ready

- [ ] Original student Visual Studio solution recovered and compared.
- [x] Official Part 2 text/rubric visually inspected in the open SharePoint Word Online tab (pages 9–11, 17–18) and locally mapped; original copyrighted assessment not copied into Git.
- [ ] Reconcile Part 1 schema (especially separate Organiser/Participant entities and event fields) against EF Identity model; document cookie-session versus Part 1 JWT plan. Part 3-only features such as Docker and themed MVC UI are not assessed in Part 2.
- [ ] End-to-end testing against a lecturer-approved demo environment.
- [ ] Submission package structure, references (if any), academic integrity and deadline reviewed.

**Status language:** planned / coded / locally built / automatically tested / manually browser-verified / committed / pushed / submitted must remain distinct. Do not claim unperformed steps.
