# PROG6212 Part 2 — RaCeDay demonstration checklist and recording script

Prepared for **Thabiso Khumalo**. This document is a recording aid, not a video submission. Use a real screen recording of the student's running project and their own explanation. Do not claim features the actual application cannot demonstrate.

Target: 8–12 minutes. Upload as **Unlisted** (not Private or Public unless your lecturer says otherwise). Add the unlisted YouTube link to the repository README before submitting the GitHub link on ARC.

| Segment | Suggested duration | Real evidence to demonstrate |
| --- | --- | --- |
| Introduce project | 0:00–0:45 | Introduce RaCeDay, the Organiser/Participant roles, Part 2 API scope |
| Explain solution | 0:45–1:45 | Show ASP.NET Core API, EF DbContext, migrations, Identity and role protection |
| Register / sign in | 1:45–3:00 | Swagger: get CSRF token, register an Organiser, logout, login, show own profile; demonstrate `GET /api/auth/session` with user ID/role stored server-side, explain Identity auth cookie; Admin signup blocked |
| Event and category API | 3:00–4:30 | Organiser creates a future race and two distance categories, retrieves, edits and lists them in Swagger |
| Participant workflows | 4:30–6:00 | Register a Participant with date of birth; enrol once, demonstrate duplicate entry/capacity error; check own entry |
| Privacy and results | 6:00–7:00 | Show forbidden Organiser-only request from Participant; owning Organiser closes event and records result; Participant views own result |
| Database and testing | 7:00–8:30 | Show SQL tables/migrations, stored category count and payment amount check; show real `dotnet test` with passing output |
| GitHub and CI | 8:30–9:30 | Show genuinely pushed branch, meaningful commit history, GitHub Actions green build/test run and CI screenshot |
| Close and submission | 9:30–10:00 | Summarise what Part 2 accomplishes, show README setup and unlisted URL |

## Authentication steps in Swagger

1. In Development, navigate to `/swagger`.
2. Call anonymous `GET /api/auth/antiforgery` to receive a request token and session cookie in the same browser.
3. Paste `requestToken` into Swagger's Authorize (X-CSRF-TOKEN).
4. Call `POST /api/auth/register` with `role: "Organiser"`, `organiserName`, first/last name, email, password, confirmed password.
5. On successful registration refresh token with `GET /api/auth/antiforgery`, then use protected API requests.
6. To demonstrate Participant role, use a distinct account with `role: "Participant"` and an ISO `dateOfBirth`.
7. Explain: Identity session cookie plus CSRF, **not JWT**; only authorised role can execute each API action.

## What must not be faked

- Do not create dummy commits merely to reach 20.
- Do not use edited/AI-generated green CI screenshots.
- Do not suggest Pending payment attempts are paid or actually processed.
- Do not film an ARC submission as successful until ARC confirms it.
- Do not upload the lecturer's copyright-protected rubric as an attachment to a public repository.

## Final checklist

- [ ] Real student-led screen recording captured with understandable voice.
- [ ] Clear evidence of API behaviour, Swagger, role boundaries, and tests.
- [ ] Actual GitHub Actions green result shown, not merely local tests.
- [ ] Video uploaded as Unlisted, URL copied into README.
- [ ] Repository branch pushed and marks/commit requirement genuinely met.
- [ ] Only then submit the repository link on ARC.
