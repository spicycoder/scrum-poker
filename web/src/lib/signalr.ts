import { useEffect, useRef } from 'react'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useNavigate } from 'react-router-dom'
import { getGameState, type GameStateResponse } from './api'

export function useGameHub(
  roomId: string | undefined,
  playerName: string,
  onStateUpdate: (state: GameStateResponse) => void,
  onVotesReset: () => void,
) {
  const navigate = useNavigate()
  const stateRef = useRef(onStateUpdate)
  stateRef.current = onStateUpdate
  const resetRef = useRef(onVotesReset)
  resetRef.current = onVotesReset
  const navRef = useRef(navigate)
  navRef.current = navigate

  useEffect(() => {
    if (!roomId || !playerName) return

    const id = parseInt(roomId, 10)
    if (isNaN(id)) return

    let active = true

    const connection = new HubConnectionBuilder()
      .withUrl('/api/hub')
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Information)
      .build()

    connection.on('RoomCreated', (data: GameStateResponse) => { if (active) stateRef.current(data) })
    connection.on('PlayerJoined', (data: GameStateResponse) => { if (active) stateRef.current(data) })
    connection.on('PlayerVoted', (data: GameStateResponse) => { if (active) stateRef.current(data) })
    connection.on('VotesRevealed', (data: GameStateResponse) => { if (active) stateRef.current(data) })
    connection.on('VotesReset', (data: GameStateResponse) => {
      if (!active) return
      stateRef.current(data)
      resetRef.current()
    })
    connection.on('PlayerLeft', (data: GameStateResponse) => { if (active) stateRef.current(data) })

    connection.onreconnected(async () => {
      if (!active) return
      try {
        await connection.invoke('JoinRoom', roomId, playerName)
      } catch {
        if (active) navRef.current(`/join/${id}`, { replace: true })
        return
      }
      try {
        const state = await getGameState(id)
        if (active) stateRef.current(state)
      } catch { /* skip */ }
    })

    connection.start()
      .then(() => { if (active) return connection.invoke('JoinRoom', roomId, playerName) })
      .catch(() => { if (active) navRef.current(`/join/${id}`, { replace: true }) })

    return () => {
      active = false
      connection.invoke('LeaveRoom', roomId)
        .finally(() => connection.stop())
    }
  }, [roomId, playerName])
}
