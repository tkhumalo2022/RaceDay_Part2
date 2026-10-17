# PROG6212 Part 2 — RaCeDay recovery project status

**Student:** Thabiso Khumalo
**Date:** 17 October 2026
**Project:** Isolated recovery branch `recovery/part2-20261017` of `tkhumalo2022/RaceDay_Part2`. The student's previously lost original Visual Studio solution remains unrecovered; this is a separate working copy.

The official IIE `PROG6212_POE.docx` was reviewed visually in the logged-in SharePoint tab. Part 2 (RESTful API Development) is **100 marks**, pages 9–11 and 17–18. See `OFFICIAL_PART2_RUBRIC_AUDIT.md`.

## Verified locally

- Windows .NET 10 Release build: **0 errors, 0 warnings**, `--warnaserror`.
- SQL Server LocalDB test suite: **31 passed, 0 failed, 0 skipped**, including API Identity role access, concurrency, event/category/enrolment/result workflow, profile updating, pending payment attempts, category-count SQL trigger, payment amount SQL trigger and role-specific registration.
- Test report: `tests/RaCeDay.Tests/TestResults/Part2-AdditionalRegression.trx` (generated locally; ignored by Git).
- MVC and JSON API register Organiser or Participant, never Admin. Participant DOB and Organiser name required for new registrations; phone/details stored with Identity profile. Profile API and form allow role-appropriate edits.
- ASP.NET Identity stores hashed passwords, issues secure authentication cookies and restricts endpoints by role. Request tokens protect API writes.
- Event/category management, capacity/date restrictions, private enrolments/results, pending-only payment attempt records, Swagger XML summaries.
- Six forward EF migrations, including `Part1CategoryCountAndPaymentRules` and `RoleRegistrationProfileFields`; original database untouched. Tests create separate GUID-named SQL Server LocalDB databases.
- `CurrentParticipants` is a database column maintained by a trigger; payments have a database-level amount-equality trigger and unique Paid safeguard.
- Windows GitHub Actions workflow configured for **every push** with .NET build, SQL Server LocalDB tests and downloadable TRX test artifact. **This is local configuration only; no remote green build verified.**

## Unfinished / assessment risks

- Exact Part 1 **physical** schema alignment is not complete. Part 1 defines separate integer-key `Organiser` and `Participant` tables; Identity uses shared `AspNetUsers` and string FKs. Added role-specific fields and SQL triggers improve parity, but a full redesign/lecturer approval is needed before claiming an exact match. See `PART1_SCHEMA_RECONCILIATION.md` and `SCHEMA_PARITY_ADDENDUM.md`.
- No live GitHub Actions verification or green screenshot. The local history now reaches 20 commits including genuine feature, test, CI and documentation changes; only the assessor can judge whether all are meaningful. Recovery branch remains local pending approval to push. Never fabricate commits.
- No real recorded and uploaded unlisted YouTube presentation. A student-led recording checklist exists at `SUBMISSION_VIDEO_SCRIPT.md`.
- Full manual authenticated Swagger walkthrough after latest code change and rubric evidence screenshots not yet completed. Earlier browser screenshots are in `outputs/RaceDay-evidence-20261017` outside the repository.
- Original Visual Studio project is not merged, production email confirmation and a real payments gateway are unimplemented; the latter is not an explicit Part 2 functional requirement.
- There is no GitHub push, merge, deployment, YouTube URL, ARC submission or grade calculation.

## Submission route

1. Review legacy schema mismatch with lecturer, or carry out a separate migration redesign against an **isolated** database only.
2. Confirm permission to push this recovery branch, verify GitHub Actions and capture the actual green run.
3. Create the student-led real video; make it Unlisted and paste the link into README.
4. Record a Swagger demonstration, collect database/commit evidence, ensure the 20 meaningful commits are authentic.
5. Check the resulting repo link and submit it yourself on ARC.
