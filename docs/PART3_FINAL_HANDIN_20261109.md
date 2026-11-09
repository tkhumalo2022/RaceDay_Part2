# RaceDay PROG6212 Part 3 — Final hand-in sheet (9 November 2026)

**Student:** Thabiso Khumalo · **Repository:** https://github.com/tkhumalo2022/RaceDay_Part2  
**Required source branch:** `feature/part3-mvc-review` · **Draft PR:** https://github.com/tkhumalo2022/RaceDay_Part2/pull/3  
**Student-supplied Part 3 YouTube:** https://youtu.be/XoXfNxmoALU  
**Verified pre-documentation-change HEAD:** `b35198c72e4adf1b01d6932990a8e8a851b51f01`  
**CI evidence:** https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42647341004 — all four PR jobs successful at that HEAD; 40-step authenticated Compose journey according to stored evidence.

> This sheet assembles accurate source and evidence links. It does not claim to have watched the YouTube video, proven Unlisted status, connected to a live Azure subscription, run on a physical phone or submitted to ARC. After any documentation/code commit, recheck CI for the NEW HEAD.

## Copy into the ARC submission fields

**GitHub repository:** https://github.com/tkhumalo2022/RaceDay_Part2  
**Part 3 source branch:** https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review  
**Part 3 demonstration video:** https://youtu.be/XoXfNxmoALU  
**GitHub Actions CI:** https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42647341004 (replace with the passing run for the final submission commit if applicable)  
**Docker instructions:** https://github.com/tkhumalo2022/RaceDay_Part2/blob/feature/part3-mvc-review/docs/PART3_DOCKER_COMPOSE.md

## Marking rubric — maximum 100 marks

| Criterion | Max | Evidence / final check |
| --- | ---: | --- |
| Correct Submission | 5 | Branch, full source, Dockerfile, README and video URL; ARC receipt still needed |
| GitHub and CI/CD | 10 | 31 commits ahead of main before documentation update, with verified green CI; lecturer determines meaningfulness |
| Authentication / role navigation | 10 | Account/session/roles and access-control tests; display login/logout in video |
| Organiser interface | 15 | Dashboard, event CRUD, categories, enrolments and published results |
| Participant interface | 15 | Filtered event discovery, event detail, enrolment, My Events, My Results, profile |
| Azure Blob Storage | 10 | MVC -> Part 2 API -> Azure SDK -> **Azurite** local emulator, stored Blob URL; live Azure unverified |
| Docker | 10 | MVC and API Dockerfiles, four-service Compose, verified hosted CI workflow |
| UI Design | 10 | RaceDay responsive views and status badges; physical-phone check still needed |
| Video Presentation | 15 | Student-supplied YouTube link; **watch to confirm** all required demonstrative topics |
| **Total** | **100** | Rubric maximum, **not an awarded grade** |

## Student-only final checks

- [ ] Open the YouTube URL in a private/incognito browser. Confirm it is the correct **Part 3** presentation and set to **Unlisted**; check organiser/participant, API, Blob, Docker and CI are genuinely covered.
- [ ] Capture a genuine screenshot of the final successful GitHub Actions check run, with the commit reference visible.
- [ ] Open both interfaces as an organiser and participant on an actual phone; test forms, navigation and statuses. Keep honest evidence.
- [ ] If the lecturer requires a real **Azure cloud** account instead of Azurite, configure it privately and record a genuine successful image upload/retrieval before claiming the criterion.
- [ ] Ensure the latest **Part 3 branch commit** is green. A green earlier commit is not sufficient to establish the newest one.
- [ ] Check the module's academic-integrity guidance and accurately declare AI-assisted implementation where required.
- [ ] Submit the branch URL and your unlisted Part 3 YouTube link on ARC. Only mark submission complete after ARC shows a confirmation/receipt.

## Architecture explained simply

The MVC project at `src/RaCeDay.Portal` is the website. It uses `RaceDayApiClient` over HTTP to communicate with the existing Part 2 API at `src/RaCeDay`. Only the API manages SQL Server, access-controlled race operations and private blob images. The local four-service demonstration includes the website, API, SQL Server and Azurite. Do not confuse that emulator with Microsoft's public Azure cloud service.

**Project-specific evidence:** [rubric audit](PART3_RUBRIC_AUDIT_20261109.md), [full handover](PART3_DEMO_AND_HANDOVER.md), [verified workflow output](evidence/Part3-Rubric-BlobUrl-Persistence-QA-20261109.txt), [SQL Blob URL check](evidence/Part3-Sql-Blob-Url-Check-20261109.txt).

**Status at document creation:** Code and automated pipeline evidence found. Original YouTube link supplied but not inspected. ARC confirmation not received.
