# RaCeDay PROG6212 Part 2 — submission evidence checklist

This is a work-in-progress evidence checklist, **not the official marking rubric**. Obtain and read the ARC brief before naming a final grade or submitting.

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

- [ ] Participant registration and login (positive and invalid password).
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
- [ ] ARC PROG6212 Part 2 rubric readable and item-by-item mapped.
- [ ] Confirm whether JWT auth API, actual gateway settlement, statistics, profile pictures and additional field requirements are compulsory.
- [ ] End-to-end testing against a lecturer-approved demo environment.
- [ ] Submission package structure, references (if any), academic integrity and deadline reviewed.

**Status language:** planned / coded / locally built / automatically tested / manually browser-verified / committed / pushed / submitted must remain distinct. Do not claim unperformed steps.
