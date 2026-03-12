'use client'

import { useEffect, useState, useCallback } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { getDashboardStats, type DashboardStatsDto } from '@/lib/api/dashboard'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { useTranslations } from 'next-intl'

type UserRole = 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'

interface StatsCardsProps {
  role?: UserRole
}

function formatAed(amount: number): string {
  return `AED ${amount.toLocaleString('en-AE', { minimumFractionDigits: 0, maximumFractionDigits: 2 })}`
}

export function StatsCards({ role = 'ADMIN' }: StatsCardsProps) {
  const t = useTranslations('dashboard.stats')
  const [stats, setStats] = useState<DashboardStatsDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const load = useCallback(() => {
    setLoading(true)
    setError(null)
    getDashboardStats()
      .then(setStats)
      .catch(() => setError('Failed to load stats'))
      .finally(() => setLoading(false))
  }, [])

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

  const showAppointments = true // all roles see appointments
  const showPendingCheckin = true // all roles see pending check-in
  const showUnpaidInvoices = role === 'ADMIN' || role === 'RECEPTIONIST'
  const showTotalPatients = role === 'ADMIN' || role === 'VET' || role === 'ASSISTANT'

  return (
    <div
      className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
      data-testid="stats-cards"
    >
      {showAppointments && (
        <Card data-testid="stat-appointments-today">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t('appointments_today')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-16" />
            ) : (
              <p className="text-3xl font-bold" data-testid="stat-appointments-today-value">
                {stats?.appointmentsToday ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showPendingCheckin && (
        <Card data-testid="stat-pending-checkin">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t('pending_checkin')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-16" />
            ) : (
              <div className="flex items-center gap-2">
                <p className="text-3xl font-bold" data-testid="stat-pending-checkin-value">
                  {stats?.pendingCheckin ?? 0}
                </p>
                {(stats?.pendingCheckin ?? 0) > 0 && (
                  <Badge variant="destructive" data-testid="stat-pending-checkin-badge">
                    {t('urgent')}
                  </Badge>
                )}
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {showUnpaidInvoices && (
        <Card data-testid="stat-unpaid-invoices">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t('unpaid_invoices')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-28" />
            ) : (
              <p className="text-2xl font-bold" data-testid="stat-unpaid-invoices-value">
                <LtrText>{formatAed(stats?.unpaidInvoicesAed ?? 0)}</LtrText>
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showTotalPatients && (
        <Card data-testid="stat-total-patients">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              {t('total_patients')}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-8 w-16" />
            ) : (
              <p className="text-3xl font-bold" data-testid="stat-total-patients-value">
                {stats?.totalPatients ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
