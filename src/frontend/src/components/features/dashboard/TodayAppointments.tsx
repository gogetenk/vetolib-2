'use client'

import { useEffect, useState, useCallback } from 'react'
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
  type ConsultationType,
} from '@/lib/api/dashboard'
import { apiPatch } from '@/lib/api/client'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { useDirection } from '@/hooks/use-direction'

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

const CONSULTATION_TYPE_COLORS: Record<ConsultationType, { dot: string; badge: string; text: string }> = {
  GENERAL: { dot: 'bg-blue-400', badge: 'bg-blue-100 text-blue-700', text: 'General' },
  VACCINATION: { dot: 'bg-success', badge: 'bg-success/15 text-success', text: 'Vaccination' },
  SURGERY: { dot: 'bg-red-400', badge: 'bg-red-100 text-red-700', text: 'Surgery' },
  EMERGENCY: { dot: 'bg-rose-500', badge: 'bg-rose-100 text-rose-700', text: 'Emergency' },
  FOLLOWUP: { dot: 'bg-violet-400', badge: 'bg-violet-100 text-violet-700', text: 'Follow-up' },
  GROOMING: { dot: 'bg-amber-400', badge: 'bg-amber-100 text-amber-700', text: 'Grooming' },
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
  const dir = useDirection()
  const arrow = dir === 'rtl' ? '\u2190' : '\u2192'
  const t = useTranslations('dashboard.today')
  const tStatus = useTranslations('appointments.status')
  const tEmpty = useTranslations('onboarding.empty.dashboard_today')
  const [appointments, setAppointments] = useState<TodayAppointmentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [checkingIn, setCheckingIn] = useState<string | null>(null)

  const canCheckIn = role === 'ADMIN' || role === 'RECEPTIONIST'

  const loadAppointments = useCallback(() => {
    setLoading(true)
    setError(null)
    getTodayAppointments()
      .then(setAppointments)
      .catch(() => setError(t('error_load')))
      .finally(() => setLoading(false))
  }, [t])

  useEffect(() => {
    loadAppointments()
  }, [loadAppointments])

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
    <Card className="border-border/80 shadow-sm" data-testid="today-appointments-card">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-[15px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-4 bg-primary rounded-full"></span>
          {t('title')}
        </CardTitle>
        <Link
          href="/appointments"
          className="text-[13px] text-primary font-semibold hover:underline"
          data-testid="today-appointments-view-all"
        >
          {t('view_all')} {arrow}
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
            <p className="text-[13px] font-semibold text-foreground">
              {tEmpty('title')}
            </p>
            <p className="text-[13px] text-muted-foreground">
              {tEmpty('description')}
            </p>
            <Link
              href="/appointments"
              className="text-sm text-primary hover:underline mt-1 inline-block"
              data-testid="empty-state-cta-dashboard-today"
            >
              {tEmpty('cta')} {arrow}
            </Link>
          </div>
        ) : (
          <ul data-testid="today-appointments-list" className="divide-y divide-border/30">
            {appointments.map((appt) => {
              const typeConfig = appt.consultationType
                ? CONSULTATION_TYPE_COLORS[appt.consultationType]
                : null

              return (
                <li
                  key={appt.id}
                  data-testid={`today-appointment-row-${appt.id}`}
                  className="flex items-center justify-between px-4 py-3 hover:bg-muted/50 transition-colors"
                >
                  <div className="flex items-center gap-3 min-w-0">
                    {typeConfig && (
                      <span
                        className={`h-2.5 w-2.5 rounded-full shrink-0 ${typeConfig.dot}`}
                        data-testid={`appointment-type-dot-${appt.id}`}
                        aria-hidden="true"
                      />
                    )}
                    <LtrText
                      className="text-[13px] font-mono text-muted-foreground w-12 shrink-0"
                      data-testid={`appointment-time-${appt.id}`}
                    >
                      {formatTime(appt.scheduledAt)}
                    </LtrText>
                    <Link
                      href={`/appointments/${appt.id}`}
                      className="hover:underline truncate"
                      data-testid={`appointment-link-${appt.id}`}
                    >
                      <span className="text-[14px] font-semibold text-foreground">{appt.patientName}</span>
                      <span className="text-muted-foreground ms-1 text-[13px]">
                        ({appt.species})
                      </span>
                    </Link>
                    {typeConfig && (
                      <span
                        className={`text-[11px] px-2 py-0.5 rounded-md font-bold shrink-0 hidden sm:inline-block ${typeConfig.badge}`}
                        data-testid={`appointment-type-badge-${appt.id}`}
                      >
                        {typeConfig.text}
                      </span>
                    )}
                    <span
                      className="text-[13px] text-muted-foreground hidden md:inline truncate"
                      data-testid={`appointment-vet-${appt.id}`}
                    >
                      {appt.vetName}
                    </span>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    <Badge
                      variant={STATUS_VARIANTS[appt.status]}
                      className="rounded-md text-[10px] font-bold uppercase tracking-wider"
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
                        className="rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted"
                      >
                        {checkingIn === appt.id ? t('checking_in') : t('check_in')}
                      </Button>
                    )}
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </CardContent>
    </Card>
  )
}
