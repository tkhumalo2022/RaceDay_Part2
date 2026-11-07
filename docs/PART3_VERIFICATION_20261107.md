# RaceDay Part 3 — submission-readiness verification (7 November 2026)

**Status: NOT YET SUBMITTED.** This document records observed evidence and remaining manual sign-offs for PROG6212 POE Part 3. It is not a mark prediction or proof of upload to ARC. The Monday 9 November at 23:59 deadline is from the previously tracked timetable; confirm the official ARC deadline and the current lecturer rubric.

## Verification carried out 7 November 2026
- Authorised Windows laptop: Thabiso; working folder `RaceDay_PROG6212_Part3_Development`.
- Reviewed existing branch `feature/part3-mvc-review`, prior source commit `f0253380eda2cf134551e7e526336d946a6511ee` and open draft PR #3: https://github.com/tkhumalo2022/RaceDay_Part2/pull/3
- `git rev-list --count main..HEAD`: **22 commits ahead of main**. Individual commit messages describe actual features/tests/docs, but rubric-based meaningfulness requires lecturer review.
- Ran `dotnet test RaCeDay.slnx --configuration Debug --no-restore --verbosity quiet`: **58 passed, 0 failed, 0 skipped**. Command exited with code 0.
- Ran `dotnet build RaCeDay.slnx --configuration Debug --no-restore --warnaserror --verbosity quiet`: **build succeeded, 0 warnings, 0 errors**. Command exited with code 0.
- Ran `powershell -ExecutionPolicy Bypass -File scripts/part3-preflight.ps1`: **all 16 required source/configuration/evidence paths PASS**.
- Verified GitHub Actions run #72 for `f025338`: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41501949381, overall **success**. Four successful jobs: `build-and-test`, `compose-integration`, `blob-sdk-integration`, `docker-portal`.
- Existing isolated API/MVC/Azurite end-to-end script evidence: 34/34 assertions on **2 November** in `docs/evidence/Part3-E2E-Date-Filters-20261102.txt`. This was NOT rerun today.
- Existing desktop and simulated mobile screenshots: `docs/evidence/Part3-Desktop-RaceDay-20261031.png` and `docs/evidence/Part3-Mobile-RaceDay-20261031.png`, not a real-phone test.

## Current submission checklist
- [x] Reuse the original Part 2 API and extend with separate Part 3 MVC portal, API-backed workflows.
- [x] Include MVC and API source, both Dockerfiles, Compose, private Blob SDK implementation and tests.
- [x] Meet numerical minimum of 20 commits on this development branch (22 before this documentation checkpoint).
- [x] Latest source code has a successful four-job GitHub Actions run at `f025338`. **Recheck for any later documentation commits.**
- [x] Run today's local tests and warnings-as-errors build.
- [x] Keep genuine E2E transcripts and existing screenshots in repo.
- [ ] Match the **actual official lecturer Part 3 brief/rubric** to features, marks and exact submission fields; brief was not located in reviewed files or Drive.
- [ ] Test actual phone layout, keyboard navigation, and screen reader using your own devices and capture genuine results.
- [ ] Clarify whether live Azure cloud is compulsory. Azurite only is verified; do not misrepresent emulator as real Azure. If required, test authorized real Azure account without exposing keys.
- [ ] Show an authenticated API-backed action from a real container, if lecturer requires it. CI's Compose job verified startup/readiness, not a complete authenticated container journey; Docker not available on this laptop.
- [ ] Student personally record an original walkthrough and upload it as an unlisted YouTube video. Put the **actual Part 3** URL in README; the existing README's YouTube link is for Part 2.
- [ ] Give accurate AI assistance disclosure according to course policy, understand and explain source and design.
- [ ] Student submits repository **branch** link, unlisted demo and any required files/fields to ARC, then saves actual submission confirmation and checks deadlines.

## Handover links
- Part 3 source: https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review
- Review PR: https://github.com/tkhumalo2022/RaceDay_Part2/pull/3
- Latest successful source CI: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41501949381
- Demo order and honest limitation notes: `docs/PART3_DEMO_AND_HANDOVER.md`
- Full release checklist: `docs/PART3_RELEASE_CHECKLIST.md`

**Do not merge the draft PR or submit on the user's behalf; preserve historical Part 2 and existing DB.**