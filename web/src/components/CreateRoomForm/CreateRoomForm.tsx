import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Hash, Shirt } from 'lucide-react'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'
import { createRoom } from '../../lib/api'

type VoteSeries = 'fib' | 'tshirt'

const seriesValues: Record<VoteSeries, string[]> = {
  fib: ['0', '0.5', '1', '2', '3', '5', '8', '13', '21', '?'],
  tshirt: ['XS', 'S', 'M', 'L', 'XL', '?'],
}

const seriesLabels: Record<VoteSeries, string> = {
  fib: '0, 0.5, 1, 2, 3, 5, 8, 13, 21, ?',
  tshirt: 'XS, S, M, L, XL, ?',
}

function getStoredSeries(): VoteSeries {
  const stored = localStorage.getItem('voteSeries')
  if (stored === 'tshirt') return 'tshirt'
  return 'fib'
}

function getStoredName(): string {
  return localStorage.getItem('playerName') ?? ''
}

export default function CreateRoomForm() {
  const navigate = useNavigate()
  const [name, setName] = useState(getStoredName)
  const [series, setSeries] = useState<VoteSeries>(getStoredSeries)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    localStorage.setItem('voteSeries', series)
  }, [series])

  async function handleCreate() {
    const trimmed = name.trim()
    if (!trimmed) return

    localStorage.setItem('playerName', trimmed)
    setLoading(true)
    try {
      const roomId = await createRoom(trimmed, seriesValues[series])
      navigate(`/room/${roomId}`)
    } catch (err) {
      console.error('Failed to create room:', err)
    } finally {
      setLoading(false)
    }
  }

  const canSubmit = name.trim().length > 0 && !loading

  return (
    <Card>
      <CardContent className="flex flex-col gap-5 p-6">
        <Input
          placeholder="Your name"
          maxLength={20}
          value={name}
          onChange={(e) => setName(e.target.value)}
        />

        <ToggleGroup
          type="single"
          value={series}
          onValueChange={(value) => {
            if (value) setSeries(value as VoteSeries)
          }}
          className="w-full"
        >
          <ToggleGroupItem value="fib" aria-label="Fibonacci" className="flex-1">
            <Hash className="h-4 w-4" />
            <span>Fib</span>
          </ToggleGroupItem>
          <ToggleGroupItem value="tshirt" aria-label="T-Shirt Size" className="flex-1">
            <Shirt className="h-4 w-4" />
            <span>T-Shirt</span>
          </ToggleGroupItem>
        </ToggleGroup>

        <p className="text-sm text-muted-foreground text-center">
          {seriesLabels[series]}
        </p>

        <div className="flex gap-3">
          <Button
            className="flex-1"
            disabled={!canSubmit}
            onClick={handleCreate}
          >
            {loading ? 'Creating...' : 'Create'}
          </Button>
          <Button
            variant="outline"
            className="flex-1"
            onClick={() => setName('')}
            disabled={!canSubmit}
          >
            Clear
          </Button>
        </div>
      </CardContent>
    </Card>
  )
}
