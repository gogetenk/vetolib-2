'use client'

import { useEffect, useMemo, useRef, useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { ChevronRight, PlusIcon, Video } from 'lucide-react'
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
  COMPLETED: 'bg-stone-400',
  CANCELLED: 'bg-red-500',
}

interface LayoutedAppointment {
  appointment: CalendarAppointment
  column: number
  totalColumns: number
}

function computeOverlapLayout(appointments: CalendarAppointment[]): LayoutedAppointment[] {
  if (appointments.length === 0) return []

  const sorted = [...appointments].sort((a, b) => {
    const aTime = new Date(a.scheduledAt).getTime()
    const bTime = new Date(b.scheduledAt).getTime()
    if (aTime !== bTime) return aTime - bTime
    return (b.durationMinutes ?? 30) - (a.durationMinutes ?? 30)
  })

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

  const result: LayoutedAppointment[] = []
  for (const apt of sorted) {
    const col = aptColumnMap.get(apt.id) ?? 0
    const aptStart = new Date(apt.scheduledAt).getTime()
    const aptEnd = aptStart + (apt.durationMinutes ?? 30) * 60000

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
  onAppointmentClick?: (apt: CalendarAppointment) => void
  onSlotClick?: (date: Date, time: string) => void
}

export function DayCalendarBody({ date, appointments, onAppointmentClick, onSlotClick }: DayCalendarBodyProps) {
  const locale = useLocale()
  const t = useTranslations('calendar')
  const isRtl = locale === 'ar'
  const containerRef = useRef<HTMLDivElement>(null)

  const totalHours = END_HOUR - START_HOUR + 1
  const [hoveredSlot, setHoveredSlot] = useState<string | null>(null)

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

  function isOffHours(hour: number): boolean {
    return hour < 8 || hour >= 18
  }

  function handleSlotClick(hour: number, isTopHalf: boolean) {
    if (isOffHours(hour)) return
    const minutes = isTopHalf ? 0 : 30
    const time = `${String(hour).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`
    onSlotClick?.(date, time)
  }

  function getSlotKey(hour: number, half: 'top' | 'bottom') {
    return `day-${hour}-${half}`
  }

  return (
    <div
      ref={containerRef}
      className="flex overflow-y-auto border border-border/60 rounded-xl bg-white shadow-sm max-h-[calc(100vh-200px)]"
      data-testid="calendar-day-view"
    >
      {/* Time column */}
      <div className="flex-shrink-0 w-16 border-e border-border/40">
        {Array.from({ length: totalHours }, (_, i) => {
          const hour = START_HOUR + i
          return (
            <div key={i} className="h-16 relative border-b border-border/20">
              <span className="absolute -top-2.5 end-2 text-[11px] font-semibold text-muted-foreground/70">
                {String(hour).padStart(2, '0')}:00
              </span>
            </div>
          )
        })}
      </div>

      {/* Day column */}
      <div className={`flex-1 min-w-0 ${isRtl ? 'text-right' : ''}`}>
        <div className="relative" style={{ height: `${totalHours * HOUR_HEIGHT}px` }}>
          {/* Hour lines with clickable slots */}
          {Array.from({ length: totalHours }, (_, i) => {
            const hour = START_HOUR + i
            const offHours = isOffHours(hour)
            const isClickable = !offHours

            const topHalfKey = getSlotKey(hour, 'top')
            const bottomHalfKey = getSlotKey(hour, 'bottom')
            const isTopHovered = hoveredSlot === topHalfKey
            const isBottomHovered = hoveredSlot === bottomHalfKey

            return (
              <div key={i} className={`h-16 border-b border-border/20 ${offHours ? 'bg-[#f9fafb]' : ''}`}>
                {/* Top half */}
                <div
                  className={`h-8 relative transition-colors duration-200 ease-in-out border-b border-border/10 ${
                    isClickable
                      ? 'cursor-pointer hover:bg-[#eef2fd]/60'
                      : 'cursor-not-allowed'
                  } ${isTopHovered && isClickable ? 'bg-[#eef2fd]/60' : ''}`}
                  onClick={() => isClickable && handleSlotClick(hour, true)}
                  onMouseEnter={() => isClickable && setHoveredSlot(topHalfKey)}
                  onMouseLeave={() => setHoveredSlot(null)}
                  data-testid={isClickable ? `calendar-day-slot-${hour}-00` : undefined}
                >
                  {isTopHovered && isClickable && (
                    <div className="absolute inset-0 flex items-center justify-center pointer-events-none animate-in fade-in duration-200">
                      <PlusIcon className="size-4 text-[#303ef5]/50" />
                    </div>
                  )}
                </div>
                {/* Bottom half */}
                <div
                  className={`h-8 relative transition-colors duration-200 ease-in-out ${
                    isClickable
                      ? 'cursor-pointer hover:bg-[#eef2fd]/60'
                      : 'cursor-not-allowed'
                  } ${isBottomHovered && isClickable ? 'bg-[#eef2fd]/60' : ''}`}
                  onClick={() => isClickable && handleSlotClick(hour, false)}
                  onMouseEnter={() => isClickable && setHoveredSlot(bottomHalfKey)}
                  onMouseLeave={() => setHoveredSlot(null)}
                  data-testid={isClickable ? `calendar-day-slot-${hour}-30` : undefined}
                >
                  {isBottomHovered && isClickable && (
                    <div className="absolute inset-0 flex items-center justify-center pointer-events-none animate-in fade-in duration-200">
                      <PlusIcon className="size-4 text-[#303ef5]/50" />
                    </div>
                  )}
                </div>
              </div>
            )
          })}

          {/* Now indicator with pulse */}
          {nowOffset !== null && (
            <div
              className="absolute left-0 right-0 z-20 pointer-events-none"
              style={{ top: `${nowOffset}px` }}
              data-testid="calendar-now-indicator"
            >
              <div className="flex items-center">
                <div className="w-2.5 h-2.5 rounded-full bg-red-500 -ms-1 animate-pulse shadow-sm shadow-red-500/50" />
                <div className="flex-1 h-0.5 bg-red-500/80" />
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
                className={`absolute rounded-xl ${color.bg} p-1.5 cursor-pointer overflow-hidden transition-all duration-200 ease-in-out hover:shadow-lg hover:-translate-y-0.5 hover:z-30 active:scale-[0.98] z-10 flex group shadow-sm border border-black/5`}
                style={{
                  top: `${topPx}px`,
                  height: `${Math.max(heightPx, 36)}px`,
                  [isRtl ? 'right' : 'left']: `${leftPercent}%`,
                  width: `calc(${widthPercent}% - 4px)`,
                }}
                onClick={() => onAppointmentClick?.(apt)}
                role="button"
                tabIndex={0}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault()
                    onAppointmentClick?.(apt)
                  }
                }}
              >
                {/* Inner vertical bar */}
                <div className={`w-[2.5px] rounded-full my-0.5 flex-shrink-0 ${color.line}`} />
                
                {/* Content */}
                <div className="flex flex-col min-w-0 flex-1 justify-center ms-2 py-0.5">
                  <span className="text-[12px] tracking-tight truncate text-[#061e44] leading-tight">
                    <span className="font-bold">{apt.ownerName.split(' ')[0].toUpperCase()}</span>{' '}
                    <span className="font-medium text-[#061e44]/90">{apt.patientName}</span>
                  </span>
                  
                  {heightPx >= 40 && (
                    <span className="text-[11px] truncate text-slate-500 font-medium leading-tight mt-[1px]">
                      {apt.consultationType}
                      {apt.reason && ` · ${apt.reason}`}
                    </span>
                  )}
                </div>

                {apt.consultationType === 'Teleconsultation' && (
                  <div className="flex-shrink-0 ms-2 flex items-center opacity-0 group-hover:opacity-100 transition-opacity md:opacity-100">
                    <div className="bg-white/80 w-[24px] h-[24px] rounded-lg shadow-sm border border-black/5 flex items-center justify-center text-[#061e44]">
                      <Video className="w-3.5 h-3.5" />
                    </div>
                  </div>
                )}
              </div>
            )
          })}

          {/* No appointments message with fade-in */}
          {dayAppointments.length === 0 && (
            <div className="absolute inset-0 flex items-center justify-center animate-in fade-in duration-500">
              <p className="text-sm text-muted-foreground">{t('noAppointments')}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
