# RaceDay — Cloud Peer-Review Deployment (NOT LIVE)

This document prepares a disposable **Staging** deployment for Sam to review PROG6212 RaceDay Part 3. It does **not** claim to be deployed, production-ready, ARC-submitted, or to have a functioning real Azure account. The assessment branch `feature/part3-mvc-review` remains separate.

## Architecture

```
Sam's browser (HTTPS)
       |
       v
Render Web: raceday-prog6212-portal-review  (.NET 10 MVC)
       |   HTTPS server-to-server via RaCeDayApi__BaseUrl
       v
Render Web: raceday-prog6212-api-review     (.NET 10 Web API / Identity)
       |                                |
       v                                v
Azure SQL Database                 Azure Blob Storage
SQL Server provider                private raceday-images container
```

The MVC portal talks only to the API. The API owns all Entity Framework Core database operations, roles/Identity, and Blob uploads. Do not deploy `compose.yaml` publicly: its SQL Server + Azurite stack is local-development only.

## Before provisioning

1. Choose the intended cloud account/workspace and confirm any cost or free-tier restrictions.
2. Use a **new, disposable** database and synthetic organiser/participant test records. Do not export the local development database, and do not migrate real classmate data.
3. Arrange a new **Azure SQL Database**, not a Supabase/Render Postgres database; the application uses the `Microsoft.EntityFrameworkCore.SqlServer` provider and existing SQL Server migrations. When eligible, use Azure SQL free tier with **auto-pause at the free limit**. Verify eligibility, region, firewall rules and plan in Azure before creation. Never silently select billable overage behavior.
4. Arrange an Azure Storage account and a **private** Blob container named `raceday-images`. Storage and bandwidth may have charges even if SQL is free.
5. Render's two free web services can sleep after idle and consume a shared monthly free runtime allowance; they are suitable for peer review, **not** an always-available production SLA. The portal uses a longer request timeout in Staging to accommodate an API cold start.

## Prepare the Azure SQL schema

- Create a new empty Azure SQL database (do not overwrite any existing local database).
- Configure Azure SQL firewall to allow **only** the necessary admin workstation address for migrations, and the Render API service's outbound CIDR addresses (find these after provisioning under Render service > Connect > Outbound). Do not open SQL to `0.0.0.0/0` for convenience. CIDRs can be shared by Render tenants; use a strong independent app database user with least privilege.
- Keep the SQL connection string in a secure local environment while applying migrations. Example structure (not credentials):

  `Server=tcp:YOUR_SERVER.database.windows.net,1433;Database=RaceDayReview;User ID=YOUR_APP_USER;Password=YOUR_SECRET;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;`

- From a trusted Windows terminal in the repository root, with .NET 10 SDK and the required EF local tool:

```powershell
dotnet restore RaCeDay.slnx
dotnet tool restore
$env:ASPNETCORE_ENVIRONMENT = 'Staging'
$env:ConnectionStrings__DefaultConnection = Read-Host 'Paste temporary Azure SQL connection string'
dotnet tool run dotnet-ef database update --project src/RaCeDay/RaCeDay.csproj --startup-project src/RaCeDay/RaCeDay.csproj --configuration Release
dotnet run --project src/RaCeDay/RaCeDay.csproj --no-launch-profile -- --seed-roles
Remove-Item Env:\ConnectionStrings__DefaultConnection
```

- EF migrations require a database principal with schema permissions. After applying them, configure the hosted API with a **separate restricted application database user** that can read/write the app's tables but not manage the SQL server. Validate the actual application's SQL and triggers before finalising privileges.
- Keep a genuine migration/connection test transcript; never claim success based only on file existence.

## Render Blueprint

The repository root has a `render.yaml` with two separate .NET 10 Docker web services. Provision it from the *deployment branch* (`deploy/raceday-cloud-review-20261108`), not from the assignment branch or `main`.

Fill in these secret service environment variables using Render's dashboard, never GitHub code or chat:

| Service | Variable | Purpose |
| --- | --- | --- |
| API | `ConnectionStrings__DefaultConnection` | Azure SQL app-login connection string |
| API | `AzureBlob__ConnectionString` | Azure Storage private-blob connection string |
| Portal | `RaCeDayApi__BaseUrl` | Public **HTTPS** API URL ending with `/` |

Non-secret settings are in the Blueprint: `ASPNETCORE_ENVIRONMENT=Staging`, forwarded-header support, loopback-independent container port, Azure Blob container name, and `RaceDay__AllowUnconfirmedReviewAccounts=true` **only in Staging**.

- API probe: `/healthz` means the process is alive; `/readyz` verifies the Identity SQL table is queryable.
- Portal probe: `/healthz` means the frontend process is alive. It does not prove the API works.
- API Swagger remains disabled outside Development.
- Deploy API first, verify HTTPS and `/readyz`; then point the portal's `RaCeDayApi__BaseUrl` to the actual API HTTPS URL and deploy portal.
- Configure the proxy's forwarded-header trust according to the hosting provider before production exposure. The cloud environment flag is intended for the trusted managed reverse proxy, not for arbitrary proxies.

## Functional smoke test (required before sending a live link)

1. Open portal over HTTPS on desktop and mobile; staging notice is visible.
2. Register a synthetic Organiser and Participant (Staging permits accounts without email confirmation; **Production never does**).
3. Organiser: create event/category; participant: browse/filter and enrol.
4. Organiser: record a result; participant: verify history.
5. Upload and retrieve an image from **actual private Azure Blob**, not Azurite.
6. Confirm restart/deploy behavior, sign-out, validation errors, role restrictions and secure cookies.
7. Confirm API responds with safe errors and no credentials in rendered HTML, logs or Git.
8. Re-run automated tests and source-branch CI against the exact commit being reviewed.

## Known limitations / production exit criteria

This is a **peer-review staging site**, not a finished public race platform:

- Real production requires a verified email sender and confirmation workflow; do not enable the staging bypass in Production.
- In-memory ASP.NET Session and default Data Protection key storage are not resilient to platform restarts/scale-out. Persist and protect keys and sessions using an approved shared store before real production accounts and multi-instance hosting. On free preview restarts, Sam may need to log in again.
- Rate limiting, backups, monitoring/alerts, privacy handling, secure domain, full Azure Blob integration, and third-party security testing require explicit verification.
- Azure SQL free allowances can pause the database when exhausted; Render's free instances can sleep. Neither has a production SLA for this demo.
- The assessment branch, student-recorded walkthrough, real Azure proof and ARC submission are separate school requirements. Cloud deployment is **not** evidence of completing those requirements.

## No secrets / no fake completion

Do not check in SQL passwords, storage access keys, Azure credentials, test-account passwords, Render tokens or actual private user data. No permanent cloud resources should be created without choosing the account and approving any costs. Give Sam the public website URL only after the full smoke test passes.
