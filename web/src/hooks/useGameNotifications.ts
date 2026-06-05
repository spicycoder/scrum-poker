import { useEffect, useRef } from 'react'
import { toast } from 'sonner'
import type { GameStateResponse } from '../lib/api'

export function useGameNotifications(
  state: GameStateResponse | null,
  currentPlayer: string,
) {
  const prevRef = useRef<GameStateResponse | null>(null)
  const initializedRef = useRef(false)

  useEffect(() => {
    if (!state) {
      prevRef.current = null
      initializedRef.current = false
      return
    }

    if (!initializedRef.current) {
      initializedRef.current = true
      prevRef.current = state
      return
    }

    const prev = prevRef.current
    if (!prev) {
      prevRef.current = state
      return
    }

    const prevPlayers = prev.players
    const currPlayers = state.players

    for (const name of Object.keys(currPlayers)) {
      if (!(name in prevPlayers) && name !== currentPlayer) {
        toast(`${name} joined`)
      }
    }

    for (const name of Object.keys(prevPlayers)) {
      if (!(name in currPlayers)) {
        toast(`${name} left`)
      }
    }

    if (!state.revealed) {
      for (const [name, value] of Object.entries(currPlayers)) {
        if (name !== currentPlayer && value !== null && prevPlayers[name] !== value) {
          toast(`${name} voted`)
        }
      }
    }

    if (state.revealed && !prev.revealed) {
      toast('Votes revealed')
    } else if (!state.revealed && prev.revealed) {
      toast('Votes reset')
    }

    if (!state.revealed && !prev.revealed) {
      const prevSomeNonNull = Object.values(prevPlayers).some(v => v !== null)
      const currAllNull = Object.values(currPlayers).every(v => v === null)
      if (prevSomeNonNull && currAllNull) {
        toast('Votes reset')
      }
    }

    prevRef.current = state
  }, [state, currentPlayer])
}
