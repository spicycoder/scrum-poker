import { useState } from 'react'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '../ui/input-otp'

function getStoredName(): string {
  return localStorage.getItem('playerName') ?? ''
}

interface JoinRoomFormProps {
  roomId?: string
}

export default function JoinRoomForm({ roomId }: JoinRoomFormProps) {
  const [name, setName] = useState(getStoredName)
  const [otp, setOtp] = useState(roomId?.replace(/\D/g, '').slice(0, 4) ?? '')

  function handleJoin() {
    localStorage.setItem('playerName', name.trim())
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

        <p className="text-sm text-muted-foreground text-center">
          XS, S, M, L, XL, ?
        </p>

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
