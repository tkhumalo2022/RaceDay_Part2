# PROG6212 Part 2 — Final requirements check

**Student:** Thabiso Khumalo
**Project:** RaCeDay
**Branch:** `recovery/part2-20261017`
**Total marks:** 100
**Source:** official Part 2 brief and marking rubric, pages 9–11 and 17–18, reviewed on ARC. The course videos are useful examples, but **Part 3 tutorials are not Part 2 requirements**.

Use this sheet before recording and again before uploading to ARC. Tick a step only after you've **seen it work**. The marks are the maximum for each section, **not marks already earned**.

## 1. Submission — 5 marks

- [x] GitHub repository exists: https://github.com/tkhumalo2022/RaceDay_Part2
- [x] README explains the application, requirements and how to build it.
- [x] Genuine CI screenshot is in `docs/evidence/GitHub-CI-Green-20261018.png`.
- [ ] Add the real Unlisted YouTube presentation URL to README.
- [ ] Check what ARC asks you to submit, submit the **correct** GitHub link and verify the confirmation screen.
- [ ] Decide whether to merge the tested recovery branch to `main`. **Nothing has been merged or submitted**.

## 2. GitHub and CI/CD — 10 marks

- [x] Source code is pushed on `recovery/part2-20261017`.
- [x] `.github/workflows/dotnet-build.yml` builds and runs the tests on push and pull request.
- [x] Real Windows CI run: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39147081224 — **success**, with test and build steps passing.
- [ ] Check whether at least **20 commits contain meaningful Part 2 work**, not just renamed files or filler commits. Git history count alone is not proof.
- [ ] Capture an up-to-date genuine CI screenshot after the final code changes.
- [ ] Make sure the branch/link ARC receives contains all the Part 2 work.

## 3. Authentication and role access — 20 marks

- [x] Register **Organiser** and **Participant** through Identity; password hashing, role assignment and validation exist.
- [x] Public registration does not permit **Admin**.
- [x] Login/logout and a signed-in session storing user ID and role are implemented.
- [x] MVC pages and API routes are protected; unit/integration tests cover role restrictions.
- [ ] Film a successful Organiser login, Participant login and logout.
- [ ] Film denied access: anonymous API request **401**, Participant trying to change an event **403**.
- [ ] Show `GET /api/auth/session`, with the user ID and correct role.
- [ ] Check credentials on a fresh browser session before recording.

## 4. REST API and Microsoft SQL Server — 25 marks

- [x] ASP.NET Core REST controllers cover auth, user profile, event and category CRUD, enrolments, result recording and own results.
- [x] SQL Server 2022 default instance `localhost`, with a separate database `RaCeDayPart2_SQLServer_Demo_20261018`.
- [x] Six Code-First EF Core migrations and three seeded roles verified.
- [x] Category-participant and payment-amount SQL triggers verified.
- [ ] **BLOCKING FOR TOP MARKS:** reconcile the **physical** Part 1 ERD/SQL schema. Original `RaceDayDB` uses separate integer-key `Organiser` and `Participant` tables; Part 2 has Identity string-key `AspNetUsers`. Table/column names and foreign keys differ. The rubric's strongest band expects a match. Do not describe them as identical.
- [ ] Ask the lecturer whether Identity-based tables are accepted as an evolution of Part 1, **or** implement a forward migration to a **new isolated test database**, with backups and tests. Never run the destructive Part 1 SQL script against `RaceDayDB`.
- [ ] Register two local demonstration users, create a future event, create two categories, enrol a participant, close the event, record a result and show the stored rows in SSMS.
- [ ] Film event create/edit/delete, capacity/duplicate restrictions, and ownership checks.
- [ ] Explain that payment attempts remain **Pending**: there is no confirmed real-payment processing.

## 5. Swagger — 10 marks

- [x] Swagger UI and JSON run at `http://127.0.0.1:5236/swagger`; the API currently exposes **18 paths**.
- [x] XML API comments and request/response DTOs exist.
- [ ] Demonstrate `GET /api/auth/antiforgery`; copy the returned `requestToken` to Swagger **Authorize** as `X-CSRF-TOKEN`. Refresh after changing sign-in.
- [ ] Show Organiser and Participant endpoints returning useful data.
- [ ] Show at least one invalid request and the HTTP status/error response.

## 6. Automated tests — 15 marks

- [x] Release build completed with **0 warnings and 0 errors**.
- [x] Latest local test run: **35 passed, 0 failed** (including safe profile picture and Admin dashboard tests).
- [x] Windows GitHub CI run succeeded on the updated code.
- [ ] Capture a real terminal run of `dotnet build RaCeDay.slnx -c Release --warnaserror` and `dotnet test RaCeDay.slnx -c Release` for your presentation.
- [ ] Briefly explain a valid test and an invalid test for login, event management, enrolments and access permissions.

## 7. Video presentation — 15 marks

- [ ] Record the actual running application with your voice: explain the C# controllers, DbContext, migration, Identity, roles, Swagger, SQL Server, one full race workflow, tests and GitHub Actions.
- [ ] Use `docs/SUBMISSION_VIDEO_SCRIPT.md` as a guide. Keep the language clear and your own; speak while demonstrating the screen.
- [ ] Upload as **Unlisted** on YouTube and test the link in a signed-out browser.
- [ ] Add that genuine URL to README. Don't claim the video is submitted until ARC confirms.

## Suggested recording order

1. Home page and what RaCeDay does
2. Program.cs, API controllers, RaceWorkflow and DbContext
3. SQL Server tables/migrations, explain Part 1 mapping
4. Organiser registration/login/session and dashboard
5. Organiser creates an event and categories
6. Participant login, enrolment and duplicate/full check
7. Organiser lists entries and saves a result
8. Participant views their own result
9. Swagger paths, CSRF authorisation and denied access
10. Run test suite, show GitHub Actions success and README

## Question to ask the lecturer about the Part 1 database

Good day,

I'm finishing my PROG6212 Part 2 RaCeDay application. I used ASP.NET Core Identity for registration and login, so Part 2 stores user accounts in AspNetUsers with string keys. My Part 1 SQL script used separate Organiser and Participant tables with integer keys.

The event, category, enrolment, payment and results functions are in place, but the physical relationships are different. Does Part 2 need to use exactly the same Part 1 table and foreign-key structure, or is the Identity-based change acceptable if I explain and document it?

I want to confirm this before I submit.

Kind regards,
Thabiso Khumalo

---
## Already-open tools

- App: http://127.0.0.1:5236
- Registration: http://127.0.0.1:5236/Identity/Account/Register
- Swagger: http://127.0.0.1:5236/swagger
- GitHub draft PR: https://github.com/tkhumalo2022/RaceDay_Part2/pull/2
- Passing CI: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39147081224
- SSMS: server `localhost`, Windows Authentication, database `RaCeDayPart2_SQLServer_Demo_20261018`
- Run a read-only SQL check using `RaCeDay_Part2_SQLServer_Demo_ReadOnly.sql` in the neighbouring outputs folder.
- Check actual state using `scripts/part2-preflight.ps1`.

## What is still in your hands

You need to confirm the permitted Part 1 schema change with your lecturer, complete the real video, upload it as Unlisted, approve the final code/branch and submit it on ARC. All of those matter to the mark. I cannot honestly guarantee 100%.
