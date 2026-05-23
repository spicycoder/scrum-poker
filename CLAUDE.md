# Scrum Poker

Online, real-time, multiplayer Scrum Poker game.

## Intentions (not obvious from code)

- **Redis dual role** — Used for volatile game state AND SignalR backplane. Backplane not yet configured.
- **Aspire.Hosting.Testing** — Integration tests use Aspire orchestration, NOT TestContainers.
- **Coverage** — All 3 test projects contribute to same coverage report. Int tests included.
- **build.ps1** — Local runs only (don't wait for CI). Coverage goes to `./.coverage/`.
