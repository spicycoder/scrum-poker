# Endpoints

## Create Room

`POST /rooms`

Creates a new scrum poker room and adds the creator as the first player.

### Sequence

```mermaid
sequenceDiagram
    participant C as Client
    participant API as API
    participant R as Redis

    C->>API: POST /rooms
    alt Validation failed
        API-->>C: 400 Bad Request
    else OK
        API->>R: Save new room
        R-->>API: OK
        API-->>C: 201 Created
        Note over API,C: SignalR (async, server push)
        API-)C: GameStateResponse
    end
```

### PlayerName (body)

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| PlayerName | string | yes | max 50 chars |

| Status Code | Description |
|-------------|-------------|
| 201 | Room created |
| 400 | Validation failed |

## Join Room

### Sequence

`POST /rooms/{id}/join`

Joins an existing room as a player.

```mermaid
sequenceDiagram
    participant C as Client
    participant All as All Clients
    participant API as API
    participant R as Redis

    C->>API: POST /rooms/{id}/join
    alt Validation failed
        API-->>C: 400 Bad Request
    else Room not found
        API->>R: Get room
        R-->>API: null
        API-->>C: 404 Not Found
    else Player already in room
        API->>R: Get room
        R-->>API: room
        API-->>C: 409 Conflict
    else OK
        API->>R: Get room
        R-->>API: room
        API->>R: Save updated room
        R-->>API: OK
        API-->>C: 200 OK
        Note over API,C: SignalR (async, server push)
        API-)All: GameStateResponse
    end
```

### id (path)

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| id | int | yes | must be > 0 |

### PlayerName (body)

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| PlayerName | string | yes | max 50 chars |

| Status Code | Description |
|-------------|-------------|
| 200 | Joined successfully |
| 400 | Validation failed |
| 404 | Room not found |
| 409 | Player already in room |

---
