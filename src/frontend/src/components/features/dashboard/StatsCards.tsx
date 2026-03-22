'use client'

import { useEffect, useState, useCallback } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Skeleton } from '@/components/ui/skeleton'
import { getDashboardStats, type DashboardStatsDto } from '@/lib/api/dashboard'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { useTranslations } from 'next-intl'
import { Calendar, Clock, Users, DollarSign } from 'lucide-react'

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

  const load = useCallback(async () => {
    try {
      const result = await getDashboardStats()
      setStats(result)
      setError(null)
    } catch {
      setError('Failed to load stats')
    } finally {
      setLoading(false)
    }
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
  const showTodaysRevenue = role === 'ADMIN' || role === 'RECEPTIONIST'

  return (
    <div
      className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4"
      data-testid="stats-cards"
    >
      {showAppointments && (
        <Card className="min-h-[100px] bg-white border-border/80 rounded-xl shadow-sm" data-testid="stat-appointments-today">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                {t('appointments_today')}
              </CardTitle>
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
                <Calendar className="h-4 w-4 text-primary" aria-hidden="true" />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <p className="text-3xl font-bold text-foreground" data-testid="stat-appointments-today-value">
                {stats?.appointmentsToday ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showPendingCheckin && (
        <Card className="min-h-[100px] bg-white border-border/80 rounded-xl shadow-sm" data-testid="stat-pending-checkin">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                {t('pending_checkin')}
              </CardTitle>
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-orange-50">
                <Clock className="h-4 w-4 text-orange-500" aria-hidden="true" />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
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
            )}
          </CardContent>
        </Card>
      )}

      {showUnpaidInvoices && (
        <Card className="min-h-[100px] bg-white border-border/80 rounded-xl shadow-sm" data-testid="stat-unpaid-invoices">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                {t('unpaid_invoices')}
              </CardTitle>
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-red-50">
                <DollarSign className="h-4 w-4 text-red-500" aria-hidden="true" />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-28" />
            ) : (
              <p className="text-2xl font-bold text-foreground" data-testid="stat-unpaid-invoices-value">
                <LtrText>{formatAed(stats?.unpaidInvoicesAed ?? 0)}</LtrText>
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showTotalPatients && (
        <Card className="min-h-[100px] bg-white border-border/80 rounded-xl shadow-sm" data-testid="stat-total-patients">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                {t('total_patients')}
              </CardTitle>
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-success/10">
                <Users className="h-4 w-4 text-green-500" aria-hidden="true" />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <p className="text-3xl font-bold text-foreground" data-testid="stat-total-patients-value">
                {stats?.totalPatients ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showTodaysRevenue && (
        <Card className="min-h-[100px] bg-white border-border/80 rounded-xl shadow-sm" data-testid="stat-todays-revenue">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-[13px] font-semibold text-muted-foreground">
                {t('todays_revenue')}
              </CardTitle>
              <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-success/10">
                <DollarSign className="h-4 w-4 text-green-500" aria-hidden="true" />
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-24" />
            ) : (
              <p className="text-3xl font-bold text-foreground" data-testid="stat-todays-revenue-value">
                <LtrText>AED 0</LtrText>
              </p>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
