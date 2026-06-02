import { HubConnectionBuilder, LogLevel, HubConnection } from '@microsoft/signalr'
import type { GameStateResponse } from './api'

const GAME_EVENTS = [
  'RoomCreated',
  'PlayerJoined',
  'PlayerVoted',
  'VotesRevealed',
  'VotesReset',
  'PlayerLeft',
] as const

export type GameEventName = typeof GAME_EVENTS[number]
export type GameEventHandler = (event: GameEventName, state: GameStateResponse) => void

export function createConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl('/api/hub')
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()
}

export function registerGameStateHandlers(
  connection: HubConnection,
  handler: GameEventHandler,
): void {
  for (const event of GAME_EVENTS) {
    connection.on(event, (payload: GameStateResponse) => {
      handler(event, payload)
    })
  }
}

export async function joinRoomGroup(connection: HubConnection, roomId: number, playerName: string): Promise<void> {
  await connection.invoke('JoinRoom', roomId.toString(), playerName)
}

export async function leaveRoomGroup(connection: HubConnection, roomId: number): Promise<void> {
  await connection.invoke('LeaveRoom', roomId.toString())
}
