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
import { PageContainer } from "@/components/ui/page-container"
import { StatCard } from "./StatCard"
import { getTriageStats, getClassificationAccuracy } from "@/lib/api/messaging"
import type { TriageStatsDto, ClassificationAccuracyDto } from "@/lib/api/messaging-types"

// Using theme colors instead of hardcoded hex colors for better integration
const CATEGORY_COLORS: Record<string, string> = {
  MedicalUrgency: "var(--color-chart-1)",
  PostOperativeFollowUp: "var(--color-chart-2)",
  MedicalQuestion: "var(--color-chart-3)",
  AppointmentRequest: "var(--color-chart-4)",
  Administrative: "var(--color-chart-5)",
  Feedback: "var(--color-primary)",
  Other: "var(--color-muted-foreground)",
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
  const [classificationStats, setClassificationStats] = useState<ClassificationAccuracyDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState(false)

  const load = useCallback(async () => {
    try {
      setIsLoading(true)
      setError(false)
      const [data, classData] = await Promise.all([
        getTriageStats(),
        getClassificationAccuracy(),
      ])
      setStats(data)
      setClassificationStats(classData)
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
      <PageContainer data-testid="stats-loading">
        <p className="text-[13px] text-muted-foreground">{t("loading")}</p>
      </PageContainer>
    )
  }

  if (error || !stats) {
    return (
      <PageContainer data-testid="stats-error">
        <p className="text-[13px] text-destructive">{t("load_failed")}</p>
      </PageContainer>
    )
  }

  const categoryData = stats.byCategory.map((c) => ({
    name: tCat(c.category),
    count: c.count,
    percentage: c.percentage,
    color: CATEGORY_COLORS[c.category] ?? "var(--color-muted-foreground)",
  }))

  const volumeData = stats.dailyVolume.map((d) => ({
    date: d.date.slice(5), // "MM-DD"
    count: d.count,
  }))

  return (
    <PageContainer data-testid="triage-stats-page">
      <div>
        <h2 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h2>
        <p className="text-[13px] text-muted-foreground mt-1 ms-3">{t("subtitle")}</p>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4" data-testid="stats-cards">
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
      <div className="bg-white border border-border/80 rounded-xl shadow-sm p-6" data-testid="stats-category-chart">
        <h3 className="text-[14px] font-bold text-foreground mb-4">{t("by_category")}</h3>
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={categoryData} margin={{ top: 4, right: 16, bottom: 40, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--color-border)" className="opacity-50" />
            <XAxis
              dataKey="name"
              tick={{ fill: "var(--color-muted-foreground)", fontSize: 12 }}
              tickLine={false}
              axisLine={false}
              angle={-25}
              textAnchor="end"
              interval={0}
              dy={10}
            />
            <YAxis 
              tick={{ fill: "var(--color-muted-foreground)", fontSize: 12 }} 
              tickLine={false} 
              axisLine={false} 
              dx={-10}
            />
            <Tooltip
              cursor={{ fill: "var(--color-muted)", opacity: 0.2 }}
              contentStyle={{ borderRadius: "8px", border: "1px solid var(--color-border)", boxShadow: "var(--shadow-sm, 0 1px 2px 0 rgb(0 0 0 / 0.05))" }}
              formatter={(value) => [Number(value ?? 0), t("tooltip_count")]}
            />
            <Bar dataKey="count" radius={[6, 6, 0, 0]} maxBarSize={50}>
              {categoryData.map((entry, index) => (
                <Cell key={`cell-${index}`} fill={entry.color} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </div>

      {/* Volume per day Line Chart */}
      <div className="bg-white border border-border/80 rounded-xl shadow-sm p-6" data-testid="stats-volume-chart">
        <h3 className="text-[14px] font-bold text-foreground mb-4">{t("volume_per_day")}</h3>
        <ResponsiveContainer width="100%" height={240}>
          <LineChart data={volumeData} margin={{ top: 4, right: 16, bottom: 8, left: 0 }}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--color-border)" className="opacity-50" />
            <XAxis 
              dataKey="date" 
              tick={{ fill: "var(--color-muted-foreground)", fontSize: 12 }}
              tickLine={false}
              axisLine={false}
              dy={10}
            />
            <YAxis 
              tick={{ fill: "var(--color-muted-foreground)", fontSize: 12 }}
              tickLine={false}
              axisLine={false}
              dx={-10}
            />
            <Tooltip
              contentStyle={{ borderRadius: "8px", border: "1px solid var(--color-border)", boxShadow: "var(--shadow-sm, 0 1px 2px 0 rgb(0 0 0 / 0.05))" }}
              formatter={(value) => [Number(value ?? 0), t("tooltip_messages")]}
            />
            <Line
              type="monotone"
              dataKey="count"
              stroke="var(--color-primary)"
              strokeWidth={3}
              dot={false}
              activeDot={{ r: 6, fill: "var(--color-primary)", stroke: "var(--color-background)", strokeWidth: 2 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
      {/* Classification Accuracy Section */}
      {classificationStats && (
        <div className="bg-white border border-border/80 rounded-xl shadow-sm p-6" data-testid="stats-classification-accuracy">
          <h3 className="text-[14px] font-bold text-foreground mb-4">{t("classification_accuracy_title")}</h3>

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3 mb-4">
            <div className="text-center p-3 rounded-lg bg-muted/50" data-testid="classification-total">
              <p className="text-2xl font-bold text-foreground">{classificationStats.totalClassified}</p>
              <p className="text-[11px] text-muted-foreground">{t("classification_total")}</p>
            </div>
            <div className="text-center p-3 rounded-lg bg-muted/50" data-testid="classification-accuracy-pct">
              <p className="text-2xl font-bold text-foreground">{classificationStats.accuracyPercent.toFixed(1)}%</p>
              <p className="text-[11px] text-muted-foreground">{t("classification_accuracy")}</p>
            </div>
            <div className="text-center p-3 rounded-lg bg-muted/50" data-testid="classification-feedback-count">
              <p className="text-2xl font-bold text-foreground">{classificationStats.totalFeedback}</p>
              <p className="text-[11px] text-muted-foreground">{t("classification_feedback_count")}</p>
            </div>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-[12px]" data-testid="classification-by-category-table">
              <thead>
                <tr className="border-b border-border">
                  <th className="text-start py-2 text-muted-foreground font-medium">{t("classification_col_category")}</th>
                  <th className="text-end py-2 text-muted-foreground font-medium">{t("classification_col_classified")}</th>
                  <th className="text-end py-2 text-muted-foreground font-medium">{t("classification_col_accurate")}</th>
                  <th className="text-end py-2 text-muted-foreground font-medium">{t("classification_col_accuracy")}</th>
                </tr>
              </thead>
              <tbody>
                {classificationStats.byCategory.map((cat) => (
                  <tr key={cat.category} className="border-b border-border/50">
                    <td className="py-2 text-foreground">{tCat(cat.category)}</td>
                    <td className="py-2 text-end text-foreground">{cat.totalClassified}</td>
                    <td className="py-2 text-end text-foreground">{cat.accurateCount}</td>
                    <td className="py-2 text-end">
                      <span className={cat.accuracyPercent >= 85 ? 'text-green-700' : cat.accuracyPercent >= 70 ? 'text-amber-700' : 'text-red-700'}>
                        {cat.accuracyPercent.toFixed(1)}%
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </PageContainer>
  )
}
