'use client'

import { useEffect, useState, useCallback, useRef } from 'react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/error-state'
import { useTranslations } from 'next-intl'
import { toast } from 'sonner'
import {
  getWaitingRoom,
  transitionAppointment,
  type WaitingRoomPatientDto,
} from '@/lib/api/appointments'

const REFRESH_INTERVAL_MS = 30_000

type UserRole = 'ADMIN' | 'VET' | 'RECEPTIONIST' | 'ASSISTANT'

interface WaitingRoomProps {
  role?: UserRole
}

function getWaitDurationMinutes(arrivedAt: string): number {
  return Math.max(0, Math.floor((Date.now() - new Date(arrivedAt).getTime()) / 60_000))
}

function getWaitColor(minutes: number): string {
  if (minutes < 10) return 'bg-green-500'
  if (minutes < 20) return 'bg-yellow-500'
  return 'bg-red-500'
}

function getWaitBadgeVariant(minutes: number): 'default' | 'secondary' | 'destructive' {
  if (minutes < 10) return 'default'
  if (minutes < 20) return 'secondary'
  return 'destructive'
}

function formatTime(isoDate: string): string {
  return new Date(isoDate).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

export function WaitingRoom({ role = 'ADMIN' }: WaitingRoomProps) {
  const t = useTranslations('dashboard.waiting_room')
  const [patients, setPatients] = useState<WaitingRoomPatientDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [callingNext, setCallingNext] = useState<string | null>(null)
  const [, setTick] = useState(0)
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const canCallNext = role === 'ADMIN' || role === 'VET' || role === 'RECEPTIONIST'

  const loadWaitingRoom = useCallback(() => {
    setLoading(true)
    setError(null)
    getWaitingRoom()
      .then(setPatients)
      .catch(() => setError(t('error_load')))
      .finally(() => setLoading(false))
  }, [t])

  useEffect(() => {
    loadWaitingRoom()

    // Auto-refresh every 30 seconds
    intervalRef.current = setInterval(loadWaitingRoom, REFRESH_INTERVAL_MS)

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current)
    }
  }, [loadWaitingRoom])

  // Tick every minute to update wait durations
  useEffect(() => {
    const timer = setInterval(() => setTick((n) => n + 1), 60_000)
    return () => clearInterval(timer)
  }, [])

  async function handleCallNext(id: string, patientName: string) {
    setCallingNext(id)
    try {
      await transitionAppointment(id, 'START')
      setPatients((prev) => prev.filter((p) => p.id !== id))
      toast.success(t('called_success', { name: patientName }))
    } catch {
      toast.error(t('called_failed'))
    } finally {
      setCallingNext(null)
    }
  }

  // Sort by arrival time (longest wait first)
  const sorted = [...patients].sort(
    (a, b) => new Date(a.arrivedAt).getTime() - new Date(b.arrivedAt).getTime()
  )

  return (
    <Card className="border-border/80 shadow-sm" data-testid="waiting-room-card">
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <CardTitle className="text-[15px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-4 bg-amber-500 rounded-full" />
          {t('title')}
          {patients.length > 0 && (
            <Badge
              variant="secondary"
              className="rounded-full text-[11px] font-bold"
              data-testid="waiting-room-count"
            >
              {patients.length}
            </Badge>
          )}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        {loading && patients.length === 0 ? (
          <div className="space-y-3 p-4" data-testid="waiting-room-loading">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        ) : error ? (
          <ErrorState
            data-testid="waiting-room-error"
            title={t('error_load')}
            description={error}
            onRetry={loadWaitingRoom}
          />
        ) : sorted.length === 0 ? (
          <div className="p-6 text-center" data-testid="waiting-room-empty">
            <p className="text-[13px] text-muted-foreground">{t('empty')}</p>
          </div>
        ) : (
          <ul data-testid="waiting-room-list" className="divide-y divide-border/30">
            {sorted.map((patient) => {
              const waitMin = getWaitDurationMinutes(patient.arrivedAt)
              const waitColor = getWaitColor(waitMin)
              const badgeVariant = getWaitBadgeVariant(waitMin)

              return (
                <li
                  key={patient.id}
                  data-testid={`waiting-room-row-${patient.id}`}
                  className="flex items-center justify-between px-4 py-3 hover:bg-muted/50 transition-colors"
                >
                  <div className="flex items-center gap-3 min-w-0">
                    <span
                      className={`h-2.5 w-2.5 rounded-full shrink-0 ${waitColor}`}
                      data-testid={`waiting-room-indicator-${patient.id}`}
                      aria-hidden="true"
                    />
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <span
                          className="text-[14px] font-semibold text-foreground truncate"
                          data-testid={`waiting-room-patient-${patient.id}`}
                        >
                          {patient.patientName}
                        </span>
                        <span className="text-[13px] text-muted-foreground">
                          ({patient.species})
                        </span>
                      </div>
                      <div className="flex items-center gap-2 text-[12px] text-muted-foreground">
                        <span data-testid={`waiting-room-owner-${patient.id}`}>
                          {patient.ownerName}
                        </span>
                        <span className="hidden sm:inline">
                          {t('scheduled_at', { time: formatTime(patient.scheduledAt) })}
                        </span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    <Badge
                      variant={badgeVariant}
                      className="rounded-md text-[11px] font-bold"
                      data-testid={`waiting-room-wait-${patient.id}`}
                    >
                      {t('wait_minutes', { count: waitMin })}
                    </Badge>
                    {canCallNext && (
                      <Button
                        size="sm"
                        variant="default"
                        disabled={callingNext === patient.id}
                        aria-busy={callingNext === patient.id}
                        onClick={() => handleCallNext(patient.id, patient.patientName)}
                        data-testid={`waiting-room-call-btn-${patient.id}`}
                        className="rounded-xl text-[12px] font-semibold"
                      >
                        {callingNext === patient.id ? t('calling') : t('call_next')}
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
