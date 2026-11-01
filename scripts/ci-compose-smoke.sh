#!/usr/bin/env bash
# GitHub Actions-only smoke test of the four-service local-development stack.
set -euo pipefail
for attempt in $(seq 1 90); do
  if curl -fsS --max-time 3 http://127.0.0.1:8081/swagger/v1/swagger.json -o /tmp/raceday-compose-swagger.json &&
     curl -fsS --max-time 6 http://127.0.0.1:8080/Account/Login -o /tmp/raceday-compose-login.html; then
    if grep -q '"openapi"' /tmp/raceday-compose-swagger.json &&
       grep -qi 'Sign in' /tmp/raceday-compose-login.html &&
       docker compose logs --no-color api | grep -q 'RaceDay local Docker demo database migrated'; then
      echo 'PASS: SQL database migrations completed, API served Swagger, and MVC portal served login through the full Compose stack.'
      docker compose ps
      exit 0
    fi
  fi
  sleep 4
done
echo 'FAIL: four-service Compose stack did not complete database/API/MVC startup.' >&2
docker compose ps >&2 || true
docker compose logs --no-color --tail 60 api portal >&2 || true
exit 1
