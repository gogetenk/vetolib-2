'use client'

import { useCallback, useEffect, useMemo, useState } from 'react'
import { useLocale } from 'next-intl'
import { CalendarHeader } from './CalendarHeader'
import { DayCalendarBody } from './DayCalendar'
import { WeekCalendarBody } from './WeekCalendarBody'
import { MonthCalendarBody } from './MonthCalendar'
import type { CalendarAppointment, CalendarView } from './types'
import type { VetDto } from '@/lib/api/appointments'
import { getAppointments, getVets } from '@/lib/api/appointments'

// UAE: week starts on Sunday (0)
function getWeekStart(date: Date): Date {
  const d = new Date(date)
  const day = d.getDay()
  d.setDate(d.getDate() - day)
  d.setHours(0, 0, 0, 0)
  return d
}

function formatWeekLabel(start: Date, end: Date, locale: string): string {
  const startStr = new Intl.DateTimeFormat(locale, { month: 'short', day: 'numeric' }).format(start)
  const endStr = new Intl.DateTimeFormat(locale, { month: 'short', day: 'numeric', year: 'numeric' }).format(end)
  return `${startStr} – ${endStr}`
}

function formatDayLabel(date: Date, locale: string): string {
  return new Intl.DateTimeFormat(locale, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date)
}

function formatMonthLabel(year: number, month: number, locale: string): string {
  return new Intl.DateTimeFormat(locale, { year: 'numeric', month: 'long' }).format(new Date(year, month, 1))
}

export function CalendarContainer() {
  const locale = useLocale()

  const [currentDate, setCurrentDate] = useState(() => new Date())
  const [activeView, setActiveView] = useState<CalendarView>('week')
  const [appointments, setAppointments] = useState<CalendarAppointment[]>([])
  const [vets, setVets] = useState<VetDto[]>([])
  const [selectedVetIds, setSelectedVetIds] = useState<string[]>([])
  const [isMobile, setIsMobile] = useState(false)

  // Responsive: force day view on mobile
  useEffect(() => {
    function handleResize() {
      const mobile = window.innerWidth < 768
      setIsMobile(mobile)
      if (mobile && activeView !== 'day') {
        setActiveView('day')
      }
    }
    handleResize()
    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [activeView])

  // Fetch data
  useEffect(() => {
    async function fetchData() {
      try {
        const [aptsResult, vetsResult] = await Promise.all([
          getAppointments({ pageSize: 100 }),
          getVets(),
        ])
        setAppointments(aptsResult.items as CalendarAppointment[])
        setVets(vetsResult)
      } catch {
        // Silently handle
      }
    }
    fetchData()
  }, [])

  // Filter by selected vets
  const filteredAppointments = useMemo(() => {
    if (selectedVetIds.length === 0) return appointments
    return appointments.filter((apt) => selectedVetIds.includes(apt.vetId))
  }, [appointments, selectedVetIds])

  // Derived values
  const weekStart = useMemo(() => getWeekStart(currentDate), [currentDate])
  const weekEnd = useMemo(() => {
    const end = new Date(weekStart)
    end.setDate(end.getDate() + 6)
    return end
  }, [weekStart])

  // Date label
  const dateLabel = useMemo(() => {
    switch (activeView) {
      case 'day':
        return formatDayLabel(currentDate, locale)
      case 'week':
        return formatWeekLabel(weekStart, weekEnd, locale)
      case 'month':
        return formatMonthLabel(currentDate.getFullYear(), currentDate.getMonth(), locale)
    }
  }, [activeView, currentDate, weekStart, weekEnd, locale])

  // Navigation
  const goToPrev = useCallback(() => {
    setCurrentDate((prev) => {
      const d = new Date(prev)
      switch (activeView) {
        case 'day':
          d.setDate(d.getDate() - 1)
          break
        case 'week':
          d.setDate(d.getDate() - 7)
          break
        case 'month':
          d.setMonth(d.getMonth() - 1)
          break
      }
      return d
    })
  }, [activeView])

  const goToNext = useCallback(() => {
    setCurrentDate((prev) => {
      const d = new Date(prev)
      switch (activeView) {
        case 'day':
          d.setDate(d.getDate() + 1)
          break
        case 'week':
          d.setDate(d.getDate() + 7)
          break
        case 'month':
          d.setMonth(d.getMonth() + 1)
          break
      }
      return d
    })
  }, [activeView])

  const goToToday = useCallback(() => {
    setCurrentDate(new Date())
  }, [])

  const handleViewChange = useCallback((view: CalendarView) => {
    if (isMobile && view !== 'day') return // Block non-day views on mobile
    setActiveView(view)
  }, [isMobile])

  const handleDayClickFromMonth = useCallback((date: Date) => {
    setCurrentDate(date)
    setActiveView('day')
  }, [])

  return (
    <div className="flex flex-col" data-testid="calendar-container">
      <CalendarHeader
        dateLabel={dateLabel}
        onPrev={goToPrev}
        onNext={goToNext}
        onToday={goToToday}
        activeView={activeView}
        onViewChange={handleViewChange}
        vets={vets}
        selectedVetIds={selectedVetIds}
        onVetFilterChange={setSelectedVetIds}
      />

      {activeView === 'day' && (
        <DayCalendarBody
          date={currentDate}
          appointments={filteredAppointments}
        />
      )}

      {activeView === 'week' && (
        <WeekCalendarBody
          weekStart={weekStart}
          appointments={filteredAppointments}
        />
      )}

      {activeView === 'month' && (
        <MonthCalendarBody
          year={currentDate.getFullYear()}
          month={currentDate.getMonth()}
          appointments={filteredAppointments}
          onDayClick={handleDayClickFromMonth}
        />
      )}
    </div>
  )
}
