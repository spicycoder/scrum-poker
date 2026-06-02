import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '../ui/input-otp'
import { toast } from 'sonner'
import { joinRoom, getGameState, ApiError } from '../../lib/api'

function getStoredName(): string {
  return localStorage.getItem('playerName') ?? ''
}

interface JoinRoomFormProps {
  roomId?: string
}

export default function JoinRoomForm({ roomId }: JoinRoomFormProps) {
  const navigate = useNavigate()
  const [name, setName] = useState(getStoredName)
  const [otp, setOtp] = useState(roomId?.replace(/\D/g, '').slice(0, 4) ?? '')
  const [loading, setLoading] = useState(false)
  const [cardSet, setCardSet] = useState<string[] | null>(null)
  const abortRef = useRef<AbortController | null>(null)

  useEffect(() => {
    if (otp.length !== 4) {
      setCardSet(null)
      return
    }

    abortRef.current?.abort()
    const controller = new AbortController()
    abortRef.current = controller

    const id = parseInt(otp, 10)
    getGameState(id)
      .then((state) => {
        if (!controller.signal.aborted) {
          setCardSet(state.cardSet)
        }
      })
      .catch((err) => {
        if (!controller.signal.aborted) {
          setCardSet(null)
          console.error('Room not found:', err)
          toast.error('Room not found')
        }
      })

    return () => controller.abort()
  }, [otp])

  async function handleJoin() {
    const trimmed = name.trim()
    if (!trimmed || otp.length !== 4) return

    localStorage.setItem('playerName', trimmed)
    setLoading(true)
    try {
      const id = parseInt(otp, 10)
      await joinRoom(id, trimmed)
      navigate(`/${id}`)
    } catch (err) {
      console.error('Failed to join room:', err)
      if (err instanceof ApiError && err.status === 409) {
        toast.error('Player name taken')
      } else {
        toast.error('Failed to join room')
      }
    } finally {
      setLoading(false)
    }
  }

  function handleClear() {
    setName('')
    setOtp('')
    setCardSet(null)
  }

  const canSubmit = name.trim().length > 0 && otp.length === 4 && cardSet !== null && !loading

  return (
    <Card>
      <CardContent className="flex flex-col gap-5 p-6">
        <Input
          placeholder="Your name"
          maxLength={20}
          value={name}
          onChange={(e) => setName(e.target.value)}
        />

        <div className="flex justify-center">
          <InputOTP
            maxLength={4}
            value={otp}
            onChange={(value) => setOtp(value)}
          >
            <InputOTPGroup>
              <InputOTPSlot index={0} />
              <InputOTPSlot index={1} />
              <InputOTPSlot index={2} />
              <InputOTPSlot index={3} />
            </InputOTPGroup>
          </InputOTP>
        </div>

        {cardSet && (
          <p className="text-sm text-muted-foreground text-center">
            {cardSet.join(', ')}
          </p>
        )}

        <div className="flex gap-3">
          <Button
            className="flex-1"
            disabled={!canSubmit}
            onClick={handleJoin}
          >
            {loading ? 'Joining...' : 'Join'}
          </Button>
          <Button
            variant="outline"
            className="flex-1"
            onClick={handleClear}
            disabled={name.length === 0 && otp.length === 0}
          >
            Clear
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
