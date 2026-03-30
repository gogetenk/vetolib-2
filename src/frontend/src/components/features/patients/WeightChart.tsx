'use client'

import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from 'recharts'
import type { WeightCurvePointDto } from '@/lib/api/weights'

interface WeightChartProps {
  data: WeightCurvePointDto[]
}

function formatChartDate(dateStr: string): string {
  const d = new Date(dateStr)
  return d.toLocaleDateString('en-AE', {
    month: 'short',
    year: '2-digit',
  })
}

interface TooltipPayload {
  value: number
  payload: WeightCurvePointDto
}

function CustomTooltip({
  active,
  payload,
}: {
  active?: boolean
  payload?: TooltipPayload[]
  label?: string
}) {
  if (!active || !payload?.length) return null

  const entry = payload[0]
  const date = new Date(entry.payload.date).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })

  return (
    <div className="rounded-lg border border-border/80 bg-card px-3 py-2 shadow-sm">
      <p className="text-[12px] text-muted-foreground">{date}</p>
      <p className="text-[14px] font-bold text-foreground">
        {entry.value} kg
      </p>
    </div>
  )
}

export function WeightChart({ data }: WeightChartProps) {
  if (data.length === 0) return null

  // Calculate Y axis domain with some padding
  const minWeight = Math.min(...data.map((d) => d.weightKg))
  const maxWeight = Math.max(...data.map((d) => d.weightKg))
  const padding = Math.max((maxWeight - minWeight) * 0.1, 1)
  const yMin = Math.max(0, Math.floor(minWeight - padding))
  const yMax = Math.ceil(maxWeight + padding)

  return (
    <ResponsiveContainer width="100%" height={280}>
      <LineChart
        data={data}
        margin={{ top: 5, right: 20, left: 10, bottom: 5 }}
      >
        <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
        <XAxis
          dataKey="date"
          tickFormatter={formatChartDate}
          tick={{ fontSize: 11, fill: '#888' }}
          tickLine={false}
          axisLine={{ stroke: '#e5e5e5' }}
        />
        <YAxis
          domain={[yMin, yMax]}
          tick={{ fontSize: 11, fill: '#888' }}
          tickLine={false}
          axisLine={{ stroke: '#e5e5e5' }}
          unit=" kg"
          width={65}
        />
        <Tooltip content={<CustomTooltip />} />
        <Line
          type="monotone"
          dataKey="weightKg"
          stroke="hsl(var(--primary))"
          strokeWidth={2}
          dot={{ r: 4, fill: 'hsl(var(--primary))', strokeWidth: 0 }}
          activeDot={{ r: 6 }}
        />
      </LineChart>
    </ResponsiveContainer>
  )
}
