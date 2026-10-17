# RaceDay Part 2 — development status (17 October 2026)

**Repository:** isolated recovery working copy, branch `recovery/part2-20261017`. This is **not** the previously lost original student application. Original starter copies are preserved separately. The official ARC POE Part 2 rubric has not been read (the local DOCX is encrypted and ARC browser access was not available). No marks percentage can be justified.

## Verified locally

- `.NET 10` Release build with `--warnaserror`: **0 warnings, 0 errors**.
- SQL Server LocalDB integration suite: **24 passed, 0 failed, 0 skipped**. TRX: `tests/RaCeDay.Tests/TestResults/RaceDay-Part2-final.trx`.
- Complete EF Core migration chain on **isolated test databases**: `InitialIdentity`, `RaceWorkflowCore`, `PaymentAttempts`, `PerCategoryEnrolmentUniqueness`. Original application databases were not migrated.
- Identity: register/login, Admin/Organiser/Participant roles, role seeding, dashboard access restrictions, profile updates, CSRF protection and anonymous denial.
- MVC pages plus REST API and Swagger for Events/Categories/Enrolments/Results and authenticated GET/PUT `/api/users/me`. Organiser ownership and participant privacy tested.
- Enrollment capacities and deadline checks; concurrent last-place requests tested; same participant can enter *different* categories but not the same category twice, matching the accessible Part 1 plan.
- Payment-attempt REST and MVC with amount equality, limited role access, unique reference and pending-only status. Database filtered index prohibits more than one Paid payment per enrolment; tests check the rule. This is **not a real gateway** and does not confirm funds.
- Generic production error page to avoid a broken `/Home/Error` handler.

## Missing or not yet independently verified

- Original source recovery and integration.
- Official marking rubric, submission instructions and marks.
- Part 1 described JWT-based `POST /api/auth/register` and `POST /api/auth/login`, while this recovery build currently uses ASP.NET Identity cookie authentication; rubric decision needed.
- Production email sender, confirmation/recovery, security review and release deployment.
- Payment processing and settlement webhooks (only mock Pending records exist).
- Real-browser screenshots, Swagger/manual walkthrough, SSMS screenshots and evidence-to-rubric mapping.
- CI/remote branch verification for this new work.

## Safety and delivery

- The original project remains unchanged; earlier backed-up work is in `outputs/RaceDay-local-backup`, and a further `outputs/RaceDay-pre-payments-backup` snapshot was created before these changes.
- No course submission, GitHub push or merge is authorised/claimed.
- See `README.md` for **isolated database** setup, `docs/SUBMISSION_EVIDENCE_CHECKLIST.md` for required demonstration evidence, and `docs/PAYMENT_WORKFLOW.md` for limitations.

## Next steps

1. Get the official rubric and compare it to the implementation.
2. Verify the app manually in a browser using a new test database, capture screenshots and SQL/Swagger evidence.
3. Address confirmed missing rubric functionality, particularly API auth design and any additional fields.
4. Confirm any final local commits and only push/submit after approval.
