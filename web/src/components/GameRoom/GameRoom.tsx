import { useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useGameStore } from '../../store/gameStore'
import { Card, CardContent } from '../ui/card'
import { Button } from '../ui/button'

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
    leaveRoom,
  } = useGameStore()

  useEffect(() => {
    if (!roomId) return
    const id = parseInt(roomId, 10)
    if (isNaN(id)) {
      navigate('/')
      return
    }
    connect(id)

    return () => {
      disconnect()
    }
  }, [roomId]) // eslint-disable-line react-hooks/exhaustive-deps

  async function handleLeave() {
    await leaveRoom()
    navigate('/')
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
      <Card className="w-full max-w-md">
        <CardContent className="p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold">Room {gameId}</h2>
            <span className="text-sm text-muted-foreground">
              {connectionStatus}
            </span>
          </div>

          <div className="space-y-2 mb-6">
            {Object.entries(players).map(([name, value]) => (
              <div key={name} className="flex items-center justify-between">
                <span className={name === currentPlayerName ? 'font-bold' : ''}>
                  {name} {name === currentPlayerName && '(you)'}
                </span>
                <span className="text-muted-foreground">
                  {revealed ? (value ?? '-') : value ? '✓' : '-'}
                </span>
              </div>
            ))}
          </div>

          {!revealed && cardSet.length > 0 && (
            <div className="flex flex-wrap gap-2 mb-4">
              {cardSet.map((card) => (
                <Button
                  key={card}
                  variant={players[currentPlayerName] === card ? 'default' : 'outline'}
                  onClick={() => useGameStore.getState().castVote(card)}
                >
                  {card}
                </Button>
              ))}
            </div>
          )}

          <div className="flex gap-3">
            {!revealed ? (
              <Button className="flex-1" onClick={() => useGameStore.getState().revealVotes()}>
                Reveal
              </Button>
            ) : (
              <Button className="flex-1" variant="outline" onClick={() => useGameStore.getState().resetVotes()}>
                Reset
              </Button>
            )}
            <Button variant="destructive" onClick={handleLeave}>
              Leave
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
