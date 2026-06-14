param(
  [switch]$Images
)

$ErrorActionPreference = 'Stop'

if ($Images) {
  # Build container images — skip tests
  $containerCmd = if (Get-Command docker -ErrorAction SilentlyContinue) { "docker" } else { "podman" }

  dotnet publish src/ScrumPoker.API /t:PublishContainer -c Release `
    -p ContainerRepository=ghcr.io/spicycoder/scrumpoker-api `
    -p ContainerImageTag=latest `
    -p ContainerRuntimeIdentifier=linux-x64

  & $containerCmd build -f web/Dockerfile -t ghcr.io/spicycoder/scrumpoker-web:latest web/

  Write-Host "Images built: ghcr.io/spicycoder/scrumpoker-api:latest, ghcr.io/spicycoder/scrumpoker-web:latest" -ForegroundColor Green
} else {
  # .NET build + test + coverage
  dotnet clean -c Release ./ScrumPoker.slnx
  dotnet tool restore
  dotnet restore ./ScrumPoker.slnx
  dotnet build --no-restore -c Release ./ScrumPoker.slnx

  # Frontend build
  Push-Location web
  pnpm install --frozen-lockfile
  pnpm run build
  Pop-Location

  if (Test-Path ./.coverage) { Remove-Item ./.coverage -Recurse -Force }
  New-Item -ItemType Directory -Path ./.coverage | Out-Null

  dotnet dotnet-coverage collect `
      --settings ./coverage.runsettings `
      --output ./.coverage/coverage.cobertura.xml `
      --output-format cobertura `
      "dotnet test --no-build -c Release ./ScrumPoker.slnx"

  dotnet reportgenerator `
      "-reports:./.coverage/coverage.cobertura.xml" `
      "-targetdir:./.coverage" `
      -reporttypes:"Html_Dark;Badges"
}
