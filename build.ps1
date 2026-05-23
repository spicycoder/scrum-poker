param(
    [string]$Configuration = "Release",
    [switch]$SkipTests,
    [switch]$SkipCoverage
)

$ErrorActionPreference = 'Stop'

dotnet clean -c $Configuration ./ScrumPoker.slnx
dotnet tool restore
dotnet restore ./ScrumPoker.slnx
dotnet build --no-restore -c $Configuration ./ScrumPoker.slnx

if (-not $SkipTests) {
    if ($SkipCoverage) {
        dotnet test --no-build -c $Configuration ./ScrumPoker.slnx
    } else {
        dotnet test --no-build -c $Configuration ./ScrumPoker.slnx --collect:"XPlat Code Coverage" --settings ./coverage.runsettings
        dotnet reportgenerator "-reports:./**/coverage.cobertura.xml" "-targetdir:./.coverage" -reporttypes:"Html_Dark;SonarQube"
    }
}