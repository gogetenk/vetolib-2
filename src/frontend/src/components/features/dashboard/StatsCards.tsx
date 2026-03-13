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
        <Card className="min-h-[100px] bg-blue-50 border-blue-100" data-testid="stat-appointments-today">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-stone-500">
                {t('appointments_today')}
              </CardTitle>
              <Calendar className="h-5 w-5 text-blue-400" aria-hidden="true" />
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <p className="text-3xl font-bold text-stone-900" data-testid="stat-appointments-today-value">
                {stats?.appointmentsToday ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showPendingCheckin && (
        <Card className="min-h-[100px] bg-amber-50 border-amber-100" data-testid="stat-pending-checkin">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-stone-500">
                {t('pending_checkin')}
              </CardTitle>
              <Clock className="h-5 w-5 text-amber-400" aria-hidden="true" />
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <div className="flex items-center gap-2">
                <p className="text-3xl font-bold text-stone-900" data-testid="stat-pending-checkin-value">
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
        <Card className="min-h-[100px] bg-rose-50 border-rose-100" data-testid="stat-unpaid-invoices">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-stone-500">
                {t('unpaid_invoices')}
              </CardTitle>
              <DollarSign className="h-5 w-5 text-rose-400" aria-hidden="true" />
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-28" />
            ) : (
              <p className="text-2xl font-bold text-stone-900" data-testid="stat-unpaid-invoices-value">
                <LtrText>{formatAed(stats?.unpaidInvoicesAed ?? 0)}</LtrText>
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showTotalPatients && (
        <Card className="min-h-[100px] bg-emerald-50 border-emerald-100" data-testid="stat-total-patients">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-stone-500">
                {t('total_patients')}
              </CardTitle>
              <Users className="h-5 w-5 text-emerald-400" aria-hidden="true" />
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-16" />
            ) : (
              <p className="text-3xl font-bold text-stone-900" data-testid="stat-total-patients-value">
                {stats?.totalPatients ?? 0}
              </p>
            )}
          </CardContent>
        </Card>
      )}

      {showTodaysRevenue && (
        <Card className="min-h-[100px] bg-emerald-50 border-emerald-100" data-testid="stat-todays-revenue">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium text-stone-500">
                {t('todays_revenue')}
              </CardTitle>
              <DollarSign className="h-5 w-5 text-emerald-400" aria-hidden="true" />
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <Skeleton className="h-9 w-24" />
            ) : (
              <p className="text-3xl font-bold text-stone-900" data-testid="stat-todays-revenue-value">
                <LtrText>AED 0</LtrText>
              </p>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  )
}
