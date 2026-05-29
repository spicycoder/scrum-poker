# Tests

## Test Pyramid

```
                                /\
                               /  \
                              /    \
                             /      \
                            /        \
                           /          \
                          /            \
                         /              \
                        /      API       \
                       / IntegrationTests \
                      /       Aspire       \
                     /                      \
                    /       few · 🐌         \
                   /────────────────────────  \
                  /                            \
                 /                              \
                /       API.UnitTests            \
               /   controllers · validators       \
              /                🐎                  \
             /───────────────────────────────────── \
            /                                        \
           /                                          \
          /         Application.UnitTests              \
         /       handlers · domain logic                \
        /                many · 🚀                       \
       /__________________________________________________\
```

## Projects

| Project | Scope | What it tests | Speed |
|---|---|---|---|
| `ScrumPoker.Domain.UnitTests` | Unit | `Room` aggregate logic (pure domain, no mocks) | Fast |
| `ScrumPoker.Application.UnitTests` | Unit | Command/query handlers, command validators | Fast |
| `ScrumPoker.API.UnitTests` | Unit | `RoomController` (all status code branches), request validators, `GameStateMapper`, `GlobalExceptionHandler` | Fast |
| `ScrumPoker.AppHost.IntegrationTests` | Integration | Full HTTP pipeline via Aspire: controller -> handler -> Redis -> response, SignalR | Slow |
