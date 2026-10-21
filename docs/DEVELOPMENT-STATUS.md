# PROG6212 Part 2 — RaCeDay recovery project status

**Student:** Thabiso Khumalo
**Date:** 17 October 2026
**Project:** Isolated recovery branch `recovery/part2-20261017` of `tkhumalo2022/RaceDay_Part2`. The student's previously lost original Visual Studio solution remains unrecovered; this is a separate working copy.

The official IIE `PROG6212_POE.docx` was reviewed visually in the logged-in SharePoint tab. Part 2 (RESTful API Development) is **100 marks**, pages 9–11 and 17–18. See `OFFICIAL_PART2_RUBRIC_AUDIT.md`.

## Verified locally

- Windows .NET 10 Release build: **0 errors, 0 warnings**, `--warnaserror`.
- SQL Server LocalDB test suite: **32 passed, 0 failed, 0 skipped**, including API Identity role access, concurrency, event/category/enrolment/result workflow, profile updating, pending payment attempts, category-count SQL trigger, payment amount SQL trigger and role-specific registration.
- Test report: `tests/RaCeDay.Tests/TestResults/RaceDay-Session-20261017.trx` (generated locally; ignored by Git).
- MVC and JSON API register Organiser or Participant, never Admin. Participant DOB and Organiser name required for new registrations; phone/details stored with Identity profile. Profile API and form allow role-appropriate edits.
- ASP.NET Identity stores hashed passwords and enforces role authorization with secure authentication cookies. The application also stores the current user ID and role in a memory-backed server-side session on login/registration; logout clears it. Request tokens protect API writes. This is single-instance session storage, not a distributed production session store.
- Event/category management, capacity/date restrictions, private enrolments/results, pending-only payment attempt records, Swagger XML summaries.
- Six forward EF migrations, including `Part1CategoryCountAndPaymentRules` and `RoleRegistrationProfileFields`; original database untouched. Tests create separate GUID-named SQL Server LocalDB databases.
- `CurrentParticipants` is a database column maintained by a trigger; payments have a database-level amount-equality trigger and unique Paid safeguard.
- Windows GitHub Actions workflow runs on **every push** and verified remotely green on [run #39068333340](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/39068333340): SQL Server LocalDB integration suite **32 passed, 0 failed**, compiler build successful. Real screenshot checked in at `docs/evidence/GitHub-CI-Green-20261017.png`; TRX test artifact available from GitHub.

## Unfinished / assessment risks

- Exact Part 1 **physical** schema alignment is not complete. Part 1 defines separate integer-key `Organiser` and `Participant` tables; Identity uses shared `AspNetUsers` and string FKs. Added role-specific fields and SQL triggers improve parity, but a full redesign/lecturer approval is needed before claiming an exact match. See `PART1_SCHEMA_RECONCILIATION.md` and `SCHEMA_PARITY_ADDENDUM.md`.
- The approved recovery branch is now on GitHub. An initial CI run exposed a locale-dependent HTML assertion (31/32), fixed in commit `7611b47`; the second run **passed all 32**. Screenshot and real run URL retained. The 23 commits present at that run include substantive implementation/test/docs; only the assessor can decide if the 20-meaningful-commits rubric is fully met.
- No real recorded and uploaded unlisted YouTube presentation. A student-led recording checklist exists at `SUBMISSION_VIDEO_SCRIPT.md`.
- A full role-based HTTP walkthrough has been performed against a separate demonstration database: **21 expected status-code checks**, including server-side session reads for both roles, registration, owned events/categories, duplicate rejection, forbidden role action, pending-only payment, result recording and Swagger's **18 paths**. Evidence log: `outputs/RaceDay-Part2-LiveEvidence-20261017/verified-http-log.txt`, outside the repo. Full screen-recorded Swagger walkthrough and screenshots still remain outstanding.
- Original Visual Studio project is not merged, production email confirmation and a real payments gateway are unimplemented; the latter is not an explicit Part 2 functional requirement.
- The recovery branch was pushed to GitHub. There is **no merge into main**, production deployment, YouTube URL, ARC submission or defensible grade prediction.

## Submission route

1. Review legacy schema mismatch with lecturer, or carry out a separate migration redesign against an **isolated** database only.
2. [x] Push the authorised recovery branch, verify GitHub Actions, and capture a genuine green run; [ ] confirm any remaining screenshot/README requirements with the lecturer.
3. Create the student-led real video; make it Unlisted and paste the link into README.
4. Record a Swagger demonstration, collect database/commit evidence, ensure the 20 meaningful commits are authentic.
5. Check the resulting repo link and submit it yourself on ARC.
