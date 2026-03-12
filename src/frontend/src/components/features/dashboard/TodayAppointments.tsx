'use client'

import { useEffect, useState } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import {
  getTodayAppointments,
  type TodayAppointmentDto,
  type AppointmentStatus,
} from '@/lib/api/dashboard'
import { apiPatch } from '@/lib/api/client'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { ErrorState } from '@/components/ui/error-state'

type UserRole = 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'

interface TodayAppointmentsProps {
  role?: UserRole
}

const STATUS_VARIANTS: Record<
  AppointmentStatus,
  'default' | 'secondary' | 'destructive' | 'outline'
> = {
  SCHEDULED: 'secondary',
  CHECKED_IN: 'default',
  IN_PROGRESS: 'default',
  COMPLETED: 'outline',
  CANCELLED: 'destructive',
}

function formatTime(isoDate: string): string {
  return new Date(isoDate).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

export function TodayAppointments({ role = 'ADMIN' }: TodayAppointmentsProps) {
  const router = useRouter()
  const t = useTranslations('dashboard.today')
  const tStatus = useTranslations('appointments.status')
  const tEmpty = useTranslations('onboarding.empty.dashboard_today')
  const [appointments, setAppointments] = useState<TodayAppointmentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [checkingIn, setCheckingIn] = useState<string | null>(null)

  const canCheckIn = role === 'ADMIN' || role === 'RECEPTIONIST'

  const loadAppointments = () => {
    setLoading(true)
    setError(null)
    getTodayAppointments()
      .then(setAppointments)
      .catch(() => setError(t('error_load')))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    loadAppointments()
  }, [])

  async function handleCheckIn(id: string) {
    setCheckingIn(id)
    try {
      await apiPatch(`/api/appointments/${id}/transition`, { action: 'CHECK_IN' })
      setAppointments((prev) =>
        prev.map((a) => (a.id === id ? { ...a, status: 'CHECKED_IN' } : a))
      )
      toast.success(t('checked_in_success'))
      router.refresh()
    } catch {
      toast.error(t('checked_in_failed'))
    } finally {
      setCheckingIn(null)
    }
  }

  return (
    <Card data-testid="today-appointments-card">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-base font-semibold">{t('title')}</CardTitle>
        <Link
          href="/appointments"
          className="text-sm text-muted-foreground hover:underline"
          data-testid="today-appointments-view-all"
        >
          {t('view_all')} →
        </Link>
      </CardHeader>
      <CardContent className="p-0">
        {loading ? (
          <div className="space-y-3 p-4" data-testid="today-appointments-loading">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : error ? (
          <ErrorState
            data-testid="today-appointments-error"
            title={t('error_load')}
            description={error}
            onRetry={loadAppointments}
          />
        ) : appointments.length === 0 ? (
          <div
            data-testid="empty-state-dashboard-today"
            className="p-6 flex flex-col gap-2"
          >
            <p className="text-sm font-medium text-foreground">
              {tEmpty('title')}
            </p>
            <p className="text-sm text-muted-foreground">
              {tEmpty('description')}
            </p>
            <Link
              href="/appointments"
              className="text-sm text-primary hover:underline mt-1 inline-block"
              data-testid="empty-state-cta-dashboard-today"
            >
              {tEmpty('cta')} →
            </Link>
          </div>
        ) : (
          <ul data-testid="today-appointments-list" className="divide-y">
            {appointments.map((appt) => (
              <li
                key={appt.id}
                data-testid={`today-appointment-row-${appt.id}`}
                className="flex items-center justify-between px-4 py-3 hover:bg-muted/30 transition-colors"
              >
                <div className="flex items-center gap-4 min-w-0">
                  <span
                    className="text-sm font-mono text-muted-foreground w-12 shrink-0"
                    data-testid={`appointment-time-${appt.id}`}
                  >
                    {formatTime(appt.scheduledAt)}
                  </span>
                  <Link
                    href={`/appointments/${appt.id}`}
                    className="hover:underline truncate"
                    data-testid={`appointment-link-${appt.id}`}
                  >
                    <span className="font-medium">{appt.patientName}</span>
                    <span className="text-muted-foreground ml-1 text-sm">
                      ({appt.species})
                    </span>
                  </Link>
                  <span
                    className="text-sm text-muted-foreground hidden sm:inline truncate"
                    data-testid={`appointment-vet-${appt.id}`}
                  >
                    {appt.vetName}
                  </span>
                </div>
                <div className="flex items-center gap-2 shrink-0">
                  <Badge
                    variant={STATUS_VARIANTS[appt.status]}
                    data-testid={`appointment-status-${appt.id}`}
                  >
                    {tStatus(appt.status)}
                  </Badge>
                  {canCheckIn && appt.status === 'SCHEDULED' && (
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={checkingIn === appt.id}
                      aria-busy={checkingIn === appt.id}
                      onClick={() => handleCheckIn(appt.id)}
                      data-testid={`checkin-btn-${appt.id}`}
                    >
                      {checkingIn === appt.id ? t('checking_in') : t('check_in')}
                    </Button>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}
