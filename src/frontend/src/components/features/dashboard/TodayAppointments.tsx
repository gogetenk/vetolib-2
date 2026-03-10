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

type UserRole = 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'

interface TodayAppointmentsProps {
  role?: UserRole
}

const STATUS_LABELS: Record<AppointmentStatus, string> = {
  SCHEDULED: 'Planifié',
  CHECKED_IN: 'Arrivé',
  IN_PROGRESS: 'En cours',
  COMPLETED: 'Terminé',
  CANCELLED: 'Annulé',
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
  return new Date(isoDate).toLocaleTimeString('fr-AE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

export function TodayAppointments({ role = 'ADMIN' }: TodayAppointmentsProps) {
  const router = useRouter()
  const tEmpty = useTranslations('onboarding.empty.dashboard_today')
  const [appointments, setAppointments] = useState<TodayAppointmentDto[]>([])
  const [loading, setLoading] = useState(true)
  const [checkingIn, setCheckingIn] = useState<string | null>(null)

  const canCheckIn = role === 'ADMIN' || role === 'RECEPTIONIST'

  useEffect(() => {
    getTodayAppointments()
      .then(setAppointments)
      .catch(() => toast.error('Failed to load today appointments'))
      .finally(() => setLoading(false))
  }, [])

  async function handleCheckIn(id: string) {
    setCheckingIn(id)
    try {
      await apiPatch(`/api/appointments/${id}/transition`, { action: 'CHECK_IN' })
      setAppointments((prev) =>
        prev.map((a) => (a.id === id ? { ...a, status: 'CHECKED_IN' } : a))
      )
      toast.success('Patient checked in')
      router.refresh()
    } catch {
      toast.error('Failed to check in patient')
    } finally {
      setCheckingIn(null)
    }
  }

  return (
    <Card data-testid="today-appointments-card">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-base font-semibold">Agenda du jour</CardTitle>
        <Link
          href="/appointments"
          className="text-sm text-muted-foreground hover:underline"
          data-testid="today-appointments-view-all"
        >
          Voir tout →
        </Link>
      </CardHeader>
      <CardContent className="p-0">
        {loading ? (
          <div className="space-y-3 p-4">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
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
                    {STATUS_LABELS[appt.status]}
                  </Badge>
                  {canCheckIn && appt.status === 'SCHEDULED' && (
                    <Button
                      size="sm"
                      variant="outline"
                      disabled={checkingIn === appt.id}
                      onClick={() => handleCheckIn(appt.id)}
                      data-testid={`checkin-btn-${appt.id}`}
                    >
                      {checkingIn === appt.id ? '...' : 'Check In'}
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
