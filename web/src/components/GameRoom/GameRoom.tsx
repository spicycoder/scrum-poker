import { useEffect, useState } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { UserCheck } from 'lucide-react'
import { toast } from 'sonner'
import { Card, CardContent } from '../ui/card'
import { Button } from '../ui/button'
import { Badge } from '../ui/badge'
import GooeyNav from '../GooeyNav/GooeyNav'
import ShinyText from '../ShinyText/ShinyText'
import { getGameState, castVote, revealVotes, resetVotes, ApiError, type GameStateResponse } from '../../lib/api'
import { useGameHub } from '../../lib/signalr'
import { useGameNotifications } from '../../hooks/useGameNotifications'

export default function GameRoom() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()
  const [state, setState] = useState<GameStateResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [activeIndex, setActiveIndex] = useState(-1)
  const [voting, setVoting] = useState(false)
  const [revealing, setRevealing] = useState(false)
  const [resetting, setResetting] = useState(false)
  const playerName = localStorage.getItem('playerName') ?? ''

  useGameHub(
    roomId,
    playerName,
    setState,
    () => setActiveIndex(-1),
  )

  useGameNotifications(state, playerName)

  useEffect(() => {
    if (!roomId) return
    const id = parseInt(roomId, 10)
    if (isNaN(id)) {
      navigate('/join', { replace: true })
      return
    }

    getGameState(id)
      .then((data) => {
        setState(data)
        const myVote = data.players[playerName]
        if (myVote != null) {
          const idx = data.cardSet.indexOf(myVote)
          if (idx !== -1) setActiveIndex(idx)
        }
      })
      .catch((err) => {
        if (err instanceof ApiError && err.status === 404) {
          navigate(`/join/${id}`, { replace: true })
        }
      })
      .finally(() => setLoading(false))
  }, [roomId, playerName, navigate])

  async function handleVote(value: string, index: number) {
    if (!roomId || voting) return
    setVoting(true)
    try {
      await castVote(parseInt(roomId, 10), playerName, value)
      setState(prev => prev ? { ...prev, players: { ...prev.players, [playerName]: value } } : prev)
      setActiveIndex(index)
    } catch (err) {
      console.error(err)
    } finally {
      setVoting(false)
    }
  }

  async function handleReveal() {
    if (!roomId || revealing) return
    setRevealing(true)
    try {
      await revealVotes(parseInt(roomId, 10))
      setState(prev => prev ? { ...prev, revealed: true } : prev)
    } catch (err) {
      console.error(err)
    } finally {
      setRevealing(false)
    }
  }

  async function handleReset() {
    if (!roomId || resetting) return
    setResetting(true)
    try {
      await resetVotes(parseInt(roomId, 10))
      setActiveIndex(-1)
      setState(prev => prev ? {
        ...prev,
        players: Object.fromEntries(Object.entries(prev.players).map(([k]) => [k, null])),
        revealed: false,
      } : prev)
    } catch (err) {
      console.error(err)
    } finally {
      setResetting(false)
    }
  }

  async function handleCopy() {
    await navigator.clipboard.writeText(`${window.location.origin}/join/${roomId}`)
    toast('Link copied')
  }

  if (loading) {
    return (
      <div className="flex justify-center px-6 pt-8">
        <p className="text-muted-foreground">Connecting...</p>
      </div>
    )
  }

  if (!state) {
    return (
      <div className="flex justify-center px-6 pt-8">
        <p className="text-muted-foreground">Room not found</p>
      </div>
    )
  }

  const { players, revealed, cardSet } = state

  return (
    <div className="flex flex-col items-center gap-6 px-6 pt-8">
      <button
        onClick={handleCopy}
        className="text-sm text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
      >
        🔗 {window.location.origin}/join/{roomId}
      </button>

      <Card className="w-full max-w-md">
        <CardContent className="p-6">
          <h3 className="text-lg font-bold mb-2">Players</h3>
          <div className="space-y-2 mb-6">
            {Object.keys(players).length === 0 && (
              <div className="flex items-center justify-between">
                <span className="flex items-center gap-1.5">
                  <ShinyText text={playerName} />
                  <Badge variant="secondary" className="size-5 p-0 items-center justify-center">
                    <UserCheck className="size-3" />
                  </Badge>
                </span>
                <span className="text-muted-foreground">-</span>
              </div>
            )}
            {Object.entries(players).map(([name, value]) => (
              <div key={name} className="flex items-center justify-between">
                {name === playerName ? (
                  <span className="flex items-center gap-1.5">
                    <ShinyText text={name} />
                    <Badge variant="secondary" className="size-5 p-0 items-center justify-center">
                      <UserCheck className="size-3" />
                    </Badge>
                  </span>
                ) : (
                  <span>{name}</span>
                )}
                <span className="text-muted-foreground">
                  {revealed ? (value ?? '-') : value ? '✓' : '-'}
                </span>
              </div>
            ))}
          </div>

          {!revealed && cardSet.length > 0 && (
            <div className="mb-4">
              <GooeyNav
                items={cardSet.map((card, idx) => ({
                  label: card,
                  onClick: () => handleVote(card, idx),
                }))}
                activeIndex={activeIndex}
                particleCount={10}
                particleDistances={[60, 8]}
                particleR={80}
                animationTime={400}
                timeVariance={200}
                colors={[1, 2, 3, 4]}
              />
            </div>
          )}

          <div className="flex gap-3">
            {!revealed && (
              <Button
                className="flex-1"
                disabled={revealing || !Object.values(players).some(v => v !== null)}
                onClick={handleReveal}
              >
                {revealing ? 'Revealing...' : 'Reveal'}
              </Button>
            )}
            <Button
              className={revealed ? 'w-full' : 'flex-1'}
              variant="outline"
              disabled={resetting || !Object.values(players).some(v => v !== null)}
              onClick={handleReset}
            >
              {resetting ? 'Resetting...' : 'Reset'}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
