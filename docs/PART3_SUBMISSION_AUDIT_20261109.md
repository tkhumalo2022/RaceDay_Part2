# RaceDay PROG6212 Part 3 — submission audit (9 November 2026)

This is an evidence-based student review aid, **not** a grade, lecturer sign-off, video submission, or proof of public hosting.

## Verified against the existing repository

| Assessment area | Evidence available | Status / scope |
| --- | --- | --- |
| Source and README | `src/RaCeDay.Portal`, `src/RaCeDay`, `README.md` | Present on `feature/part3-mvc-review`. |
| CI/CD, build and tests | [PR run #42603394014](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42603394014) on `8db121b` | All four jobs green: build-and-test, Docker portal, Blob SDK/Azurite, Compose startup. Local Release build 0 errors / 0 warnings; 59/59 tests passed on 9 Nov. |
| Git commits | [Part 3 branch commits](https://github.com/tkhumalo2022/RaceDay_Part2/commits/feature/part3-mvc-review) | 25 commits ahead of `main` before this audit; meaningfulness requires student/lecturer review. |
| Authentication and role navigation | `AuthApiController.cs`, `AccountController.cs`, `DashboardController.cs`, `CoreWorkflowTests.cs` | Code and automated integration tests exist. |
| Organiser functions | `OrganiserController.cs`, event and category APIs, result and banner flows | Create/edit/delete events, categories, enrolment list, result entry and banner endpoint implemented. |
| Participant functions | `ParticipantController.cs`, `HomeController.cs`, `DashboardController.cs` | Discover/search/filter, details, enrolment, profile and results implemented. |
| Separate MVC -> Part 2 API | `RaceDayApiClient.cs`, `RaceDayApiClient.Writes.cs` | Portal uses HTTP to API, not EF/SQL. [Actual 28-check isolated workflow](evidence/Part3-Core-Integration-20261109.md) passed on 9 Nov against `b2877bb`. |
| Azure Blob Storage | `AzureRaceImageStore.cs`, `ImagesApiController.cs`, Azure SDK integration test | Private image flow implemented; **Azurite** test successful, **live Azure account** unverified. |
| Docker | Both Dockerfiles, `compose.yaml`, `scripts/ci-compose-smoke.sh` | Four-service startup passed in hosted GitHub Actions CI; no local Docker installed; authenticated container journey not verified. |
| UI | `src/RaCeDay.Portal/Views`, responsive CSS, [desktop](evidence/Part3-Desktop-RaceDay-20261031.png) and [mobile](evidence/Part3-Mobile-RaceDay-20261031.png) QA screenshots | Automated and saved QA images exist; physical phone/keyboard/screen reader still require real checks. |
| Presentation and final submission | [Student demonstration guide](PART3_DEMO_AND_HANDOVER.md) | **Not complete**: student-recorded unlisted Part 3 YouTube link and ARC submission confirmation. |

## Student's remaining actions before ARC submission

1. Review the implementation and lecturer's exact Part 3 rubric; check especially the 20 **meaningful** commits rather than relying only on their count.
2. Record an original short screen-share video demonstrating MVC/API architecture, organiser/participant login, event/category/enrolment/results, private image upload, Docker and a successful CI run. Do not substitute the existing **Part 2** YouTube video.
3. If live Azure storage is mandated, provision an authorised Azure Storage account and show a real image upload/retrieval; the current Azurite emulator does **not** satisfy live cloud proof.
4. Do a real mobile and keyboard accessibility pass; fix any observed issues and rerun tests/CI.
5. Check the final branch HEAD and green CI. Paste the real unlisted YouTube URL in README only after recording. Submit required links/files on ARC personally and verify receipt.

## Verified source and links

- [Source branch](https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review) | [Draft PR #3](https://github.com/tkhumalo2022/RaceDay_Part2/pull/3)
- [Four-job latest successful CI, 9 November](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42603394014)
- [Local integration evidence: 28 checks](evidence/Part3-Core-Integration-20261109.md)
- [Docker local demo instructions](PART3_DOCKER_COMPOSE.md) | [Cloud deployment safety gate](PART3_CLOUD_DEPLOYMENT_GATE.md)

**Academic integrity:** AI assistance contributed to this project. The student must make an accurate disclosure under the institution's rules, be able to explain the code, and record their own genuine demonstration. No video, live Azure run, public web demo, or ARC upload is claimed here.
