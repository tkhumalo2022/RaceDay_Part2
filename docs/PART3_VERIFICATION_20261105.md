# RaceDay Part 3 — verified readiness snapshot (5 November 2026)

This is an evidence-backed progress note, **not** a claim that the academic submission is complete.

## Locally verified on 5 November
- Device: authorised Windows development PC; project `RaceDay_PROG6212_Part3_Development`.
- Branch: `feature/part3-mvc-review`; working tree was clean **before** this documentation was added.
- Commit inspected: `f0253380eda2cf134551e7e526336d946a6511ee`.
- `dotnet --version`: `10.0.403`.
- Command: `dotnet test RaCeDay.slnx --verbosity quiet --nologo`.
- Result: **58 passed, 0 failed, 0 skipped**; exit code 0. This is a local automated test result, not manual browser testing.
- Command: `scripts/part3-preflight.ps1` (read-only preflight, no optional build or test flags).
- Preflight: all 16 expected source/configuration/test/evidence paths **PASS**.
- `git rev-list --count main..HEAD`: **22** commits ahead of local `main`. The minimum count is met; every commit's meaningfulness remains subject to human assessment.

## GitHub Actions verified for that exact commit
- Workflow run: https://github.com/tkhumalo2022/RaceDay_Part2/actions/runs/41501949381
- Workflow: `Build and test RaceDay Parts 2 and 3`, run #72.
- Overall conclusion: **success**.
- All four jobs completed successfully: `build-and-test`, `compose-integration`, `blob-sdk-integration`, and `docker-portal`.
- Passing CI does not prove a local Docker installation, an authenticated user journey inside the complete deployed stack, a real Azure cloud upload, or production readiness.

## Important remaining tasks — do not mark complete without evidence
- [ ] Test on an actual mobile phone; check keyboard accessibility and screen-reader flow.
- [ ] Verify an authorised **real Azure Storage** image round trip. Azurite is only a local emulator.
- [ ] Validate a full authenticated workflow in the deployed/container environment, with appropriate HTTPS and cookies, if deployment is in scope.
- [ ] Record and personally explain the original student demonstration; add the real unlisted YouTube link.
- [ ] Review AI assistance disclosure and submission instructions with the lecturer.
- [ ] Submit via ARC personally and retain the actual confirmation.

## Preflight environment warnings
- Docker was not available as a command on this laptop.
- `AzureBlob__ConnectionString` was absent **in the inspected process environment**; this does not establish whether the user has authorised Azure access elsewhere.

## Integrity boundaries
No new real Azure, phone, browser, screen-reader or ARC submission claims are made here. This note records actual command output and the linked GitHub Actions result. Do not fabricate missing evidence.
