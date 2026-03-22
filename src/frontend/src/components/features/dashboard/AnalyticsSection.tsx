'use client'

import { useEffect, useState, useCallback } from 'react'
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  Legend,
} from 'recharts'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { getDashboardAnalytics, type DashboardAnalyticsDto } from '@/lib/api/dashboard'
import { useTranslations } from 'next-intl'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

// UAE-context species color palette
const SPECIES_COLORS: Record<string, string> = {
  Dog: '#3b82f6',
  Cat: '#f59e0b',
  Bird: '#10b981',
  Rabbit: '#8b5cf6',
  Horse: '#ef4444',
  Camel: '#d97706',
  Exotic: '#6366f1',
}

const DEFAULT_COLOR = '#94a3b8'

type MonthKey = '01' | '02' | '03' | '04' | '05' | '06' | '07' | '08' | '09' | '10' | '11' | '12'

function formatMonthLabel(
  month: string, // "2026-01"
  t: ReturnType<typeof useTranslations<'dashboard.analytics'>>
): string {
  const parts = month.split('-')
  if (parts.length < 2) return month
  const monthNum = parts[1] as MonthKey
  return t(`months.${monthNum}`)
}

export function AnalyticsSection() {
  const t = useTranslations('dashboard.analytics')
  const [data, setData] = useState<DashboardAnalyticsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadAnalytics = useCallback(async () => {
    try {
      const result = await getDashboardAnalytics()
      setData(result)
      setError(null)
    } catch {
      setError(t('error'))
    } finally {
      setLoading(false)
    }
  }, [t])

  useEffect(() => {
    trackEvent(AnalyticsEvents.ANALYTICS_SECTION_VIEWED)
    loadAnalytics()
  }, [loadAnalytics])

  if (error) {
    return (
      <div data-testid="analytics-error" className="text-destructive text-sm py-4">
        {error}
      </div>
    )
  }

  // Revenue bar chart data
  const revenueData =
    data?.revenueByMonth.map((r) => ({
      month: formatMonthLabel(r.month, t),
      total: r.total,
    })) ?? []

  // Pie chart data for species
  const speciesData =
    data?.patientsBySpecies.map((s) => ({
      name: s.species,
      value: s.count,
    })) ?? []

  const noShowRate = data?.noShowRate ?? 0

  return (
    <div
      className="space-y-6"
      data-testid="analytics-section"
    >
      <h2 className="text-[18px] font-bold text-foreground flex items-center gap-2" data-testid="analytics-title">
        <span className="w-1 h-5 bg-primary rounded-full"></span>
        {t('title')}
      </h2>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* No-show rate stat card */}
        <Card className="border-border/80 shadow-sm" data-testid="analytics-no-show-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-[13px] font-semibold text-muted-foreground">
              {t('no_show_rate')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-10 w-24" />
            ) : (
              <p
                className="text-4xl font-bold text-foreground"
                data-testid="analytics-no-show-value"
              >
                {noShowRate.toFixed(1)}%
              </p>
            )}
          </CardContent>
        </Card>

        {/* Patients by species — pie chart */}
        <Card className="lg:col-span-2 border-border/80 shadow-sm" data-testid="analytics-species-card">
          <CardHeader className="pb-2">
            <CardTitle className="text-[13px] font-semibold text-muted-foreground">
              {t('patients_by_species')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-40 w-full" />
            ) : speciesData.length === 0 ? (
              <div className="flex h-40 items-center justify-center text-muted-foreground text-sm">
                —
              </div>
            ) : (
              <div
                className="h-40"
                data-testid="analytics-species-chart"
                role="img"
                aria-label={`Patients by species: ${speciesData.map((s) => `${s.name} ${s.value}`).join(', ')}`}
              >
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={speciesData}
                      dataKey="value"
                      nameKey="name"
                      cx="50%"
                      cy="50%"
                      outerRadius={60}
                    >
                      {speciesData.map((entry) => (
                        <Cell
                          key={entry.name}
                          fill={SPECIES_COLORS[entry.name] ?? DEFAULT_COLOR}
                        />
                      ))}
                    </Pie>
                    <Tooltip formatter={(value) => [value, '']} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Revenue by month — bar chart */}
      <Card className="border-border/80 shadow-sm" data-testid="analytics-revenue-card">
        <CardHeader className="pb-2">
          <CardTitle className="text-[13px] font-semibold text-muted-foreground">
            {t('revenue_by_month')} ({t('currency')})
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <Skeleton className="h-48 w-full" />
          ) : (
            <div
              className="h-48"
              data-testid="analytics-revenue-chart"
              role="img"
              aria-label={t('revenue_by_month')}
            >
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={revenueData} margin={{ top: 4, right: 8, left: 8, bottom: 0 }}>
                  <XAxis
                    dataKey="month"
                    tick={{ fontSize: 11 }}
                    tickLine={false}
                    axisLine={false}
                  />
                  <YAxis
                    tick={{ fontSize: 11 }}
                    tickLine={false}
                    axisLine={false}
                    tickFormatter={(v: number) => `${(v / 1000).toFixed(0)}k`}
                  />
                  <Tooltip
                    formatter={(value) => [
                      `${Number(value).toLocaleString('en-AE')} ${t('currency')}`,
                      t('revenue_by_month'),
                    ]}
                  />
                  <Bar dataKey="total" fill="oklch(var(--primary))" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
