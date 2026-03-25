'use client'

import React, { useEffect, useState, useCallback } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { getDashboardStats, type DashboardStatsDto } from '@/lib/api/dashboard'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { useTranslations } from 'next-intl'
import { Calendar, Clock, Users, DollarSign } from 'lucide-react'
import { useAhaMoment } from '@/hooks/use-aha-moment'

type UserRole = 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'

interface StatsCardsProps {
  role?: UserRole
}

function formatAed(amount: number): string {
  return `AED ${amount.toLocaleString('en-AE', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}`
}

export function StatsCards({ role = 'ADMIN' }: StatsCardsProps) {
  const t = useTranslations('dashboard.stats')
  const { triggerAha } = useAhaMoment()
  const [stats, setStats] = useState<DashboardStatsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(async () => {
    try {
      const result = await getDashboardStats()
      setStats(result)
      if (result.totalPatients >= 10) {
        triggerAha('ten_patients')
      }
      setError(null)
    } catch {
      setError('Failed to load stats')
    } finally {
      setLoading(false)
    }
  }, [triggerAha])

  useEffect(() => {
    load()
  }, [load])

  if (error) {
    return (
      <ErrorState
        data-testid="stats-error"
        title="Failed to load dashboard stats"
        description="Could not retrieve clinic statistics."
        onRetry={load}
      />
    )
  }

  // Build the list of visible stat cards, priority-ordered, capped at 4
  const MAX_VISIBLE_CARDS = 4

  type StatCardDef = {
    key: string
    testId: string
    label: string
    icon: typeof Calendar
    iconColor: string
    iconBg: string
    render: () => React.ReactNode
  }

  const allCards: StatCardDef[] = [
    {
      key: 'appointments',
      testId: 'stat-appointments-today',
      label: t('appointments_today'),
      icon: Calendar,
      iconColor: 'text-primary',
      iconBg: 'bg-primary/10',
      render: () =>
        loading ? (
          <Skeleton className="h-9 w-16" />
        ) : (
          <p className="text-3xl font-bold text-foreground" data-testid="stat-appointments-today-value">
            {stats?.appointmentsToday ?? 0}
          </p>
        ),
    },
    {
      key: 'pendingCheckin',
      testId: 'stat-pending-checkin',
      label: t('pending_checkin'),
      icon: Clock,
      iconColor: 'text-orange-500',
      iconBg: 'bg-orange-50',
      render: () =>
        loading ? (
          <Skeleton className="h-9 w-16" />
        ) : (
          <div className="flex items-center gap-2">
            <p className="text-3xl font-bold text-foreground" data-testid="stat-pending-checkin-value">
              {stats?.pendingCheckin ?? 0}
            </p>
            {(stats?.pendingCheckin ?? 0) > 0 && (
              <Badge variant="destructive" className="rounded-md text-[10px] font-bold" data-testid="stat-pending-checkin-badge">
                {t('urgent')}
              </Badge>
            )}
          </div>
        ),
    },
    ...(role === 'ADMIN' || role === 'VET' || role === 'ASSISTANT'
      ? [{
          key: 'totalPatients',
          testId: 'stat-total-patients',
          label: t('total_patients'),
          icon: Users,
          iconColor: 'text-green-500',
          iconBg: 'bg-success/10',
          render: () =>
            loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <p className="text-3xl font-bold text-foreground" data-testid="stat-total-patients-value">
                {stats?.totalPatients ?? 0}
              </p>
            ),
        } as StatCardDef]
      : []),
    ...(role === 'ADMIN' || role === 'RECEPTIONIST'
      ? [{
          key: 'todaysRevenue',
          testId: 'stat-todays-revenue',
          label: t('todays_revenue'),
          icon: DollarSign,
          iconColor: 'text-green-500',
          iconBg: 'bg-success/10',
          render: () =>
            loading ? (
              <Skeleton className="h-9 w-24" />
            ) : (
              <p className="text-sm text-muted-foreground" data-testid="stat-todays-revenue-value">
                {t('no_data_yet')}
              </p>
            ),
        } as StatCardDef]
      : []),
    ...(role === 'ADMIN' || role === 'RECEPTIONIST'
      ? [{
          key: 'unpaidInvoices',
          testId: 'stat-unpaid-invoices',
          label: t('unpaid_invoices'),
          icon: DollarSign,
          iconColor: 'text-red-500',
          iconBg: 'bg-red-50',
          render: () =>
            loading ? (
              <Skeleton className="h-9 w-28" />
            ) : (
              <p className="text-2xl font-bold text-foreground" data-testid="stat-unpaid-invoices-value">
                <LtrText>{formatAed(stats?.unpaidInvoicesAed ?? 0)}</LtrText>
              </p>
            ),
        } as StatCardDef]
      : []),
  ]

  const visibleCards = allCards.slice(0, MAX_VISIBLE_CARDS)

  return (
    <div
      className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
      data-testid="stats-cards"
    >
      {visibleCards.map((card) => {
        const IconComponent = card.icon
        return (
          <Card key={card.key} className="min-h-[100px] border-border/80 shadow-sm" data-testid={card.testId}>
            <CardHeader className="pb-2">
              <div className="flex items-center justify-between">
                <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                  {card.label}
                </CardTitle>
                <div className={`flex h-9 w-9 items-center justify-center rounded-xl ${card.iconBg}`}>
                  <IconComponent className={`h-4 w-4 ${card.iconColor}`} aria-hidden="true" />
                </div>
              </div>
            </CardHeader>
            <CardContent>
              {card.render()}
            </CardContent>
          </Card>
        )
      })}
    </div>
  )
}
