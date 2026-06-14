dotnet tool update --all
dotnet aspire update
dotnet package update --all

# Frontend dependencies
Push-Location web
pnpm update
Pop-Location
