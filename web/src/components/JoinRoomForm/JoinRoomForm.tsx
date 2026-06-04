import { useState, useEffect, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { InputOTP, InputOTPGroup, InputOTPSlot } from '../ui/input-otp'
import { getGameState, joinRoom, ApiError, type GameStateResponse } from '../../lib/api'

const STORAGE_KEY_NAME = 'playerName'

interface JoinRoomFormProps {
  roomId?: string
}

export default function JoinRoomForm({ roomId }: JoinRoomFormProps) {
  const navigate = useNavigate()
  const [name, setName] = useState(() => localStorage.getItem(STORAGE_KEY_NAME) ?? '')
  const [otp, setOtp] = useState(roomId?.replace(/\D/g, '').slice(0, 4) ?? '')
  const [roomState, setRoomState] = useState<GameStateResponse | null>(null)
  const [validating, setValidating] = useState(false)
  const [joinError, setJoinError] = useState<string | null>(null)
  const fetchId = useRef(0)

  useEffect(() => {
    if (otp.length !== 4) {
      setRoomState(null)
      setJoinError(null)
      return
    }

    const id = parseInt(otp, 10)
    const thisFetch = ++fetchId.current

    setValidating(true)
    getGameState(id)
      .then((state) => {
        if (thisFetch !== fetchId.current) return
        setRoomState(state)
        setJoinError(null)
      })
      .catch((err) => {
        if (thisFetch !== fetchId.current) return
        setRoomState(null)
        if (err instanceof ApiError && err.status === 404) {
          setJoinError('Room not found')
        } else {
          setJoinError('Failed to validate room')
        }
      })
      .finally(() => {
        if (thisFetch === fetchId.current) setValidating(false)
      })
  }, [otp])

  const nameError =
    !validating && name && roomState && name in roomState.players

  async function handleJoin() {
    const trimmed = name.trim()
    if (!trimmed || otp.length !== 4) return

    const roomIdNum = parseInt(otp, 10)

    try {
      await joinRoom(roomIdNum, trimmed)
      localStorage.setItem(STORAGE_KEY_NAME, trimmed)
      navigate(`/${roomIdNum}`)
    } catch {
      setJoinError('Failed to join room')
    }
  }

  function handleClear() {
    setName('')
    setOtp('')
    setRoomState(null)
    setJoinError(null)
    navigate('/join', { replace: true })
  }

  const displayError = validating
    ? null
    : joinError
  const displayCardSet =
    !validating && !joinError && !nameError && roomState
      ? roomState.cardSet
      : null
  const canSubmit =
    name.trim().length > 0 &&
    otp.length === 4 &&
    !validating &&
    !joinError &&
    !nameError &&
    roomState !== null

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

        {validating && (
          <p className="text-sm text-muted-foreground text-center">
            Validating...
          </p>
        )}

        {displayError && (
          <p className="text-sm text-destructive text-center">
            {displayError}
          </p>
        )}

        {nameError && (
          <p className="text-sm text-destructive text-center">
            Name already taken
          </p>
        )}

        {displayCardSet && (
          <p className="text-sm text-muted-foreground text-center">
            {displayCardSet.join(', ')}
          </p>
        )}

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
