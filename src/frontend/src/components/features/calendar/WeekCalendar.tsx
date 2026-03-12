'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import { useLocale } from 'next-intl'
import { CalendarHeader } from './CalendarHeader'
import { TimeColumn, START_HOUR, END_HOUR } from './TimeColumn'
import { AppointmentBlock } from './AppointmentBlock'
import type { CalendarAppointment, CalendarDay, CalendarView } from './types'
import type { VetDto } from '@/lib/api/appointments'
import { getAppointments, getVets } from '@/lib/api/appointments'

// UAE: week starts on Sunday (0), weekend is Friday (5) and Saturday (6)
function getWeekStart(date: Date): Date {
  const d = new Date(date)
  const day = d.getDay() // 0=Sun
  d.setDate(d.getDate() - day)
  d.setHours(0, 0, 0, 0)
  return d
}

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

function formatWeekLabel(start: Date, end: Date, locale: string): string {
  const fmt = new Intl.DateTimeFormat(locale, { month: 'short', day: 'numeric', year: 'numeric' })
  const startStr = new Intl.DateTimeFormat(locale, { month: 'short', day: 'numeric' }).format(start)
  const endStr = fmt.format(end)
  return `${startStr} - ${endStr}`
}

function buildWeekDays(weekStart: Date): CalendarDay[] {
  const today = new Date()
  const days: CalendarDay[] = []
  for (let i = 0; i < 7; i++) {
    const date = new Date(weekStart)
    date.setDate(date.getDate() + i)
    const dayIndex = date.getDay()
    days.push({
      date,
      dayIndex,
      isToday: isSameDay(date, today),
      isWeekend: dayIndex === 5 || dayIndex === 6, // Friday, Saturday (UAE)
    })
  }
  return days
}

export function WeekCalendar() {
  const locale = useLocale()
  const isRtl = locale === 'ar'

  const [weekStart, setWeekStart] = useState(() => getWeekStart(new Date()))
  const [appointments, setAppointments] = useState<CalendarAppointment[]>([])
  const [vets, setVets] = useState<VetDto[]>([])
  const [selectedVetIds, setSelectedVetIds] = useState<string[]>([])
  const [activeView] = useState<CalendarView>('week')
  const [visibleStartIndex, setVisibleStartIndex] = useState(0)

  // Responsive: determine how many columns to show
  const [columnCount, setColumnCount] = useState(7)

  useEffect(() => {
    function handleResize() {
      const width = window.innerWidth
      if (width < 768) {
        setColumnCount(1)
      } else if (width < 1024) {
        setColumnCount(3)
      } else {
        setColumnCount(7)
      }
    }
    handleResize()
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  const days = useMemo(() => buildWeekDays(weekStart), [weekStart])

  // For responsive: which days are visible
  const visibleDays = useMemo(() => {
    if (columnCount >= 7) return isRtl ? [...days].reverse() : days
    const slice = days.slice(visibleStartIndex, visibleStartIndex + columnCount)
    return isRtl ? [...slice].reverse() : slice
  }, [days, columnCount, visibleStartIndex, isRtl])

  const weekEnd = useMemo(() => {
    const end = new Date(weekStart)
    end.setDate(end.getDate() + 6)
    return end
  }, [weekStart])

  const weekLabel = useMemo(() => formatWeekLabel(weekStart, weekEnd, locale), [weekStart, weekEnd, locale])

  // Fetch appointments and vets
  useEffect(() => {
    async function fetchData() {
      try {
        const [aptsResult, vetsResult] = await Promise.all([
          getAppointments({ pageSize: 100 }),
          getVets(),
        ])
        // Cast to CalendarAppointment (MSW provides enriched data)
        setAppointments(aptsResult.items as CalendarAppointment[])
        setVets(vetsResult)
      } catch {
        // Silently handle fetch errors for now
      }
    }
    fetchData()
  }, [weekStart])

  // Filter appointments for this week and selected vets
  const filteredAppointments = useMemo(() => {
    return appointments.filter((apt) => {
      const aptDate = new Date(apt.scheduledAt)
      const inWeek = aptDate >= weekStart && aptDate <= new Date(weekEnd.getTime() + 24 * 60 * 60 * 1000)
      const vetMatch = selectedVetIds.length === 0 || selectedVetIds.includes(apt.vetId)
      return inWeek && vetMatch
    })
  }, [appointments, weekStart, weekEnd, selectedVetIds])

  // Group appointments by day
  const appointmentsByDay = useMemo(() => {
    const map = new Map<string, CalendarAppointment[]>()
    for (const apt of filteredAppointments) {
      const dateKey = new Date(apt.scheduledAt).toDateString()
      const existing = map.get(dateKey) ?? []
      existing.push(apt)
      map.set(dateKey, existing)
    }
    return map
  }, [filteredAppointments])

  const goToPrev = useCallback(() => {
    if (columnCount < 7) {
      setVisibleStartIndex((prev) => Math.max(0, prev - columnCount))
    } else {
      setWeekStart((prev) => {
        const next = new Date(prev)
        next.setDate(next.getDate() - 7)
        return next
      })
    }
  }, [columnCount])

  const goToNext = useCallback(() => {
    if (columnCount < 7) {
      setVisibleStartIndex((prev) => Math.min(6 - columnCount + 1, prev + columnCount))
    } else {
      setWeekStart((prev) => {
        const next = new Date(prev)
        next.setDate(next.getDate() + 7)
        return next
      })
    }
  }, [columnCount])

  const goToToday = useCallback(() => {
    setWeekStart(getWeekStart(new Date()))
    setVisibleStartIndex(0)
  }, [])

  const totalHours = END_HOUR - START_HOUR + 1

  // Day header formatter
  const dayNameFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { weekday: 'short' }),
    [locale]
  )
  const dayNumberFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { day: 'numeric' }),
    [locale]
  )

  return (
    <div className="flex flex-col" data-testid="calendar-week-view">
      <CalendarHeader
        weekLabel={weekLabel}
        onPrev={goToPrev}
        onNext={goToNext}
        onToday={goToToday}
        activeView={activeView}
        vets={vets}
        selectedVetIds={selectedVetIds}
        onVetFilterChange={setSelectedVetIds}
      />

      <div className="flex overflow-x-auto border border-border rounded-lg bg-background">
        {/* Time column */}
        <TimeColumn />

        {/* Day columns */}
        <div className={`flex flex-1 min-w-0 ${isRtl ? 'flex-row-reverse' : ''}`}>
          {visibleDays.map((day) => {
            const dayApts = appointmentsByDay.get(day.date.toDateString()) ?? []
            return (
              <div
                key={day.date.toISOString()}
                className={`flex-1 min-w-28 border-e border-border last:border-e-0 ${
                  day.isWeekend ? 'bg-muted/40' : ''
                }`}
                data-testid={`calendar-day-column-${day.dayIndex}`}
              >
                {/* Day header */}
                <div
                  className={`h-12 flex flex-col items-center justify-center border-b border-border ${
                    day.isToday ? 'bg-primary/10' : ''
                  }`}
                >
                  <span className="text-xs text-muted-foreground">
                    {dayNameFormatter.format(day.date)}
                  </span>
                  <span
                    className={`text-sm font-semibold ${
                      day.isToday
                        ? 'bg-primary text-primary-foreground rounded-full w-6 h-6 flex items-center justify-center'
                        : ''
                    }`}
                  >
                    {dayNumberFormatter.format(day.date)}
                  </span>
                </div>

                {/* Time slots */}
                <div className="relative" style={{ height: `${totalHours * 64}px` }}>
                  {/* Hour lines */}
                  {Array.from({ length: totalHours }, (_, i) => {
                    const hour = START_HOUR + i
                    const isOffHours = hour < 8 || hour >= 18
                    return (
                      <div
                        key={i}
                        className={`h-16 border-b border-border/50 ${isOffHours ? 'bg-muted/30' : ''}`}
                      />
                    )
                  })}

                  {/* Appointment blocks */}
                  {dayApts.map((apt) => (
                    <AppointmentBlock key={apt.id} appointment={apt} />
                  ))}
                </div>
              </div>
            )
          })}
        </div>
      </div>
    </div>
  )
}
