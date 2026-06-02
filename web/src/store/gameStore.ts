import { create } from 'zustand'
import { toast } from 'sonner'
import type { GameStateResponse } from '../lib/api'
import * as api from '../lib/api'
import {
  createConnection,
  registerGameStateHandlers,
  joinRoomGroup,
  leaveRoomGroup,
} from '../lib/signalr'
import type { HubConnection } from '@microsoft/signalr'

type ConnectionStatus = 'connecting' | 'connected' | 'disconnected'

interface GameState {
  gameId: number | null
  players: Record<string, string | null>
  revealed: boolean
  cardSet: string[]
  currentPlayerName: string
  connectionStatus: ConnectionStatus
  connection: HubConnection | null
}

interface GameActions {
  connect: (roomId: number) => Promise<void>
  disconnect: () => Promise<void>
  updateGameState: (state: GameStateResponse) => void
  setCurrentPlayerName: (name: string) => void
  castVote: (value: string) => Promise<void>
  revealVotes: () => Promise<void>
  resetVotes: () => Promise<void>
  leaveRoom: () => Promise<void>
}

function applyGameState(_state: GameState, payload: GameStateResponse): Partial<GameState> {
  return {
    gameId: payload.gameId,
    players: payload.players,
    revealed: payload.revealed,
    cardSet: payload.cardSet,
  }
}

export const useGameStore = create<GameState & GameActions>((set, get) => ({
  gameId: null,
  players: {},
  revealed: false,
  cardSet: [],
  currentPlayerName: localStorage.getItem('playerName') ?? '',
  connectionStatus: 'disconnected',
  connection: null,

  setCurrentPlayerName: (name) => set({ currentPlayerName: name }),

  updateGameState: (payload) => set((state) => applyGameState(state, payload)),

  connect: async (roomId) => {
    set({ currentPlayerName: localStorage.getItem('playerName') ?? '' })
    const existing = get().connection
    if (existing) {
      try { await existing.stop() } catch { /* ignore */ }
    }

    set({ connectionStatus: 'connecting' })

    const connection = createConnection()
    registerGameStateHandlers(connection, (event, payload) => {
      const prevPlayers = get().players
      get().updateGameState(payload)

      if (event === 'PlayerJoined') {
        const newPlayers = Object.keys(payload.players).filter(n => !(n in prevPlayers))
        if (newPlayers.length > 0 && newPlayers[0] !== get().currentPlayerName) {
          toast.info(`${newPlayers[0]} joined`)
        }
      } else if (event === 'PlayerLeft') {
        const leftPlayers = Object.keys(prevPlayers).filter(n => !(n in payload.players))
        if (leftPlayers.length > 0) {
          toast.info(`${leftPlayers[0]} left`)
        }
      } else if (event === 'PlayerVoted') {
        const voted = Object.keys(payload.players).filter(
          n => prevPlayers[n] === null && payload.players[n] !== null
        )
        if (voted.length > 0) {
          toast.info(`${voted[0]} voted`)
        }
      } else if (event === 'VotesRevealed') {
        toast.info('Votes revealed')
      } else if (event === 'VotesReset') {
        toast.info('Votes reset')
      }
    })

    connection.onreconnecting(() => set({ connectionStatus: 'connecting' }))
    connection.onreconnected(() => set({ connectionStatus: 'connected' }))
    connection.onclose(() => set({ connectionStatus: 'disconnected' }))

    await connection.start()
    await joinRoomGroup(connection, roomId, get().currentPlayerName)

    set({ connection, connectionStatus: 'connected' })

    // Fetch initial state
    const gameState = await api.getGameState(roomId)
    get().updateGameState(gameState)
  },

  disconnect: async () => {
    const { connection } = get()
    if (connection) {
      try {
        await connection.stop()
      } catch { /* ignore */ }
    }
    set({
      gameId: null,
      players: {},
      revealed: false,
      cardSet: [],
      connection: null,
      connectionStatus: 'disconnected',
    })
  },

  castVote: async (value) => {
    const { gameId, currentPlayerName } = get()
    if (!gameId) return
    await api.castVote(gameId, currentPlayerName, value)
  },

  revealVotes: async () => {
    const { gameId } = get()
    if (!gameId) return
    await api.revealVotes(gameId)
  },

  resetVotes: async () => {
    const { gameId } = get()
    if (!gameId) return
    await api.resetVotes(gameId)
  },

  leaveRoom: async () => {
    const { gameId, currentPlayerName, connection } = get()
    if (gameId && connection) {
      try {
        await leaveRoomGroup(connection, gameId)
        await api.leaveRoom(gameId, currentPlayerName)
      } catch { /* ignore */ }
    }
    await get().disconnect()
  },
}))
