# PROG6212 Part 3 — Actual rubric evidence audit (9 November 2026)

**Assessment:** Part 3 of PROG6212 POE (100 marks). **Student project:** RaceDay, `feature/part3-mvc-review`.

**Rubric source:** Direct visual inspection of the **PART 3** marking table in the institution's open `PROG6212_POE.docx` Word Online tab on 9 November, at reduced zoom to read the complete band columns. This supersedes the earlier workplan-only rubric interpretation. The original document is not modified by this audit. The figures below are *maximum possible marks*, not an awarded grade.

| Exact Part 3 marking category | Maximum | Strong-band expectations visible in the actual Word rubric | Evidence verified or still missing |
| --- | ---: | --- | --- |
| Correct Submission | 5 | Clear folder layout and complete README with a working Docker run command, CI screenshot, and **video link** | Repository, README, Docker docs and green CI exist. **Need the original Part 3 video link and ARC submission evidence**; screenshot should be included in handover. |
| GitHub and CI/CD | 10 | **20+ meaningful commits**, CI validates repository structure with a green build | Branch already has 29 commits ahead of main before this final Blob URL change, plus verified prior four-job CI. Lecturer evaluates commit meaningfulness. Recheck the new head after push. |
| Authentication and Role-Based Navigation | 10 | Working register/login, session contains user ID and role, role-specific menus, logout clears session, guests redirected | Identity + server-side session, two roles, logout and access denial implemented. Login/registration and 403 access tested. Capture visual logout proof. |
| Organiser Interface | 15 | All pages functional: dashboard, events, categories, enrolments, results and event management | Event CRUD/category CRUD/start lists/results implemented. Create and Edit forms both accept banners via API. E2E verifies organiser workflows; human-run delete evidence still worth capturing. |
| Participant Interface | 15 | Fully functional home, event detail with enrolment, My Events, My Results showing finish time and position, profile | Event search/filters, categories/enrolments, dashboard/history/results and profile work in integrated tests. Show the complete journey in video. |
| Azure Blob Storage | 10 | Full **MVC → Part 2 API → Azure Blob Storage** image flow, image appears in MVC; intermediate band **explicitly demands image URL saved to the database** | **Gap addressed in this audit:** new EF migration persists actual Blob URL in `Event.BannerBlobUrl` and `AspNetUsers.ProfileBlobUrl`. Stored URLs have no SAS secrets. Both remain private and render via authenticated MVC/API proxy. 40/40 end-to-end checks and a direct SQL check confirm persistence on an **isolated QA database with Azurite**. **Live Azure subscription evidence is still missing**. |
| Docker | 10 | Application builds and runs cleanly inside Docker with a working README command | Both Dockerfiles, four-service Compose, CI builds and authenticated workflow. Last confirmed full Docker run #42642195375 passed 4/4 jobs and 38/38 before latest URL migration. Recheck 40-check CI on the new commit. |
| UI Design | 10 | Professional RaceDay branded design, colour-coded statuses, useful imagery and intuitive navigation | Responsive RaceDay styling and focus states exist, with prior desktop/mobile captures. Confirm actual phone, keyboard accessibility and images in final demo. |
| Video Presentation | 15 | Original engaging and professional demonstration: MVC pages, API/Blob integration, Docker setup, explanation | Script/storyboard is ready in [student video plan](PART3_SHORT_VIDEO_PLAN_20261109.md). **Actual student recording, unlisted YouTube URL and final ARC upload remain outstanding**. |
| **TOTAL** | **100** | Top-band requirements must be demonstrated to the assessor | **No claim of an official 100/100.** |

## Tested in the latest Azure-URL gap-fix

- **Release build:** 0 errors / 0 warnings, with apphost creation disabled locally to avoid locking a separately running demo binary.
- **Automated tests:** 59 passed, 0 failed.
- **New isolated QA database:** `RaceDayPart3RubricQA_20261109`; all EF migrations applied, including `Part3PersistBlobImageUrls`. No original Part 2 database was altered.
- **Full local MVC → API → SQL Server → Azurite E2E:** **40/40 PASS**, including event/banner upload, profile-picture upload, retrieval, and the presence of the newly persisted Blob URLs in the authenticated MVC views. [40-check raw transcript](evidence/Part3-Rubric-BlobUrl-Persistence-QA-20261109.txt).
- **Direct SQL inspection:** one QA event row and one QA profile row have valid stored Blob URLs without a `?` query/SAS fragment; migration present. [Sanitised SQL count evidence](evidence/Part3-Sql-Blob-Url-Check-20261109.txt).
- **Secrets/privacy:** storage keys remain private to the Part 2 API, URLs persisted without SAS query parameters, and MVC images are streamed through authenticated endpoints.
- **Latest hosted CI:** must be checked on the **final pushed commit** to verify the increased 40-check Compose workflow; previous 38-check run cannot be counted for the latest edits.

## Student sign-offs necessary for the rubric

1. Film your original Part 3 screencast demonstrating both projects, roles, event/category CRUD, enrolment/results, uploaded images, API endpoints, database link, Docker CI and responsive interface.
2. Upload to YouTube as **unlisted**, test link from another account/incognito, insert it into the README and required ARC form. Include a genuine passing CI screenshot.
3. If an **actual Azure cloud account** is required by the lecturer, demonstrate a live Blob upload rather than describing Azurite as Azure cloud; never publish connection strings.
4. Check keyboard, real phone, and the full organiser/participant journey. Then submit the required ARC artifacts and keep the confirmation receipt.
5. Review source and disclose AI assistance in accordance with module academic-integrity guidance.

**Not established:** lecturer-issued marks, completed student video, actual Azure subscription, production deployment, physical-phone verification or ARC submission receipt.
