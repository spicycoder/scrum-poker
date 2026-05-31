# Contributing

Thanks for your interest in contributing to Scrum Poker!

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) or [Podman](https://podman.io/) (for integration tests).
    > If using Podman, enable Docker compatibility.
- [pnpm](https://pnpm.io/) (frontend, coming soon)

## Getting Started

```bash
git clone https://github.com/spicycoder/scrum-poker.git
cd scrum-poker
./build.ps1
```

`build.ps1` restores tools, builds, runs all tests, and generates coverage reports to `./.coverage/`.

### Architecture

- **Clean Architecture**: Domain → Application → Infrastructure → API
- **CQRS**: Commands and queries via WolverineFx
- **Vertical slices**: `Features/{Name}/` folders with request DTO, validator, handler
- **Dual validation**: FluentValidation at API layer (DTOs) and Application layer (commands)

### Test Structure

| Project | What | Speed |
|---|---|---|
| `ScrumPoker.Domain.UnitTests` | Room aggregate logic | Fast |
| `ScrumPoker.Application.UnitTests` | Handlers, validators | Fast |
| `ScrumPoker.API.UnitTests` | Controller branches, request validators | Fast |
| `ScrumPoker.AppHost.IntegrationTests` | Full HTTP pipeline via Aspire | Slow |

- Per-endpoint test files (e.g., `CreateRoomEndpointTests.cs`)
- Expiry tests use short TTL fixtures (4s)

## Pull Requests

1. Fork the repo
2. Create a branch from `main`
3. Make your changes
4. Ensure `dotnet build` passes
5. Ensure `dotnet test` passes
6. Submit a PR

### Commit Messages

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add player kick endpoint
fix: prevent duplicate votes
test: add expiry tests for join endpoint
docs: update setup instructions
chore: bump dependencies
```

### Code Style

- Follow existing patterns in the codebase
- No warnings — treat warnings as errors
- Keep controllers thin — business logic in handlers
- Domain layer has zero external dependencies

## Reporting Issues

- Use [GitHub Issues](https://github.com/spicycoder/scrum-poker/issues)
- Include steps to reproduce
- Include .NET version (`dotnet --version`)

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
