FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY Directory.Build.props Directory.Packages.props nuget.config ./
COPY src/ScrumPoker.API/ScrumPoker.API.csproj src/ScrumPoker.API/
COPY src/ScrumPoker.Application/ScrumPoker.Application.csproj src/ScrumPoker.Application/
COPY src/ScrumPoker.Domain/ScrumPoker.Domain.csproj src/ScrumPoker.Domain/
COPY src/ScrumPoker.Infrastructure/ScrumPoker.Infrastructure.csproj src/ScrumPoker.Infrastructure/
COPY src/ScrumPoker.Persistence/ScrumPoker.Persistence.csproj src/ScrumPoker.Persistence/
COPY src/ScrumPoker.ServiceDefaults/ScrumPoker.ServiceDefaults.csproj src/ScrumPoker.ServiceDefaults/
RUN dotnet restore src/ScrumPoker.API/ScrumPoker.API.csproj

COPY . .
RUN dotnet publish src/ScrumPoker.API/ScrumPoker.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1
CMD ASPNETCORE_URLS="http://0.0.0.0:${PORT:-8080}" exec dotnet ScrumPoker.API.dll
