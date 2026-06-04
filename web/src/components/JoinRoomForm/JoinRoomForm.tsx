import { useState } from 'react'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '../ui/input-otp'

const STORAGE_KEY_NAME = 'playerName'

interface JoinRoomFormProps {
  roomId?: string
}

export default function JoinRoomForm({ roomId }: JoinRoomFormProps) {
  const [name, setName] = useState(() => localStorage.getItem(STORAGE_KEY_NAME) ?? '')
  const [otp, setOtp] = useState(roomId?.replace(/\D/g, '').slice(0, 4) ?? '')

  function handleJoin() {
    const trimmed = name.trim()
    if (!trimmed || otp.length !== 4) return
    localStorage.setItem(STORAGE_KEY_NAME, trimmed)
  }

  function handleClear() {
    setName('')
    setOtp('')
  }

  const canSubmit = name.trim().length > 0 && otp.length === 4

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

        <div className="flex gap-3">
          <Button
            className="flex-1"
            disabled={!canSubmit}
            onClick={handleJoin}
          >
            Join
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
