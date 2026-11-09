# RaceDay Admin — peer-review handover

**Scope:** New Admin feature on `deploy/raceday-cloud-review-20261108`, kept separate from the PROG6212 Part 3 assessment branch. The existing Part 2 Admin totals page remains; this adds a separate API-backed Part 3 MVC Admin command centre.

## What is implemented

- `GET /api/admin/overview`: authenticated Admin-only totals for users, organisers, participants, events, categories, enrolments, recorded results, payment attempts, pending payment attempts and upcoming events.
- `GET /api/admin/users?query=...&role=...&page=...`: searchable, role-filtered, paginated user accounts (15 per page) with email confirmation and disabled flags.
- `GET /api/admin/events?page=...`: event oversight (12 per page), organiser ownership, enrolment counts and recorded result counts.
- `GET /api/admin/enrolments?page=...`: recent enrolment overview, paginated (12 per page).
- `GET /api/admin/reports/events.csv`: Admin-only CSV export capped at the 2000 latest events, no participant emails, field quoting and formula-injection safety.
- `POST /api/admin/users/{id}/access`: enable or disable a non-admin user. Explicit anti-forgery validation. Disabling invalidates active API sessions immediately.
- `POST /api/admin/users/{id}/role`: switch a non-admin user between Participant and Organiser **only if there are no linked event/enrolment records**. Admin self-promotion and Admin account changes are forbidden. Organisation name is required for Organiser.
- MVC `/Admin` frontend consumes those API endpoints, shows real data, status, sorting/pagination, search/role filters, account controls, event and enrolment tables, and CSV report. No EF Core/database connection in portal.
- `/Dashboard` detects Admin users and routes to `/Admin`. Admin role navigation appears in the site header.
- Stats and list emptiness are never presented as real zero/empty when the API failed; dedicated unavailable states are shown.

## Security and design guardrails

Admin API requires `[Authorize(Roles="Admin")]`; every mutating action checks CSRF and current Identity session. The API verifies current account status and role claims on each authenticated request, so disabled accounts or stale role cookies are rejected immediately. This adds a database lookup per authenticated API request; revisit caching/performance after a real workload and security review.

Admin cannot disable other Admin accounts or promote users to Admin through these routes. Account role changes are intentionally refused if they would break ownership consistency. Event management remains owned by Organisers: admin has an **oversight/reporting** role, not cross-owner CRUD. Payments are recorded as **attempts**, not actual revenue. No real payment processing or settlement exists.

The MVC dashboard has no hardcoded sample statistics; summary values come from API-backed SQL queries. When data requests fail, the dashboard warns rather than presenting artificial zero values.

## How Sam can review

1. Read the branch and draft pull request; inspect `src/RaCeDay/Controllers/AdminApiController.cs`, `src/RaCeDay.Portal/Controllers/AdminController.cs`, `src/RaCeDay.Portal/Views/Admin/Index.cshtml` and `tests/RaCeDay.Tests/AdminApiTests.cs`.
2. Run the solution locally with .NET 10, SQL Server and the existing isolated QA setup in `docs/PART3_RELEASE_CHECKLIST.md` or use the Docker Compose development guide. **Do not run reset/delete scripts against an existing database.**
3. For an authorized local Development-only Admin account, run the API's `--create-admin` command with a fresh test email and a securely supplied password in `RACEDAY_ADMIN_EMAIL`/`RACEDAY_ADMIN_PASSWORD`. Do not check credentials into the repo. Register other test users as Participant/Organiser.
4. Start the API and portal on the same hostname as documented; log in as Admin, navigate to `/Admin` and test search, pagination, enable/disable, role changes, event overview and CSV download.
5. Verify the participant and organiser accounts cannot access `/api/admin/overview` or `/api/admin/reports/events.csv`.
6. Verify disabled users lose access and that accounts with linked events/enrolments cannot change role.

## Verification and remaining limitations

- **8 November 2026:** 65/65 local xUnit tests passed, Debug build with warnings-as-errors clean on prior tested cloud branch revision `9f71872`. Re-run after newer UI/availability-state edits and cite only the actual result.
- GitHub Actions branch checks must be inspected for the latest head, not a past commit.
- This is not yet a publicly hosted site. The Render Blueprint requires manual env values, a newly provisioned Azure SQL Database and private Azure Blob account, secure migrations/roles, and live browser tests.
- Production account email confirmation sender, persistent Data Protection keys/session state, backups, rate limiting, domain/TLS and monitoring remain required before real-world usage.
- The CSV caps output at 2000 events. Add scheduled streaming/export jobs if real datasets grow beyond the coursework scope.
- Keep this work separate from the academically graded Part 3 branch until the student has personally reviewed the code and lecturer requirements.
