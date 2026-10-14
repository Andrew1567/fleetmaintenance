#!/usr/bin/env bash
# Локальний еквівалент конвеєра CI: ті самі кроки, що в .github/workflows/ci.yml.
set -euo pipefail
dotnet restore FleetMaintenance.sln
dotnet format FleetMaintenance.sln --verify-no-changes --no-restore
dotnet build FleetMaintenance.sln -c Release --no-restore -warnaserror
dotnet test FleetMaintenance.sln -c Release --no-build \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=../../coverage/
