'use client'

import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useLocale } from 'next-intl'
import { CalendarHeader } from './CalendarHeader'
import { DayCalendarBody } from './DayCalendar'
import { WeekCalendarBody } from './WeekCalendarBody'
import { MonthCalendarBody } from './MonthCalendar'
import { QuickAppointmentForm } from './QuickAppointmentForm'
import { AppointmentDetailSheet } from './AppointmentDetailSheet'
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

interface SelectedSlot {
  date: Date
  time: string // HH:mm
}

export function CalendarContainer() {
  const locale = useLocale()

  const [currentDate, setCurrentDate] = useState(() => new Date())
  const [activeView, setActiveView] = useState<CalendarView>('week')
  const [appointments, setAppointments] = useState<CalendarAppointment[]>([])
  const [vets, setVets] = useState<VetDto[]>([])
  const [selectedVetIds, setSelectedVetIds] = useState<string[]>([])
  const [isMobile, setIsMobile] = useState(false)
  const [isLoading, setIsLoading] = useState(true)

  // View transition state
  const [isTransitioning, setIsTransitioning] = useState(false)
  const [navDirection, setNavDirection] = useState<'left' | 'right' | null>(null)
  const navTransitionTimeout = useRef<ReturnType<typeof setTimeout> | null>(null)
  const viewTransitionTimeout = useRef<ReturnType<typeof setTimeout> | null>(null)

  // Clean up timeouts on unmount
  useEffect(() => {
    return () => {
      if (navTransitionTimeout.current) clearTimeout(navTransitionTimeout.current)
      if (viewTransitionTimeout.current) clearTimeout(viewTransitionTimeout.current)
    }
  }, [])

  // Quick create state (BUG-3 fix)
  const [selectedSlot, setSelectedSlot] = useState<SelectedSlot | null>(null)

  // Detail sheet state (BUG-2 fix)
  const [selectedAppointment, setSelectedAppointment] = useState<CalendarAppointment | null>(null)

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
  const fetchData = useCallback(async () => {
    setIsLoading(true)
    try {
      const [aptsResult, vetsResult] = await Promise.all([
        getAppointments({ pageSize: 100 }),
        getVets(),
      ])
      setAppointments(aptsResult.items as CalendarAppointment[])
      setVets(vetsResult)
    } catch {
      // Silently handle
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
  }, [fetchData])

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

  // Navigation with slide animation
  const goToPrev = useCallback(() => {
    setNavDirection('right')
    setIsTransitioning(true)
    if (navTransitionTimeout.current) clearTimeout(navTransitionTimeout.current)
    navTransitionTimeout.current = setTimeout(() => {
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
      setIsTransitioning(false)
      setNavDirection(null)
    }, 150)
  }, [activeView])

  const goToNext = useCallback(() => {
    setNavDirection('left')
    setIsTransitioning(true)
    if (navTransitionTimeout.current) clearTimeout(navTransitionTimeout.current)
    navTransitionTimeout.current = setTimeout(() => {
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
      setIsTransitioning(false)
      setNavDirection(null)
    }, 150)
  }, [activeView])

  const goToToday = useCallback(() => {
    setCurrentDate(new Date())
  }, [])

  const handleViewChange = useCallback((view: CalendarView) => {
    if (isMobile && view !== 'day') return
    if (view === activeView) return
    setIsTransitioning(true)
    if (viewTransitionTimeout.current) clearTimeout(viewTransitionTimeout.current)
    viewTransitionTimeout.current = setTimeout(() => {
      setActiveView(view)
      setIsTransitioning(false)
    }, 150)
  }, [isMobile, activeView])

  const handleDayClickFromMonth = useCallback((date: Date) => {
    setCurrentDate(date)
    setActiveView('day')
  }, [])

  // BUG-2: Handle appointment click to open detail sheet
  const handleAppointmentClick = useCallback((apt: CalendarAppointment) => {
    setSelectedAppointment(apt)
  }, [])

  // BUG-3: Handle slot click to open quick create
  const handleSlotClick = useCallback((date: Date, time: string) => {
    setSelectedSlot({ date, time })
  }, [])

  // Transition classes for navigation slide
  const getTransitionClasses = () => {
    if (isTransitioning) {
      if (navDirection === 'left') return 'opacity-0 -translate-x-4'
      if (navDirection === 'right') return 'opacity-0 translate-x-4'
      return 'opacity-0 scale-[0.98]'
    }
    return 'opacity-100 translate-x-0 scale-100'
  }

  return (
    <div className="flex flex-col h-full w-full bg-white rounded-xl" data-testid="calendar-container">
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
        onNewAppointment={() => {
          const now = new Date()
          const timeString = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`
          setSelectedSlot({ date: now, time: timeString })
        }}
      />

      {/* Loading skeleton */}
      {isLoading && (
        <div className="flex-1 animate-in fade-in duration-300" data-testid="calendar-loading-skeleton">
          <div className="flex border border-border/60 rounded-xl bg-white shadow-sm overflow-hidden h-full">
            <div className="flex-shrink-0 w-16 border-e border-border/40">
              {Array.from({ length: 8 }, (_, i) => (
                <div key={i} className="h-16 border-b border-border/30 p-2">
                  <div className="h-3 w-10 bg-[#f4f6f9] animate-pulse rounded" />
                </div>
              ))}
            </div>
            <div className="flex-1 grid grid-cols-5 gap-0">
              {Array.from({ length: 40 }, (_, i) => (
                <div key={i} className="h-16 border-b border-e border-border/30 p-1">
                  {i % 7 === 0 && (
                    <div className="h-8 bg-[#f4f6f9] animate-pulse rounded-lg mx-0.5" />
                  )}
                </div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Calendar views with transition */}
      {!isLoading && (
        <div
          className={`flex-1 min-h-0 overflow-y-auto transition-all duration-200 ease-in-out ${getTransitionClasses()}`}
          data-testid="calendar-view-container"
        >
          {activeView === 'day' && (
            <DayCalendarBody
              date={currentDate}
              appointments={filteredAppointments}
              onAppointmentClick={handleAppointmentClick}
              onSlotClick={handleSlotClick}
            />
          )}

          {activeView === 'week' && (
            <WeekCalendarBody
              weekStart={weekStart}
              appointments={filteredAppointments}
              onAppointmentClick={handleAppointmentClick}
              onSlotClick={handleSlotClick}
            />
          )}

          {activeView === 'month' && (
            <MonthCalendarBody
              year={currentDate.getFullYear()}
              month={currentDate.getMonth()}
              appointments={filteredAppointments}
              onDayClick={handleDayClickFromMonth}
              onAppointmentClick={handleAppointmentClick}
            />
          )}
        </div>
      )}

      {/* Quick create dialog (BUG-3 fix: integrated at container level) */}
      {selectedSlot && (
        <QuickAppointmentForm
          open={!!selectedSlot}
          onOpenChange={(open) => {
            if (!open) setSelectedSlot(null)
          }}
          date={selectedSlot.date}
          time={selectedSlot.time}
          vets={vets}
          onCreated={fetchData}
        />
      )}

      {/* Appointment detail sheet (BUG-2 fix: integrated at container level) */}
      <AppointmentDetailSheet
        open={!!selectedAppointment}
        onOpenChange={(open) => {
          if (!open) setSelectedAppointment(null)
        }}
        appointment={selectedAppointment}
        onUpdated={fetchData}
      />
    </div>
  )
}
