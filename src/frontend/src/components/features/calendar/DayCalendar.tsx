'use client'

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useLocale, useTranslations } from 'next-intl'
import { getConsultationColor } from './consultation-colors'
import { START_HOUR, END_HOUR } from './TimeColumn'
import type { CalendarAppointment } from './types'
import type { AppointmentStatus } from '@/lib/api/appointments'

const SLOT_HEIGHT = 16 // 15-minute slot = 16px, so 1 hour = 64px (same as week view)
const HOUR_HEIGHT = SLOT_HEIGHT * 4 // 64px per hour

const SPECIES_EMOJI: Record<string, string> = {
  Dog: '\uD83D\uDC36',
  Cat: '\uD83D\uDC31',
  Bird: '\uD83D\uDC26',
  Rabbit: '\uD83D\uDC30',
  Horse: '\uD83D\uDC34',
  Exotic: '\uD83E\uDD8E',
}

const STATUS_COLORS: Record<AppointmentStatus, string> = {
  SCHEDULED: 'bg-blue-500',
  CHECKED_IN: 'bg-yellow-500',
  IN_PROGRESS: 'bg-green-500',
  COMPLETED: 'bg-gray-400',
  CANCELLED: 'bg-red-500',
}

interface LayoutedAppointment {
  appointment: CalendarAppointment
  column: number
  totalColumns: number
}

function computeOverlapLayout(appointments: CalendarAppointment[]): LayoutedAppointment[] {
  if (appointments.length === 0) return []

  // Sort by start time, then by duration (longer first)
  const sorted = [...appointments].sort((a, b) => {
    const aTime = new Date(a.scheduledAt).getTime()
    const bTime = new Date(b.scheduledAt).getTime()
    if (aTime !== bTime) return aTime - bTime
    return (b.durationMinutes ?? 30) - (a.durationMinutes ?? 30)
  })

  // Assign columns using a greedy algorithm
  const columns: { end: number; items: CalendarAppointment[] }[] = []
  const aptColumnMap = new Map<string, number>()

  for (const apt of sorted) {
    const start = new Date(apt.scheduledAt).getTime()
    const end = start + (apt.durationMinutes ?? 30) * 60000

    let placed = false
    for (let col = 0; col < columns.length; col++) {
      if (columns[col].end <= start) {
        columns[col].end = end
        columns[col].items.push(apt)
        aptColumnMap.set(apt.id, col)
        placed = true
        break
      }
    }
    if (!placed) {
      aptColumnMap.set(apt.id, columns.length)
      columns.push({ end, items: [apt] })
    }
  }

  // For each appointment, find the max columns among overlapping group
  const result: LayoutedAppointment[] = []
  for (const apt of sorted) {
    const col = aptColumnMap.get(apt.id) ?? 0
    const aptStart = new Date(apt.scheduledAt).getTime()
    const aptEnd = aptStart + (apt.durationMinutes ?? 30) * 60000

    // Count how many columns overlap with this appointment's time range
    let maxCols = 0
    for (const column of columns) {
      const hasOverlap = column.items.some((other) => {
        const otherStart = new Date(other.scheduledAt).getTime()
        const otherEnd = otherStart + (other.durationMinutes ?? 30) * 60000
        return otherStart < aptEnd && otherEnd > aptStart
      })
      if (hasOverlap) maxCols++
    }

    result.push({
      appointment: apt,
      column: col,
      totalColumns: Math.max(maxCols, 1),
    })
  }

  return result
}

interface DayCalendarBodyProps {
  date: Date
  appointments: CalendarAppointment[]
}

export function DayCalendarBody({ date, appointments }: DayCalendarBodyProps) {
  const router = useRouter()
  const locale = useLocale()
  const t = useTranslations('calendar')
  const isRtl = locale === 'ar'
  const containerRef = useRef<HTMLDivElement>(null)

  const totalHours = END_HOUR - START_HOUR + 1

  // Auto-scroll to current hour on mount
  useEffect(() => {
    if (containerRef.current) {
      const now = new Date()
      const currentHour = now.getHours()
      if (currentHour >= START_HOUR && currentHour <= END_HOUR) {
        const scrollTop = (currentHour - START_HOUR) * HOUR_HEIGHT - 100
        containerRef.current.scrollTop = Math.max(0, scrollTop)
      }
    }
  }, [])

  // Filter appointments for this day
  const dayAppointments = useMemo(() => {
    return appointments.filter((apt) => {
      const aptDate = new Date(apt.scheduledAt)
      return (
        aptDate.getFullYear() === date.getFullYear() &&
        aptDate.getMonth() === date.getMonth() &&
        aptDate.getDate() === date.getDate()
      )
    })
  }, [appointments, date])

  const layouted = useMemo(() => computeOverlapLayout(dayAppointments), [dayAppointments])

  // "Now" indicator
  const [nowOffset, setNowOffset] = useState<number | null>(null)
  useEffect(() => {
    function updateNow() {
      const now = new Date()
      const isToday =
        now.getFullYear() === date.getFullYear() &&
        now.getMonth() === date.getMonth() &&
        now.getDate() === date.getDate()
      if (!isToday) {
        setNowOffset(null)
        return
      }
      const hours = now.getHours()
      const minutes = now.getMinutes()
      if (hours < START_HOUR || hours > END_HOUR) {
        setNowOffset(null)
        return
      }
      setNowOffset((hours - START_HOUR) * HOUR_HEIGHT + (minutes / 60) * HOUR_HEIGHT)
    }
    updateNow()
    const interval = setInterval(updateNow, 60000)
    return () => clearInterval(interval)
  }, [date])

  // Time formatter
  const timeFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit', hour12: false }),
    [locale]
  )

  function handleAppointmentClick(aptId: string) {
    router.push(`/${locale}/appointments/${aptId}`)
  }

  return (
    <div
      ref={containerRef}
      className="flex overflow-y-auto border border-border rounded-lg bg-background max-h-[calc(100vh-200px)]"
      data-testid="calendar-day-view"
    >
      {/* Time column */}
      <div className="flex-shrink-0 w-16 border-e border-border">
        {Array.from({ length: totalHours }, (_, i) => {
          const hour = START_HOUR + i
          return (
            <div key={i} className="h-16 relative border-b border-border/50">
              <span className="absolute -top-2.5 end-2 text-xs text-muted-foreground">
                {String(hour).padStart(2, '0')}:00
              </span>
            </div>
          )
        })}
      </div>

      {/* Day column */}
      <div className={`flex-1 min-w-0 ${isRtl ? 'text-right' : ''}`}>
        <div className="relative" style={{ height: `${totalHours * HOUR_HEIGHT}px` }}>
          {/* Hour lines with 15-min sub-lines */}
          {Array.from({ length: totalHours }, (_, i) => {
            const hour = START_HOUR + i
            const isOffHours = hour < 8 || hour >= 18
            return (
              <div key={i} className={`h-16 border-b border-border/50 ${isOffHours ? 'bg-muted/30' : ''}`}>
                {/* 15-min sub-lines */}
                <div className="h-4 border-b border-border/20" />
                <div className="h-4 border-b border-border/30" />
                <div className="h-4 border-b border-border/20" />
                <div className="h-4" />
              </div>
            )
          })}

          {/* Now indicator */}
          {nowOffset !== null && (
            <div
              className="absolute left-0 right-0 z-20 pointer-events-none"
              style={{ top: `${nowOffset}px` }}
              data-testid="calendar-now-indicator"
            >
              <div className="flex items-center">
                <div className="w-2.5 h-2.5 rounded-full bg-red-500 -ms-1" />
                <div className="flex-1 h-0.5 bg-red-500" />
              </div>
            </div>
          )}

          {/* Appointment blocks */}
          {layouted.map(({ appointment: apt, column, totalColumns }) => {
            const color = getConsultationColor(apt.consultationType)
            const scheduledDate = new Date(apt.scheduledAt)
            const hours = scheduledDate.getHours()
            const minutes = scheduledDate.getMinutes()
            const topPx = (hours - START_HOUR) * HOUR_HEIGHT + (minutes / 60) * HOUR_HEIGHT
            const heightPx = ((apt.durationMinutes ?? 30) / 60) * HOUR_HEIGHT
            const emoji = SPECIES_EMOJI[apt.species] ?? '\uD83D\uDC3E'
            const widthPercent = 100 / totalColumns
            const leftPercent = column * widthPercent

            return (
              <div
                key={apt.id}
                data-testid={`appointment-block-${apt.id}`}
                className={`absolute rounded-md border-s-[3px] ${color.bg} ${color.border} px-2 py-1 cursor-pointer overflow-hidden transition-shadow hover:shadow-md z-10`}
                style={{
                  top: `${topPx}px`,
                  height: `${Math.max(heightPx, 24)}px`,
                  [isRtl ? 'right' : 'left']: `${leftPercent}%`,
                  width: `calc(${widthPercent}% - 4px)`,
                }}
                onClick={() => handleAppointmentClick(apt.id)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault()
                    handleAppointmentClick(apt.id)
                  }
                }}
              >
                {/* Line 1: patient + species + owner */}
                <div className="flex items-center gap-1">
                  <span className={`w-2 h-2 rounded-full flex-shrink-0 ${STATUS_COLORS[apt.status]}`} />
                  <span className={`text-xs font-medium truncate ${color.text}`}>
                    {emoji} {apt.patientName}
                  </span>
                  <span className="text-[10px] text-muted-foreground truncate">
                    — {apt.ownerName}
                  </span>
                </div>
                {/* Line 2: type badge + vet + time */}
                {heightPx >= 36 && (
                  <div className="flex items-center gap-1.5 mt-0.5">
                    <span className={`inline-block text-[10px] font-medium ${color.text} px-1.5 py-0 rounded-full ${color.bg}`}>
                      {apt.consultationType}
                    </span>
                    <span className="text-[10px] text-muted-foreground truncate">
                      {apt.vetName} · {timeFormatter.format(scheduledDate)}
                    </span>
                  </div>
                )}
                {/* Line 3: reason */}
                {heightPx >= 52 && apt.reason && (
                  <p className="text-[10px] text-muted-foreground truncate mt-0.5">
                    {apt.reason}
                  </p>
                )}
              </div>
            )
          })}

          {/* No appointments message */}
          {dayAppointments.length === 0 && (
            <div className="absolute inset-0 flex items-center justify-center">
              <p className="text-sm text-muted-foreground">{t('noAppointments')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
