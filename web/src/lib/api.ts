export interface GameStateResponse {
  gameId: number
  players: Record<string, string | null>
  revealed: boolean
  cardSet: string[]
}

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message)
  }
}

function extractRoomId(response: Response): number {
  const location = response.headers.get('Location')
  if (!location) throw new Error('No Location header in response')
  const match = location.match(/\/(\d+)$/)
  if (!match) throw new Error(`Cannot parse roomId from: ${location}`)
  return parseInt(match[1], 10)
}

export async function createRoom(playerName: string, cardSet: string[]): Promise<number> {
  const res = await fetch('/api/rooms', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerName, cardSet }),
  })
  if (!res.ok) throw new Error(`Create room failed: ${res.status}`)
  return extractRoomId(res)
}

export async function joinRoom(roomId: number, playerName: string): Promise<void> {
  const res = await fetch(`/api/rooms/${roomId}/join`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerName }),
  })
  if (!res.ok) throw new ApiError(res.status, `Join room failed: ${res.status}`)
}

export async function getGameState(roomId: number): Promise<GameStateResponse> {
  const res = await fetch(`/api/rooms/${roomId}`)
  if (!res.ok) throw new Error(`Get game state failed: ${res.status}`)
  return res.json()
}

export async function castVote(roomId: number, playerName: string, value: string): Promise<void> {
  const res = await fetch(`/api/rooms/${roomId}/vote`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerName, value }),
  })
  if (!res.ok) throw new Error(`Cast vote failed: ${res.status}`)
}

export async function revealVotes(roomId: number): Promise<void> {
  const res = await fetch(`/api/rooms/${roomId}/reveal`, { method: 'POST' })
  if (!res.ok) throw new Error(`Reveal votes failed: ${res.status}`)
}

export async function resetVotes(roomId: number): Promise<void> {
  const res = await fetch(`/api/rooms/${roomId}/reset`, { method: 'POST' })
  if (!res.ok) throw new Error(`Reset votes failed: ${res.status}`)
}

export async function leaveRoom(roomId: number, playerName: string): Promise<void> {
  const res = await fetch(`/api/rooms/${roomId}/leave`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ playerName }),
  })
  if (!res.ok) throw new Error(`Leave room failed: ${res.status}`)
}
