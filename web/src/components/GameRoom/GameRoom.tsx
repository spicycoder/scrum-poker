import { useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import { useGameStore } from '../../store/gameStore'
import { Card, CardContent } from '../ui/card'
import { Button } from '../ui/button'
import GooeyNav from '../GooeyNav/GooeyNav'

export default function GameRoom() {
  const { roomId } = useParams<{ roomId: string }>()
  const navigate = useNavigate()
  const {
    gameId,
    players,
    revealed,
    cardSet,
    currentPlayerName,
    connectionStatus,
    connect,
    disconnect,
  } = useGameStore()

  useEffect(() => {
    if (!roomId) return
    const id = parseInt(roomId, 10)
    if (isNaN(id)) {
      navigate('/')
      return
    }
    connect(id).then(() => {
      const { currentPlayerName, players } = useGameStore.getState()
      if (currentPlayerName && !(currentPlayerName in players)) {
        localStorage.removeItem('playerName')
        disconnect()
        navigate(`/${roomId}`)
      }
    }).catch(() => {
      toast.error('Failed to connect')
      navigate('/')
    })

    return () => {
      disconnect()
    }
  }, [roomId]) // eslint-disable-line react-hooks/exhaustive-deps

  async function handleCopy() {
    await navigator.clipboard.writeText(window.location.href)
    localStorage.removeItem('playerName')
    toast.success('Link copied to clipboard')
  }

  if (connectionStatus !== 'connected' || gameId === null) {
    return (
      <div className="flex justify-center px-6 pt-8">
        <p className="text-muted-foreground">Connecting...</p>
      </div>
    )
  }

  return (
    <div className="flex flex-col items-center gap-6 px-6 pt-8">
      <button
        onClick={handleCopy}
        className="text-sm text-muted-foreground hover:text-foreground transition-colors cursor-pointer"
      >
        🔗 {window.location.href}
      </button>

      <Card className="w-full max-w-md">
        <CardContent className="p-6">
          <h3 className="text-lg font-bold mb-2">Players</h3>
          <div className="space-y-2 mb-6">
            {Object.entries(players).map(([name, value]) => (
              <div key={name} className="flex items-center justify-between">
                <span>
                  {name} {name === currentPlayerName && '(you)'}
                </span>
                <span className="text-muted-foreground">
                  {revealed ? (value ?? '-') : value ? '✓' : '-'}
                </span>
              </div>
            ))}
          </div>

          {!revealed && cardSet.length > 0 && (
            <div className="mb-4">
              <GooeyNav
                items={cardSet.map((card) => ({
                  label: card,
                  onClick: () => useGameStore.getState().castVote(card),
                }))}
                activeIndex={cardSet.indexOf(players[currentPlayerName] ?? '')}
                particleCount={10}
                particleDistances={[60, 8]}
                particleR={80}
                animationTime={400}
                timeVariance={200}
                colors={[1, 2, 3, 4]}
              />
            </div>
          )}

          {(() => {
            const hasVotes = Object.values(players).some(v => v !== null)
            return (
              <div className="flex gap-3">
                {!revealed && (
                  <Button className="flex-1" disabled={!hasVotes} onClick={() => useGameStore.getState().revealVotes()}>
                    Reveal
                  </Button>
                )}
                <Button className="flex-1" variant="outline" disabled={!hasVotes} onClick={() => useGameStore.getState().resetVotes()}>
                  Reset
                </Button>
              </div>
            )
          })()}
        </CardContent>
      </Card>
    </div>
  )
}
