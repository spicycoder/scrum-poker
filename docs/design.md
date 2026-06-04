# Design

## High Level Design

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    Client

    Client -->|HTTPS| API["API (single instance)"]

    API <-->|read / write| Redis[(Redis)]
    API <-->|backplane| Redis
    Redis -->|fan-out| API
    API -->|SignalR push| Client
```

## Low Level Design

```mermaid
%%{init: {'flowchart': {'curve': 'linear'}}}%%
flowchart TD
    Client

    Client -->|HTTPS + GET body| API

    API -->|Wolverine dispatch| Application

    Application -->|read / write| Persistence
    Application -->|notify| Infrastructure

    Persistence -->|query / mutate| Redis[(Redis)]
    Infrastructure -->|backplane| Redis
    Redis -->|fan-out| Infrastructure
    Infrastructure -->|SignalR push| Client

    subgraph ScrumPoker
        API["API\nControllers → IMessageBus"]
        Application["Application\nCommand / query handlers"]
        Persistence["Persistence\nRedis read / write"]
        Infrastructure["Infrastructure\nSignalR hub"]
    end
```
