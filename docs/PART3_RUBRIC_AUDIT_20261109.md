# PROG6212 Part 3 — Rubric-by-rubric evidence audit

**Date:** 9 November 2026. **Project:** Thabiso's existing RaceDay Part 3 review branch. **Total:** 100 marks across the Part 3 criteria below.

**Important:** This is an evidence-based readiness audit, not an official grade. The mark bands and task checklist are taken from the available 2026 PROG6212 Part 3 rubric-aligned workplan. The original `PROG6212_POE` Word Online document is open in the student's browser, but the full marking pages have not been extracted or independently reconciled here. Awarded marks depend on the lecturer, video, demonstration and ARC evidence.

| Criterion | Maximum | Verified implementation/evidence | Outstanding for full marks |
| --- | ---: | --- | --- |
| Correct submission | 5 | Public GitHub Part 3 branch, source, README and Docker instructions are present. | Add the actual unlisted Part 3 video link and personally submit the correct ARC fields/files; verify the rubric's exact required deliverables. |
| GitHub + CI/CD | 10 | 27 pre-audit commits ahead of `main`; prior PR CI run passed 4/4 jobs. This audit adds meaningful code, workflow and evidence changes. | Verify **new HEAD** with all required green jobs. The lecturer decides whether 20+ commits are meaningful. |
| Authentication and role navigation | 10 | API registration/login/logout; session stores user ID and role; Organiser and Participant views; 403 role restrictions; HTTP E2E proves registration, sessions and restrictions. | Record real browser sign-in/logout and role distinction in the student video. |
| Organiser interface | 15 | Organiser dashboard, event CRUD, category CRUD, enrolment list and results in code. **New:** optional banner field directly in event Create/Edit forms; API handles private blob. 38-check QA verifies edit + banner, enrolment and results. | Show create/edit/delete, category management, organiser totals, banner and publishing results in the demo; explicit delete UI testing remains worth checking. |
| Participant interface | 15 | Upcoming event filters, details/categories, enrolment, profile, My Events and My Results; 38-check QA confirms filtering, enrolment, profile edit, finish time and position. | Show persistence after refresh, My Events, My Results and profile picture in the student demo. |
| Azure Blob Storage | 10 | Private Azure SDK blob store exists in Part 2 API; MVC sends multipart through API; local Azurite banner and profile upload/read passed. | **Real Azure subscription round-trip not verified.** Confirm whether the brief demands actual Azure cloud rather than emulator and whether a persisted blob URL is mandatory; the code uses deterministic private blob keys and authenticated proxy URLs. |
| Docker | 10 | Dockerfiles for portal/API; local-only 4-service Compose; previous CI checks builds/startup. **New:** 38-check authenticated Compose E2E pipeline step; awaiting hosted green result. | Verify the new container workflow on GitHub and demonstrate container launch/usage in the original video. |
| UI design | 10 | Branded responsive design, focus states, input validation, skip links, reduced-motion CSS and earlier desktop/mobile QA captures. | Fresh mobile/keyboard/screen-reader checks and real browser evidence required for strongest UI band. |
| Student video presentation | 15 | [Student video walkthrough plan](PART3_SHORT_VIDEO_PLAN_20261109.md) is prepared. | **No Part 3 video is recorded or uploaded.** Student must narrate genuine feature, API, Azure and Docker demonstrations, publish unlisted to YouTube and paste the working link. |
| **Total** | **100** | Code and local testing are substantially advanced, but do not establish the mark total. | **Never claim 100/100 until lecturer awards it.** |

## Actual local technical result for this review

- `dotnet build RaCeDay.slnx -c Release --warnaserror`: 0 errors, 0 warnings.
- `dotnet test RaCeDay.slnx -c Release --no-build`: **59 passed, 0 failed**.
- Verified actual session-based, real HTTP workflow through local MVC → API → isolated SQL Server → Azurite: **38 of 38 checks passed**. [Raw transcript](evidence/Part3-Rubric-Workflow-20261109.txt).
- The complete 38-check script now checks the event Create and Edit banner fields and performs an actual multipart edit+banner write.
- Latest Docker authenticated QA GitHub workflow is in `.github/workflows/dotnet-build.yml`; count its run as verified **only** after GitHub confirms success on the commit being submitted.

## Correct final submission order

1. Verify all checks green at the final branch head, with an archived CI log/artifact showing the **authenticated** Docker workflow.
2. Record original student video, showing **both projects**, organiser/participant roles, event/category, enrolment/results, image flow, Dockerfile/container CI and responsive UI.
3. Only say **Azure cloud** if an authorised live cloud account was actually used. Local Azurite is emulator evidence; never expose secrets.
4. Add the genuine unlisted YouTube URL to the README, check it opens in a private browser, and submit the Part 3 GitHub branch/source plus any rubric-mandated files to ARC.
5. Review and accurately disclose AI assistance under the academic integrity rules.

**Not evidenced:** official grade, student's final video, ARC receipt, live Azure Storage account, public production deployment, or physical-phone accessibility sign-off.
