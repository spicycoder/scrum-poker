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
| `ScrumPoker.Application.UnitTests` | Unit | `CreateRoomHandler`, `JoinRoomHandler`, `CreateRoomCommandValidator`, `JoinRoomCommandValidator` | Fast |
| `ScrumPoker.API.UnitTests` | Unit | `RoomController` (all status code branches), `CreateRoomRequestValidator`, `JoinRoomRequestValidator` | Fast |
| `ScrumPoker.API.IntegrationTests` | Integration | Full HTTP pipeline via Aspire: controller -> handler -> Redis -> response | Slow |
