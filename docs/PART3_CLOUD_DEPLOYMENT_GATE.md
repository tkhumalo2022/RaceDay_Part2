# RaceDay Part 3 — cloud deployment gate

**Status as of 9 November 2026:** GitHub source is uploaded and its hosted CI pipeline is passing. A continuously hosted **public RaceDay application has not been deployed or verified**. Do not distribute a static-only link as if registration and enrolment work.

## Current four-service assessment stack

The checked-in `compose.yaml` is deliberately **local-only Development testing**, binding its HTTP ports to `127.0.0.1`, and uses SQL Server Developer and Azurite. Do not point a public cloud provider at this Compose file unchanged.

A public application must have:

1. Persistent private SQL Server with secure access, backups, reviewed EF migrations and seeded role names; never use test data/QA databases.
2. A backend API not accessible anonymously for write operations, plus a separate public HTTPS MVC frontend and a properly configured internal API base URL.
3. Correct reverse-proxy forwarding, secure cookie/CSRF/session handling across the browser-to-MVC and MVC-to-API hops, and durable ASP.NET Core Data Protection keys across restarts. Verify login/logout and protected role actions using the actual public domain.
4. Authenticated/private live Azure Blob storage if required by the rubric; keep the connection string and all passwords in provider secrets, **never in Git, Docker build arguments or URLs**.
5. A verified email confirmation workflow before public production registration (production code requires confirmed accounts). Avoid publishing the Development-mode sign-in bypass or Swagger to the public internet.
6. Endpoint health checks, least-privilege hosting, logs without secrets, resource/cost approval, and real end-to-end browser tests including registration, enrolment, banners and results.

## What is verified today

- Local .NET 10 Release build: **0 errors / 0 warnings**; local automated suite: **59 passed**.
- GitHub Actions [run #42640996423](https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/42640996423) passed all four jobs on `242374a`, including the authenticated 38/38 container workflow.
- Isolated local MVC -> API -> SQL Server -> Azurite workflow passed **34 checks**, including banner/profile-image upload and retrieval; this is **not** a live Azure account.
- No live SQL Server/real Azure cloud credentials or hosted HTTPS URL were verified on this machine. The `az` CLI and Docker Desktop are unavailable locally.

## Recommended path

For **assessment evidence**, use the existing GitHub branch, green CI, Docker instructions, test records and a genuine narrated demo. The supplied Part 3 requirements should be checked to confirm whether a *public* URL is actually mandatory.

For **public hosting**, use a properly configured container platform (e.g. Railway/Render/Azure) with an approved resource plan, persistent SQL Server and a real Azure Storage resource. Do not create a public front end alone or treat emulator tests as production proof. First stage a deployment, configure secrets privately, then run the complete protected workflow under HTTPS before sharing an assessor URL.

**No hosting charge, public DNS, or new cloud credential is authorised or claimed by this document.**
