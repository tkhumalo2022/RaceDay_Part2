# RaceDay PROG6212 Part 3 — submission audit (9 November 2026)

This is an evidence-based student review aid, **not** a grade, lecturer sign-off, video submission, or proof of public hosting.

## Verified against the existing repository

| Assessment area | Evidence available | Status / scope |
| --- | --- | --- |
| Source and README | `src/RaCeDay.Portal`, `src/RaCeDay`, `README.md` | Present on `feature/part3-mvc-review`. |
| CI/CD, build and tests | [PR run #42636076552](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42647341004) on `0502a09` | All four PR jobs green on branch HEAD `b35198c` (9 November): build-and-test, Docker portal, Blob SDK/Azurite and authenticated four-service Compose. Latest stored E2E evidence reports 40/40 checks. Local Release build 0 errors / 0 warnings; 59/59 tests passed on 9 Nov. |
| Git commits | [Part 3 branch commits](https://github.com/tkhumalo2022/RaceDay_Part2/commits/feature/part3-mvc-review) | 31 commits ahead of `main` at verified HEAD `b35198c`; meaningfulness still requires student/lecturer review. |
| Authentication and role navigation | `AuthApiController.cs`, `AccountController.cs`, `DashboardController.cs`, `CoreWorkflowTests.cs` | Code and automated integration tests exist. |
| Organiser functions | `OrganiserController.cs`, event and category APIs, result and banner flows | Create/edit/delete events, categories, enrolment list, result entry and banner endpoint implemented. |
| Participant functions | `ParticipantController.cs`, `HomeController.cs`, `DashboardController.cs` | Discover/search/filter, details, enrolment, profile and results implemented. |
| Separate MVC -> Part 2 API | `RaceDayApiClient.cs`, `RaceDayApiClient.Writes.cs` | Portal uses HTTP to API, not EF/SQL. [Actual 34-check full isolated workflow](evidence/Part3-Full-Local-QA-20261109.txt) passed on 9 Nov against current source with the private Azurite image path working. |
| Azure Blob Storage | `AzureRaceImageStore.cs`, `ImagesApiController.cs`, Azure SDK integration test | Private image flow implemented; the local MVC -> API -> Azurite image upload and private retrieval passed in the 34-check QA run; **live Azure account** unverified. |
| Docker | Both Dockerfiles, `compose.yaml`, `scripts/ci-compose-smoke.sh` | Four-service startup passed in hosted GitHub Actions CI; no local Docker installed; authenticated container journey not verified. |
| UI | `src/RaCeDay.Portal/Views`, responsive CSS, [desktop](evidence/Part3-Desktop-RaceDay-20261031.png) and [mobile](evidence/Part3-Mobile-RaceDay-20261031.png) QA screenshots | Automated and saved QA images exist; physical phone/keyboard/screen reader still require real checks. |
| Presentation and final submission | [Student demonstration guide](PART3_DEMO_AND_HANDOVER.md) | Student supplied [Part 3 YouTube link](https://youtu.be/XoXfNxmoALU), but playback, visibility and content are unverified. **ARC submission confirmation is missing.** |

## Student's remaining actions before ARC submission

1. Review the implementation and lecturer's exact Part 3 rubric; check especially the 20 **meaningful** commits rather than relying only on their count.
2. Check your [supplied Part 3 video](https://youtu.be/XoXfNxmoALU) actually demonstrates MVC/API architecture, organiser/participant login, events/categories/enrolment/results, images, Docker and successful CI. If any parts are absent, record a revised authentic Part 3 walkthrough; do not substitute the **Part 2** video.
3. If live Azure storage is mandated, provision an authorised Azure Storage account and show a real image upload/retrieval; the current Azurite emulator does **not** satisfy live cloud proof.
4. Do a real mobile and keyboard accessibility pass; fix any observed issues and rerun tests/CI.
5. Check the final branch HEAD and green CI. Confirm your supplied YouTube video is Unlisted and accessible from incognito; the link is now in README. Submit required links/files on ARC personally and verify receipt.

## Verified source and links

- [Source branch](https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review) | [Draft PR #3](https://github.com/tkhumalo2022/RaceDay_Part2/pull/3)
- [Four-job latest successful CI, 9 November](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42647341004)
- [Local integration evidence: 28 checks](evidence/Part3-Core-Integration-20261109.md)
- [Docker local demo instructions](PART3_DOCKER_COMPOSE.md) | [Cloud deployment safety gate](PART3_CLOUD_DEPLOYMENT_GATE.md)

**Academic integrity:** AI assistance contributed to this project. The student must make an accurate disclosure under the institution's rules, be able to explain the code, and record their own genuine demonstration. No video, live Azure run, public web demo, or ARC upload is claimed here.
