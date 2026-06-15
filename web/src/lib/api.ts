const BASE = import.meta.env.VITE_API_URL ?? ''

export class ApiError extends Error {
  status: number

  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export interface GameStateResponse {
  gameId: number
  players: Record<string, string | null>
  revealed: boolean
  cardSet: string[]
}

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  })

  if (!res.ok) {
    throw new ApiError(res.statusText, res.status)
  }

  const location = res.headers.get('Location')
  if (location) {
    const id = parseInt(location.split('/').pop() ?? '', 10)
    if (!isNaN(id)) return id as unknown as T
  }

  const text = await res.text()
  return text ? JSON.parse(text) : (undefined as T)
}

export async function createRoom(
  playerName: string,
  cardSet: string[],
): Promise<number> {
  return request('/api/rooms', {
    method: 'POST',
    body: JSON.stringify({ playerName, cardSet }),
  })
}

export async function getGameState(roomId: number): Promise<GameStateResponse> {
  return request(`/api/rooms/${roomId}`)
}

export async function joinRoom(
  roomId: number,
  playerName: string,
): Promise<void> {
  return request(`/api/rooms/${roomId}/join`, {
    method: 'POST',
    body: JSON.stringify({ playerName }),
  })
}

export async function castVote(
  roomId: number,
  playerName: string,
  value: string,
): Promise<void> {
  return request(`/api/rooms/${roomId}/vote`, {
    method: 'POST',
    body: JSON.stringify({ playerName, value }),
  })
}

export async function revealVotes(roomId: number): Promise<void> {
  return request(`/api/rooms/${roomId}/reveal`, {
    method: 'POST',
  })
}

export async function resetVotes(roomId: number): Promise<void> {
  return request(`/api/rooms/${roomId}/reset`, {
    method: 'POST',
  })
}

export interface MonthlyStatRow {
  monthKey: string
  gameCount: number
  playerCount: number
}

export interface StatsResponse {
  monthlyStats: MonthlyStatRow[]
  currentMonth: MonthlyStatRow
}

export async function getStats(): Promise<StatsResponse> {
  return request('/api/stats')
}
