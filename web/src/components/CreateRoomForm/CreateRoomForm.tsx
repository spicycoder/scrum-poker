import { useState, useEffect } from 'react'
import { Hash, Shirt } from 'lucide-react'
import { Card, CardContent } from '../ui/card'
import { Input } from '../ui/input'
import { Button } from '../ui/button'
import { ToggleGroup, ToggleGroupItem } from '../ui/toggle-group'

type VoteSeries = 'fib' | 'tshirt'

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
  const [name, setName] = useState(getStoredName)
  const [series, setSeries] = useState<VoteSeries>(getStoredSeries)

  useEffect(() => {
    localStorage.setItem('voteSeries', series)
  }, [series])

  function handleCreate() {
    localStorage.setItem('playerName', name.trim())
  }

  const canSubmit = name.trim().length > 0

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
            Create
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
