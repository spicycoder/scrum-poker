import { useState, useEffect, useRef } from 'react'
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from 'recharts'
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
  type ChartConfig,
} from '@/components/ui/chart'
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card'
import { getStats, type StatsResponse } from '@/lib/api'

const chartConfig = {
  games: { label: 'Games', color: 'var(--stat-games)' },
  players: { label: 'Players', color: 'var(--stat-players)' },
} satisfies ChartConfig

function AnimatedNumber({ value }: { value: number }) {
  const [display, setDisplay] = useState(0)
  const prev = useRef(0)
  const raf = useRef<ReturnType<typeof requestAnimationFrame> | null>(null)

  useEffect(() => {
    const start = prev.current
    const diff = value - start
    if (diff === 0) return

    const duration = 800
    const startTime = performance.now()

    function tick(now: number) {
      const elapsed = now - startTime
      const progress = Math.min(elapsed / duration, 1)
      const eased = 1 - Math.pow(1 - progress, 3)
      setDisplay(Math.round(start + diff * eased))
      if (progress < 1) raf.current = requestAnimationFrame(tick)
      else prev.current = value
    }

    raf.current = requestAnimationFrame(tick)
    return () => { if (raf.current) cancelAnimationFrame(raf.current) }
  }, [value])

  return <span>{display.toLocaleString()}</span>
}

export default function StatsCard() {
  const [data, setData] = useState<StatsResponse | null>(null)

  useEffect(() => {
    getStats().then(setData).catch(() => {})
  }, [])

  const chartData = (data?.monthlyStats ?? []).map((r) => ({
    month: r.monthKey,
    games: r.gameCount,
    players: r.playerCount,
  }))

  const current = data?.currentMonth ?? { monthKey: '', gameCount: 0, playerCount: 0 }

  return (
    <Card className="w-full max-w-md mt-6">
      <CardHeader>
        <CardTitle className="text-lg">Serving</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-4">
        <div className="flex justify-center gap-6 text-sm">
          <div className="text-center">
            <div className="text-2xl font-bold tabular-nums text-[var(--stat-games)]">
              <AnimatedNumber value={current.gameCount} />
            </div>
            <div className="text-muted-foreground text-xs">Games this month</div>
          </div>
          <div className="text-center">
            <div className="text-2xl font-bold tabular-nums text-[var(--stat-players)]">
              <AnimatedNumber value={current.playerCount} />
            </div>
            <div className="text-muted-foreground text-xs">Players this month</div>
          </div>
        </div>

        <ChartContainer config={chartConfig} className="min-h-[200px] w-full">
          <BarChart data={chartData} accessibilityLayer>
            <CartesianGrid vertical={false} />
            <XAxis
              dataKey="month"
              tickLine={false}
              tickMargin={10}
              axisLine={false}
              tickFormatter={(v: string) => {
                const [y, m] = v.split('-')
                const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec']
                return `${months[parseInt(m) - 1]} ${y?.slice(2)}`
              }}
            />
            <YAxis
              tickLine={false}
              axisLine={false}
              tickMargin={10}
              allowDecimals={false}
            />
            <ChartTooltip content={<ChartTooltipContent />} />
            <ChartLegend content={<ChartLegendContent />} />
            <Bar dataKey="games" stackId="a" fill="var(--color-games)" radius={[0, 0, 4, 4]} />
            <Bar dataKey="players" stackId="a" fill="var(--color-players)" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ChartContainer>
      </CardContent>
    </Card>
  )
}
