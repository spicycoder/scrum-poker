import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { Hash, Shirt } from 'lucide-react'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'
import { createRoom } from '../../lib/api'

const STORAGE_KEY_NAME = 'playerName'
const STORAGE_KEY_SERIES = 'voteSeries'

type VoteSeries = 'fib' | 'tshirt'

const cardSets: Record<VoteSeries, string[]> = {
  fib: ['0', '0.5', '1', '2', '3', '5', '8', '13', '21', '?'],
  tshirt: ['XS', 'S', 'M', 'L', 'XL', '?'],
}

export default function CreateRoomForm() {
  const navigate = useNavigate()
  const [name, setName] = useState(() => localStorage.getItem(STORAGE_KEY_NAME) ?? '')
  const [series, setSeries] = useState<VoteSeries>(() => {
    const stored = localStorage.getItem(STORAGE_KEY_SERIES)
    return stored === 'tshirt' ? 'tshirt' : 'fib'
  })
  const [creating, setCreating] = useState(false)

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY_SERIES, series)
  }, [series])

  async function handleCreate(e?: React.FormEvent) {
    e?.preventDefault()
    const trimmed = name.trim()
    if (!trimmed) return

    localStorage.setItem(STORAGE_KEY_NAME, trimmed)
    setCreating(true)
    try {
      const roomId = await createRoom(trimmed, cardSets[series])
      navigate(`/${roomId}`)
    } catch (err) {
      console.error('Failed to create room:', err)
    } finally {
      setCreating(false)
    }
  }

  function handleKeyDown(e: React.KeyboardEvent) {
    if (e.key === 'Escape') {
      setName('')
    }
  }

  const canSubmit = name.trim().length > 0 && !creating

  return (
    <Card>
      <CardContent className="flex flex-col gap-5 p-6" onKeyDown={handleKeyDown}>
        <form onSubmit={handleCreate} className="contents">
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
          {cardSets[series].join(', ')}
        </p>

        <Input
          placeholder="Your name"
          maxLength={20}
          value={name}
          onChange={(e) => setName(e.target.value)}
        />

        <div className="flex gap-3">
          <Button
            className="flex-1"
            disabled={!canSubmit}
            onClick={handleCreate}
          >
            {creating ? 'Creating...' : 'Create'}
          </Button>
          <Button
            type="button"
            variant="outline"
            className="flex-1"
            onClick={() => setName('')}
            disabled={!canSubmit}
          >
            Clear
          </Button>
        </div>
        </form>
      </CardContent>
    </Card>
  )
}
