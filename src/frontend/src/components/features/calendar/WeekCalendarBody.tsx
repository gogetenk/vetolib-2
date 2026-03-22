'use client'

import { useEffect, useMemo, useState } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { PlusIcon } from 'lucide-react'
import { TimeColumn, START_HOUR, END_HOUR } from './TimeColumn'
import { AppointmentBlock } from './AppointmentBlock'
import type { CalendarAppointment, CalendarDay } from './types'

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
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
      isWeekend: dayIndex === 5 || dayIndex === 6,
    })
  }
  return days
}

interface WeekCalendarBodyProps {
  weekStart: Date
  appointments: CalendarAppointment[]
  onAppointmentClick?: (apt: CalendarAppointment) => void
  onSlotClick?: (date: Date, time: string) => void
}

export function WeekCalendarBody({ weekStart, appointments, onAppointmentClick, onSlotClick }: WeekCalendarBodyProps) {
  const locale = useLocale()
  const t = useTranslations('calendar')
  const isRtl = locale === 'ar'
  const [visibleStartIndex] = useState(0)
  const [columnCount, setColumnCount] = useState(7)
  const [hoveredSlot, setHoveredSlot] = useState<string | null>(null)

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

  const filteredAppointments = useMemo(() => {
    return appointments.filter((apt) => {
      const aptDate = new Date(apt.scheduledAt)
      return aptDate >= weekStart && aptDate <= new Date(weekEnd.getTime() + 24 * 60 * 60 * 1000)
    })
  }, [appointments, weekStart, weekEnd])

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

  const totalHours = END_HOUR - START_HOUR + 1

  const dayNameFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { weekday: 'short' }),
    [locale]
  )
  const dayNumberFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { day: 'numeric' }),
    [locale]
  )

  function getSlotKey(dayIndex: number, hour: number, half: 'top' | 'bottom') {
    return `${dayIndex}-${hour}-${half}`
  }

  function isOffHours(hour: number): boolean {
    return hour < 8 || hour >= 18
  }

  function handleSlotClick(day: CalendarDay, hour: number, isTopHalf: boolean) {
    if (day.isWeekend) return
    if (isOffHours(hour)) return
    const minutes = isTopHalf ? 0 : 30
    const time = `${String(hour).padStart(2, '0')}:${String(minutes).padStart(2, '0')}`
    onSlotClick?.(day.date, time)
  }

  return (
    <div className="flex overflow-x-auto border border-border/60 rounded-xl bg-white shadow-sm" data-testid="calendar-week-view">
      <TimeColumn />

      <div className={`flex flex-1 min-w-0 ${isRtl ? 'flex-row-reverse' : ''}`}>
        {visibleDays.map((day) => {
          const dayApts = appointmentsByDay.get(day.date.toDateString()) ?? []
          return (
            <div
              key={day.date.toISOString()}
              className={`flex-1 min-w-28 border-e border-border/30 last:border-e-0 ${
                day.isWeekend ? 'bg-muted/50' : ''
              }`}
              data-testid={`calendar-day-column-${day.dayIndex}`}
            >
              {/* Day header */}
              <div
                className={`h-14 flex flex-col items-center justify-center border-b border-border/40 transition-colors duration-200 ${
                  day.isToday ? 'bg-primary/10' : ''
                }`}
              >
                <span className={`text-[11px] font-semibold uppercase tracking-wider ${day.isToday ? 'text-primary' : 'text-muted-foreground'}`}>
                  {dayNameFormatter.format(day.date)}
                </span>
                <span
                  className={`mt-0.5 text-[15px] font-bold ${
                    day.isToday
                      ? 'bg-primary text-primary-foreground rounded-full w-7 h-7 flex items-center justify-center shadow-sm text-[13px]'
                      : 'text-foreground'
                  }`}
                >
                  {dayNumberFormatter.format(day.date)}
                </span>
              </div>

              {/* Time slots */}
              <div className="relative" style={{ height: `${totalHours * 64}px` }}>
                {Array.from({ length: totalHours }, (_, i) => {
                  const hour = START_HOUR + i
                  const offHours = isOffHours(hour)
                  const isClickable = !day.isWeekend && !offHours

                  const topHalfKey = getSlotKey(day.dayIndex, hour, 'top')
                  const bottomHalfKey = getSlotKey(day.dayIndex, hour, 'bottom')
                  const isTopHovered = hoveredSlot === topHalfKey
                  const isBottomHovered = hoveredSlot === bottomHalfKey

                  return (
                    <div
                      key={i}
                      className={`h-16 border-b border-border/20 ${offHours ? 'bg-muted/50' : ''} ${
                        day.isWeekend ? 'cursor-not-allowed' : ''
                      }`}
                    >
                      {/* Top half (XX:00 - XX:30) */}
                      <div
                        className={`h-8 relative transition-colors duration-200 ease-in-out ${
                          isClickable
                            ? 'cursor-pointer hover:bg-primary/10'
                            : offHours
                            ? 'cursor-not-allowed'
                            : ''
                        } ${isTopHovered && isClickable ? 'bg-primary/10' : ''}`}
                        onClick={() => isClickable && handleSlotClick(day, hour, true)}
                        onMouseEnter={() => isClickable && setHoveredSlot(topHalfKey)}
                        onMouseLeave={() => setHoveredSlot(null)}
                        data-testid={isClickable ? `calendar-slot-${day.dayIndex}-${hour}-00` : undefined}
                        title={!isClickable && offHours ? t('closedSlot') : undefined}
                      >
                        {isTopHovered && isClickable && (
                          <div className="absolute inset-0 flex items-center justify-center pointer-events-none animate-in fade-in duration-200">
                            <PlusIcon className="size-4 text-primary/50" />
                          </div>
                        )}
                      </div>

                      {/* Bottom half (XX:30 - XX+1:00) */}
                      <div
                        className={`h-8 relative transition-colors duration-200 ease-in-out ${
                          isClickable
                            ? 'cursor-pointer hover:bg-primary/10'
                            : offHours
                            ? 'cursor-not-allowed'
                            : ''
                        } ${isBottomHovered && isClickable ? 'bg-primary/10' : ''}`}
                        onClick={() => isClickable && handleSlotClick(day, hour, false)}
                        onMouseEnter={() => isClickable && setHoveredSlot(bottomHalfKey)}
                        onMouseLeave={() => setHoveredSlot(null)}
                        data-testid={isClickable ? `calendar-slot-${day.dayIndex}-${hour}-30` : undefined}
                        title={!isClickable && offHours ? t('closedSlot') : undefined}
                      >
                        {isBottomHovered && isClickable && (
                          <div className="absolute inset-0 flex items-center justify-center pointer-events-none animate-in fade-in duration-200">
                            <PlusIcon className="size-4 text-primary/50" />
                          </div>
                        )}
                      </div>
                    </div>
                  )
                })}

                {/* Appointment blocks */}
                {dayApts.map((apt) => (
                  <AppointmentBlock
                    key={apt.id}
                    appointment={apt}
                    onClick={() => onAppointmentClick?.(apt)}
                  />
                ))}

                {/* No appointments message */}
                {dayApts.length === 0 && !day.isWeekend && (
                  <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
                    <p className="text-xs text-muted-foreground/50 animate-in fade-in duration-500">
                      {t('noAppointments')}
                    </p>
                  </div>
                )}
              </div>
            </div>
          )
        })}
      </div>
    </div>
  )
}
