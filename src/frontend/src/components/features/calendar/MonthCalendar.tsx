'use client'

import { useMemo } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { MonthDayCell } from './MonthDayCell'
import type { CalendarAppointment } from './types'

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

function getMonthGrid(year: number, month: number): Date[][] {
  // month is 0-indexed
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)

  // Start from Sunday (UAE week starts Sunday)
  const startOffset = firstDay.getDay() // 0=Sun
  const gridStart = new Date(firstDay)
  gridStart.setDate(gridStart.getDate() - startOffset)

  const weeks: Date[][] = []
  const current = new Date(gridStart)

  // Generate 6 weeks max
  for (let w = 0; w < 6; w++) {
    const week: Date[] = []
    for (let d = 0; d < 7; d++) {
      week.push(new Date(current))
      current.setDate(current.getDate() + 1)
    }
    // Only include if at least one day is in the current month
    if (week.some((d) => d.getMonth() === month) || w < 5) {
      weeks.push(week)
    }
    // Stop if we've passed the last day and completed a row
    if (current.getMonth() !== month && current.getDay() === 0 && w >= 4) break
  }

  return weeks
}

interface MonthCalendarBodyProps {
  year: number
  month: number // 0-indexed
  appointments: CalendarAppointment[]
  onDayClick: (date: Date) => void
}

export function MonthCalendarBody({ year, month, appointments, onDayClick }: MonthCalendarBodyProps) {
  const locale = useLocale()
  const t = useTranslations('calendar')
  const isRtl = locale === 'ar'

  const today = useMemo(() => new Date(), [])

  const weeks = useMemo(() => getMonthGrid(year, month), [year, month])

  // Group appointments by date key
  const appointmentsByDate = useMemo(() => {
    const map = new Map<string, CalendarAppointment[]>()
    for (const apt of appointments) {
      const d = new Date(apt.scheduledAt)
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
      const existing = map.get(key) ?? []
      existing.push(apt)
      map.set(key, existing)
    }
    // Sort each day's appointments by time
    for (const [key, apts] of map) {
      apts.sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime())
      map.set(key, apts)
    }
    return map
  }, [appointments])

  // Day names header
  const dayNames = useMemo(() => {
    const formatter = new Intl.DateTimeFormat(locale, { weekday: 'short' })
    // Generate from a known Sunday (Jan 5, 2025 is Sunday)
    const names: string[] = []
    for (let i = 0; i < 7; i++) {
      const d = new Date(2025, 0, 5 + i)
      names.push(formatter.format(d))
    }
    return isRtl ? [...names].reverse() : names
  }, [locale, isRtl])

  return (
    <div className="border border-border rounded-lg bg-background overflow-hidden" data-testid="calendar-month-view">
      {/* Day names header */}
      <div className={`grid grid-cols-7 border-b border-border ${isRtl ? 'direction-rtl' : ''}`} dir={isRtl ? 'rtl' : 'ltr'}>
        {dayNames.map((name, i) => {
          // Weekend: Friday (5) and Saturday (6) — in our array, index 5=Fri, 6=Sat
          const dayIndex = isRtl ? 6 - i : i
          const isWeekendDay = dayIndex === 5 || dayIndex === 6
          return (
            <div
              key={i}
              className={`py-2 text-center text-xs font-medium text-muted-foreground ${
                isWeekendDay ? 'bg-muted/40' : ''
              }`}
            >
              {name}
            </div>
          )
        })}
      </div>

      {/* Weeks */}
      <div dir={isRtl ? 'rtl' : 'ltr'}>
        {weeks.map((week, wi) => (
          <div key={wi} className="grid grid-cols-7">
            {(isRtl ? [...week].reverse() : week).map((date) => {
              const dateKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`
              const dayIndex = date.getDay()
              return (
                <MonthDayCell
                  key={dateKey}
                  date={date}
                  isCurrentMonth={date.getMonth() === month}
                  isToday={isSameDay(date, today)}
                  isWeekend={dayIndex === 5 || dayIndex === 6}
                  appointments={appointmentsByDate.get(dateKey) ?? []}
                  onDayClick={onDayClick}
                />
              )
            })}
          </div>
        ))}
      </div>
    </div>
  )
}
