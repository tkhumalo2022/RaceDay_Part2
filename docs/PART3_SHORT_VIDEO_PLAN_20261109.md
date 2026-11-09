# RaceDay Part 3 — short student-recorded video plan

**Target:** around 5–6 minutes. This is a speaking guide, **not a recorded video or evidence of actions that were not performed**. Record the demonstration yourself, in your own words. Capture only working functionality and disclose AI assistance as required.

| Timing | What to show | Suggested talking point (adapt in your own voice) |
| --- | --- | --- |
| 00:00–00:25 | Your name, module and RaceDay homepage | "This is RaceDay, my PROG6212 Part 3. The main change is a separate MVC website using my Part 2 REST API." |
| 00:25–01:00 | Solution Explorer: `src/RaCeDay` and `src/RaCeDay.Portal`; `RaceDayApiClient.cs` | "The API owns database access, race workflows and image storage. The MVC site asks the API for data over HTTP and handles the views." |
| 01:00–01:40 | Register/sign in as Organiser, then Participant; role-based menu | "Registration, login and logout go through the API. Organisers and participants have different menus and protected actions." |
| 01:40–02:45 | Organiser dashboard; create/edit event, category and enrolment list | "Organisers can manage events and categories and see who has enrolled." |
| 02:45–03:45 | Participant search filters, event details, enrolment, My Events, profile | "Participants filter upcoming races, pick a category and enrol. This is saved through the API." |
| 03:45–04:25 | Enter a result as Organiser, view as Participant | "The organiser records finish details. The participant then sees their result, finish time and position." |
| 04:25–05:00 | Image upload in a **real running test**, and Azure SDK code | "The MVC sends images to the API; the API uses Azure Blob SDK. If using the emulator, say *Azurite*, not real Azure." |
| 05:00–05:35 | Dockerfiles, `compose.yaml`, successful [CI run](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42603394014) | "All four cloud CI jobs passed: solution tests, Docker portal, Azure SDK emulator tests and the four-service Compose startup. Docker was run by GitHub Actions, not locally on my laptop." |
| 05:35–05:55 | Git commits, source branch, short reflection | "I learned to separate UI and business logic, protect roles and test the full journey. I still need to be careful about real Azure and production hosting." |

## Recording checklist

1. Use a clean browser with **fake** test accounts and a non-sensitive password; never expose cloud keys or school portal credentials.
2. Open the MVC portal and API locally as documented; verify sign-in and enrolment actually work **before** recording.
3. Show the browser and relevant code live; not just a slideshow of code.
4. Capture genuine screenshots as required; include a real Azure upload only if an authorised cloud account has been provisioned and tested.
5. Upload **your own** screen recording to YouTube as unlisted. Check the link in a private/incognito browser, then add the real URL to the Part 3 README and final ARC submission.

**Evidence currently available:** 59/59 local unit/integration tests and green four-job GitHub Actions on 9 November 2026. The complete 34-check end-to-end browser/API/Blob workflow used a separate QA database and local Azurite emulator. These are real tests but **not** proof of an unrecorded video.
