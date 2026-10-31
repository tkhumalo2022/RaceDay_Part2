# PROG6212: Part 3 work in progress

Student: Thabiso Khumalo. Project: RaceDay. This branch is a **development milestone**, not a completed assessed submission.

## Built here
- New `src/RaCeDay.Portal` ASP.NET Core MVC project, independent of Part 2 EF Core and SQL Server.
- Backend API client uses HTTP for events, categories, authentication and session; it does not copy API workflow rules.
- Login, registration and logout forms call backend API endpoints using their CSRF tokens.
- Event catalogue with Run/Walk/Cycle filters, event detail/category pages and responsive design.
- Solution includes the MVC project; Dockerfile for the frontend.

## Outstanding Part 3 work
- Confirm real two-app authentication flow and sessions through browser, with both roles.
- Organiser dashboard, event CRUD, banner image upload, categories, entries and results.
- Participant enrolment, My Enrolments, My Results, profile and profile picture.
- Azure Blob Storage **in the API**; frontend must upload through API, never to Azure directly.
- Docker build/run verification, CI evidence and accessibility testing.
- Official green CI screenshot, student-recorded unlisted video link and ARC submission.

## Run locally
1. In repository root: `dotnet restore RaCeDay.slnx` then `dotnet build RaCeDay.slnx -c Release`.
2. Start the Part 2 API with its configured SQL Server and Identity roles on `http://localhost:5001`.
3. Start portal: `dotnet run --project src/RaCeDay.Portal --urls http://localhost:5250`.
4. Open `http://localhost:5250`, register/sign in and view events. This local cookie forwarding assumes the **same hostname** across both ports.
5. Docker: `docker build -f src/RaCeDay.Portal/Dockerfile -t raceday-portal .` and `docker run --rm -p 8080:8080 -e RaCeDayApi__BaseUrl=http://host.docker.internal:5001/ raceday-portal`. Docker runtime has **not** yet been verified.

## Security and academic integrity
- This same-host local development cookie relay is not a production authentication architecture. Before public deployment, configure HTTPS, ingress, secure cookies, session persistence, and anti-CSRF end-to-end testing.
- This scaffold used AI assistance. Read, explain and amend the code yourself. Disclose the use as required by the PROG6212 brief.
- Never manufacture commits, screenshots, tested results or a recorded presentation.


## Checks performed 31 October 2026
- Release solution build succeeded, 0 warnings, 0 errors.
- Existing Part 2 automated tests: 39 passed, 0 failed, 0 skipped.
- New MVC sign-in and register GET routes both returned HTTP 200 locally.
- API dependent pages could not be fully verified because the backend was not reachable in the smoke test. A full authenticated browser walkthrough is still mandatory.
- Docker is not installed in the tested Windows environment. Only the Dockerfile was prepared; no successful container run is claimed.
- The separate portal also has early API-backed Organiser and Participant dashboard views, not complete Part 3 workflows.
