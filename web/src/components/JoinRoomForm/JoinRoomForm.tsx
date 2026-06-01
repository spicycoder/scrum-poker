import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '../ui/input-otp'
import { joinRoom } from '../../lib/api'

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

  async function handleJoin() {
    const trimmed = name.trim()
    if (!trimmed || otp.length !== 4) return

    localStorage.setItem('playerName', trimmed)
    setLoading(true)
    try {
      const id = parseInt(otp, 10)
      await joinRoom(id, trimmed)
      navigate(`/room/${id}`)
    } catch (err) {
      console.error('Failed to join room:', err)
    } finally {
      setLoading(false)
    }
  }

  function handleClear() {
    setName('')
    setOtp('')
  }

  const canSubmit = name.trim().length > 0 && otp.length === 4 && !loading

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

        <p className="text-sm text-muted-foreground text-center">
          XS, S, M, L, XL, ?
        </p>

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
