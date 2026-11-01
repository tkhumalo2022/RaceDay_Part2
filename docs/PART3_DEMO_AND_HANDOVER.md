# PROG6212 Part 3 — Student Demo & Final Submission Handover

**Student:** Thabiso Khumalo
**Project:** RaceDay — MVC frontend consuming the Part 2 API
**Submission status:** IN REVIEW. Do not submit until every mandatory lecturer requirement is verified.

## Before you record

- Open the Part 3 review branch: https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review
- Make sure the SQL Server API and separate MVC portal are running; use the isolated QA database, not real client/production data.
- Check the final GitHub Actions run is GREEN and includes Windows build/tests, Docker image smoke check, and Azurite SDK integration test. Save the actual run URL.
- Configure and verify an **authorised real Azure Storage account** if the lecturer requires cloud evidence. The local Azurite emulator demonstrates Azure SDK behaviour but is not a live cloud account. Do not display keys or connection strings.
- Make sure Docker demonstration runs with the backend accessible; a container sign-in page alone is not proof of API integration.
- Create sample Organiser and Participant accounts and at least one suitable future event/category. Do not expose real passwords.
- Test on an actual phone, including narrow navigation, event filters, forms and result screens.
- Run `powershell -ExecutionPolicy Bypass -File scripts/part3-preflight.ps1` and address warnings relevant to the rubric.

## Student-recorded demonstration order

This is a **guide**, not a fabricated recording. Speak in your own words and demonstrate each action live.

1. **Introduction:** State project, module, your identity and the goal: a separate MVC frontend working through the Part 2 REST API.
2. **Architecture:** Show the solution contains `src/RaCeDay` (API and EF Core) and `src/RaCeDay.Portal` (MVC). Explain why the portal uses `RaceDayApiClient`, not SQL/EF or duplicated workflow rules.
3. **Authentication:** Register or sign in as Participant, show the role-specific navigation; log out. Sign in as Organiser. Show an unauthorised participant cannot access Organiser-only actions.
4. **Organiser workflow:** Display dashboard totals and upcoming races. Create/edit an event, upload a banner through the API, manage categories, and inspect enrolments.
5. **Participant workflow:** Browse and filter events, review event details/categories, enrol, edit profile and upload profile picture through the API.
6. **Results:** As Organiser, record an actual result for the test participant. As Participant, show the updated result with finishing time and position.
7. **Azure:** Show upload and retrieval of a private image through MVC -> API -> Blob. If testing Azurite only, label it as an emulator explicitly; never call it a live Azure Storage account.
8. **Docker:** Show the Dockerfile, a successful container build/launch, and the portal loading from the container. Verify a genuine API-backed action through the container if possible.
9. **GitHub evidence:** Open the latest successful GitHub Actions run and actual meaningful Part 3 commit history. Mention successful local test output and responsive screenshots.
10. **Reflection:** State your own design choices, what you tested, and any known limitations. Do not present AI-generated code as independent work.

## Submission evidence checklist

| Artifact | Source | What to confirm |
| --- | --- | --- |
| Correct GitHub URL | [Part 3 branch](https://github.com/tkhumalo2022/RaceDay_Part2/tree/feature/part3-mvc-review) | Link opens and contains the final source |
| MVC source | `src/RaCeDay.Portal` | All MVC data access goes through API |
| API source | `src/RaCeDay` | DB/workflow logic and Blob SDK remain in API |
| Dockerfile | `src/RaCeDay.Portal/Dockerfile` | Live build and run evidence |
| README | `README.md` + this file | Accurate launch/demo instructions |
| CI | `.github/workflows/dotnet-build.yml` | Latest branch commit has green jobs |
| Visual evidence | `docs/evidence/Part3-Desktop-RaceDay-20261031.png` and mobile image | Both genuine, accurately described |
| Commit history | `git log main..feature/part3-mvc-review` | At least 20 genuine Part 3 improvements, if required |
| Presentation | Student's unlisted YouTube URL | Must be recorded by you; add real link to README |
| ARC | Assignment portal | Submit final GitHub URL and other required fields yourself |

## Responsible AI disclosure

Suggested wording to adapt **accurately** to your actual work:

> I used an AI assistant for code scaffolding, development suggestions, troubleshooting, testing guidance, and documentation. I personally reviewed the implementation, tested the features and can explain the design decisions. The demonstration and evidence shown are my own genuine run-through of the application.

Do not submit a false statement of personal review or testing. Check the lecturer's precise permitted use and attribution requirements first.

## Known remaining sign-offs

- Real Azure cloud configuration/verification has not been confirmed.
- Docker integration beyond image/startup smoke tests requires verification.
- Physical-phone and keyboard/screen-reader checks need human validation.
- The student video and real unlisted URL are not recorded here.
- The Part 3 branch may have fewer than the minimum meaningful commits.
- ARC submission has not taken place. No final-grade claim is made.

## Quick commands

```powershell
dotnet build RaCeDay.slnx -c Debug --warnaserror
dotnet test RaCeDay.slnx -c Debug --no-restore
powershell -ExecutionPolicy Bypass -File scripts/part3-preflight.ps1
git log --oneline main..feature/part3-mvc-review
```

Use **Debug** locally when the Release applications are already running, to avoid Windows DLL file locks. GitHub Actions verifies a clean **Release** build.

**Important:** Do not invent commits, screenshots, cloud credentials, grades or the unlisted video link.
