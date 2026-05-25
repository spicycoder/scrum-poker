$ErrorActionPreference = 'Stop'

dotnet clean -c Release ./ScrumPoker.slnx
dotnet tool restore
dotnet restore ./ScrumPoker.slnx
dotnet build --no-restore -c Release ./ScrumPoker.slnx

if (Test-Path ./.coverage) { Remove-Item ./.coverage -Recurse -Force }
New-Item -ItemType Directory -Path ./.coverage | Out-Null

# dotnet-coverage wraps the entire test run and attaches the profiler to
# child processes spawned by Aspire.Hosting.Testing (API, Redis-side .NET, etc.),
# so integration tests contribute to coverage too.
dotnet dotnet-coverage collect `
    --settings ./coverage.runsettings `
    --output ./.coverage/coverage.cobertura.xml `
    --output-format cobertura `
    "dotnet test --no-build -c Release ./ScrumPoker.slnx"

dotnet reportgenerator `
    "-reports:./.coverage/coverage.cobertura.xml" `
    "-targetdir:./.coverage" `
    -reporttypes:"Html_Dark;Badges"
