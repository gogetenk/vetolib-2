"use client"

import { useEffect, useState, useCallback } from "react"
import { useTranslations } from "next-intl"
import {
  BarChart,
  Bar,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from "recharts"
import { StatCard } from "./StatCard"
import { getTriageStats } from "@/lib/api/messaging"
import type { TriageStatsDto } from "@/lib/api/messaging-types"

const CATEGORY_COLORS: Record<string, string> = {
  MedicalUrgency: "#ef4444",
  PostOperativeFollowUp: "#f97316",
  MedicalQuestion: "#3b82f6",
  AppointmentRequest: "#22c55e",
  Administrative: "#a855f7",
  Feedback: "#eab308",
  Other: "#6b7280",
}

function formatMinutes(minutes: number): string {
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h === 0) return `${m}min`
  return `${h}h ${m}min`
}

export function TriageStatsPage() {
  const t = useTranslations("messaging_admin.stats")
  const tCat = useTranslations("messaging.category")
  const [stats, setStats] = useState<TriageStatsDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(false)

  const load = useCallback(async () => {
    try {
      setIsLoading(true)
      setError(false)
      const data = await getTriageStats()
      setStats(data)
    } catch {
      setError(true)
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  if (isLoading) {
    return (
      <div data-testid="stats-loading" className="text-sm text-muted-foreground">
        {t("loading")}
      </div>
    )
  }

  if (error || !stats) {
    return (
      <div data-testid="stats-error" className="text-sm text-destructive">
        {t("load_failed")}
      </div>
    )
  }

  const categoryData = stats.byCategory.map((c) => ({
    name: tCat(c.category),
    count: c.count,
    percentage: c.percentage,
    color: CATEGORY_COLORS[c.category] ?? "#6b7280",
  }))

  const volumeData = stats.dailyVolume.map((d) => ({
    date: d.date.slice(5), // "MM-DD"
    count: d.count,
  }))

  return (
    <div className="space-y-6" data-testid="triage-stats-page">
      <div>
        <h2 className="text-xl font-semibold">{t("title")}</h2>
        <p className="text-sm text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3" data-testid="stats-cards">
        <StatCard
          testId="stat-avg-response"
          title={t("avg_response_time")}
          value={formatMinutes(stats.averageFirstResponseMinutes)}
          description={t("avg_response_desc")}
        />
        <StatCard
          testId="stat-total-conversations"
          title={t("total_conversations")}
          value={String(stats.totalConversations)}
          description={t("open_conversations", { count: stats.openConversations })}
        />
        <StatCard
          testId="stat-ai-accuracy"
          title={t("ai_accuracy")}
          value={`${stats.aiAccuracyPercent.toFixed(1)}%`}
          description={t("ai_accuracy_desc")}
        />
        <StatCard
          testId="stat-conversion"
          title={t("conversion_rate")}
          value={`${stats.conversionToAppointmentPercent.toFixed(1)}%`}
          description={t("conversion_desc")}
        />
      </div>

      {/* Messages by Category Bar Chart */}
      <div className="rounded-md border p-4" data-testid="stats-category-chart">
        <h3 className="text-sm font-semibold mb-4">{t("by_category")}</h3>
        <ResponsiveContainer width="100%" height={240}>
          <BarChart data={categoryData} margin={{ top: 4, right: 16, bottom: 40, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis
              dataKey="name"
              tick={{ fontSize: 11 }}
              angle={-25}
              textAnchor="end"
              interval={0}
            />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(value) => [Number(value ?? 0), t("tooltip_count")]}
            />
            <Bar dataKey="count" radius={[4, 4, 0, 0]}>
              {categoryData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Volume per day Line Chart */}
      <div className="rounded-md border p-4" data-testid="stats-volume-chart">
        <h3 className="text-sm font-semibold mb-4">{t("volume_per_day")}</h3>
        <ResponsiveContainer width="100%" height={200}>
          <LineChart data={volumeData} margin={{ top: 4, right: 16, bottom: 8, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis dataKey="date" tick={{ fontSize: 11 }} />
            <YAxis tick={{ fontSize: 11 }} />
            <Tooltip
              formatter={(value) => [Number(value ?? 0), t("tooltip_messages")]}
            />
            <Line
              type="monotone"
              dataKey="count"
              stroke="hsl(var(--primary))"
              strokeWidth={2}
              dot={{ r: 3 }}
              activeDot={{ r: 5 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}
