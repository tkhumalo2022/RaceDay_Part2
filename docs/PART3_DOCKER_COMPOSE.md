# RaceDay — one-command local Docker assessment demo

This is an **opt-in, local Development-only** integration stack. It contains SQL Server Developer, an Azure-compatible **Azurite emulator**, the Part 2 REST API and the separate Part 3 MVC portal. It is not a public production deployment or real Azure cloud validation.

## Requirements

- Docker Desktop with Linux containers and Docker Compose v2
- Enough local RAM/storage for SQL Server 2022 and .NET containers
- An **untracked** strong SQL Server demo SA password (minimum 12 characters; upper/lowercase, digit and symbol)
- For the student's 8 GB laptop, the lighter Windows + SQL Server + Azurite path may run better. GitHub Actions checks the image builds even when Docker Desktop is unavailable locally.

## Start (PowerShell, from repository root)

One-time per terminal session:

```powershell
$env:RACEDAY_SA_PASSWORD = Read-Host 'Enter a strong local-only SQL demo password'
```

Then launch the entire isolated demonstration stack:

```powershell
docker compose -f compose.yaml up --build -d
```

Open the MVC portal at **http://localhost:8080**. The backend API's development-only Swagger page is **http://localhost:8081/swagger**. SQL Server and Azurite are only reachable by other containers, not published externally. Both browser-facing endpoints bind to the local loopback interface.

The API container waits for SQL Server, then uses an explicitly Development-only `--init-demo-db` command to migrate the **isolated `RaceDayPart3Compose` database** and seed role names. It does not create an Admin account and does not change an existing Part 2 database. The command stops after bounded retries if the database remains inaccessible.

## Check

```powershell
docker compose -f compose.yaml ps
docker compose -f compose.yaml logs --tail 80 api
docker compose -f compose.yaml logs --tail 60 portal
```

Register as Organiser, create an event and category, sign out, register as Participant, enrol, then publish a result. Upload and retrieve an image to demonstrate the portal -> API -> private Azurite Blob container route.

**Important:** SQL/Blob emulator smoke checks and real Azure cloud storage are different evidence. Record which environment was actually used. The default Azurite key in the Compose file is Microsoft's **public testing key** and is unsuitable for production. Never put real Azure secrets, real passwords or personally identifying test data in source control.

## Stop / cleanup

```powershell
docker compose -f compose.yaml down
```

This leaves isolated named volumes for future testing. If and **only if** you want to delete this stack's QA data, use:

```powershell
docker compose -f compose.yaml down -v
```

**Warning:** `down -v` permanently deletes the Compose demo SQL and Blob volume data. Do not use it against other stacks, client data or the existing Part 2 database.

## QA and limitations

- GitHub Actions builds both Dockerfiles, validates the Compose YAML and verifies MVC sign-in/register HTML. The additional PR-only `compose-integration` check starts the isolated four-service stack and checks SQL migrations, API Swagger and the portal login route (`scripts/ci-compose-smoke.sh`). This is a real container-stack **startup** check, not an authenticated end-to-end workflow or live Azure cloud proof. Check the latest Actions result before citing a pass.
- Identity cookies are proxied through MVC for same-host local development. This stack is **not hardened for production**: a real deployment must use HTTPS, a reverse proxy, consistent cookie/data-protection key strategy, trusted origin/routing and storage secrets.
- The local Azure emulator does not establish that an actual Azure Storage account is provisioned.
- This configuration is for assessed local development, not for hosting real students, user accounts, or public data.
