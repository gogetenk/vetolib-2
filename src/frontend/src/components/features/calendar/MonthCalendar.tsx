'use client'

import { useMemo } from 'react'
import { useLocale } from 'next-intl'
import { MonthDayCell } from './MonthDayCell'
import type { CalendarAppointment } from './types'

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

function getMonthGrid(year: number, month: number): Date[][] {
  const firstDay = new Date(year, month, 1)
  const startOffset = firstDay.getDay()
  const gridStart = new Date(firstDay)
  gridStart.setDate(gridStart.getDate() - startOffset)

  const weeks: Date[][] = []
  const current = new Date(gridStart)

  for (let w = 0; w < 6; w++) {
    const week: Date[] = []
    for (let d = 0; d < 7; d++) {
      week.push(new Date(current))
      current.setDate(current.getDate() + 1)
    }
    if (week.some((d) => d.getMonth() === month) || w < 5) {
      weeks.push(week)
    }
    if (current.getMonth() !== month && current.getDay() === 0 && w >= 4) break
  }

  return weeks
}

interface MonthCalendarBodyProps {
  year: number
  month: number
  appointments: CalendarAppointment[]
  onDayClick: (date: Date) => void
  onAppointmentClick?: (apt: CalendarAppointment) => void
}

export function MonthCalendarBody({ year, month, appointments, onDayClick, onAppointmentClick }: MonthCalendarBodyProps) {
  const locale = useLocale()
  const isRtl = locale === 'ar'

  const today = useMemo(() => new Date(), [])

  const weeks = useMemo(() => getMonthGrid(year, month), [year, month])

  const appointmentsByDate = useMemo(() => {
    const map = new Map<string, CalendarAppointment[]>()
    for (const apt of appointments) {
      const d = new Date(apt.scheduledAt)
      const key = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
      const existing = map.get(key) ?? []
      existing.push(apt)
      map.set(key, existing)
    }
    for (const [key, apts] of map) {
      apts.sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime())
      map.set(key, apts)
    }
    return map
  }, [appointments])

  const dayNames = useMemo(() => {
    const formatter = new Intl.DateTimeFormat(locale, { weekday: 'short' })
    const names: string[] = []
    for (let i = 0; i < 7; i++) {
      const d = new Date(2025, 0, 5 + i)
      names.push(formatter.format(d))
    }
    return isRtl ? [...names].reverse() : names
  }, [locale, isRtl])

  return (
    <div className="border border-border/60 rounded-xl bg-card shadow-sm overflow-hidden" data-testid="calendar-month-view">
      {/* Day names header */}
      <div className={`grid grid-cols-7 border-b border-border/40 ${isRtl ? 'direction-rtl' : ''}`} dir={isRtl ? 'rtl' : 'ltr'}>
        {dayNames.map((name, i) => {
          const dayIndex = isRtl ? 6 - i : i
          const isWeekendDay = dayIndex === 5 || dayIndex === 6
          return (
            <div
              key={i}
              className={`py-2.5 text-center text-[11px] font-bold text-foreground uppercase tracking-wider ${
                isWeekendDay ? 'bg-muted/50' : 'bg-muted'
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
                  onAppointmentClick={onAppointmentClick}
                />
              )
            })}
          </div>
        ))}
      </div>
    </div>
  )
}
