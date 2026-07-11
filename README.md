# ♠️ Scrum Poker

♠️ Online ♦️ Real-time ♣️ Multi-player ♥️ Game

Agile estimation cards — real-time voting, no login required.

> The public instance is for demo purposes only. Self-host for production use.

| Build | Alive |
| --- | --- |
| [![CI](https://github.com/spicycoder/scrum-poker/actions/workflows/ci.yml/badge.svg)](https://github.com/spicycoder/scrum-poker/actions/workflows/ci.yml) | [![API Status](https://img.shields.io/github/actions/workflow/status/spicycoder/scrum-poker/keepalive.yml?label=api%20status)](https://github.com/spicycoder/scrum-poker/actions/workflows/keepalive.yml) |

---

## Coverage

| Total Coverage % | Unit Tests # | Integration Tests # |
| --- | --- | --- |
| [![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/spicycoder/eca658a13dc5df0c2abb154ae9aeb820/raw/coverage.json)](https://github.com/spicycoder/scrum-poker/actions) | ![Unit Tests](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/spicycoder/eca658a13dc5df0c2abb154ae9aeb820/raw/unit.json) | ![Integration Tests](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/spicycoder/eca658a13dc5df0c2abb154ae9aeb820/raw/integration.json) |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (22.x) + [pnpm](https://pnpm.io/installation)
- [Docker](https://www.docker.com/) or [Podman](https://podman.io/) (for integration tests). If using Podman, enable Docker compatibility.

## Getting Started

```bash
git clone https://github.com/spicycoder/scrum-poker.git
cd scrum-poker
./build.ps1
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

## Acknowledgements

Thanks to [Render](https://render.com)
for providing a free tier that makes side projects like this possible.

## Supplementary Sites

- [Storybook](https://scrumpoker-sb.onrender.com)
