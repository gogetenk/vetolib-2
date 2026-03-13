'use client'

import { useState, useMemo } from 'react'
import { useLocale, useTranslations } from 'next-intl'
import { getConsultationColor } from './consultation-colors'
import type { CalendarAppointment } from './types'

const MAX_VISIBLE = 3

interface MonthDayCellProps {
  date: Date
  isCurrentMonth: boolean
  isToday: boolean
  isWeekend: boolean
  appointments: CalendarAppointment[]
  onDayClick: (date: Date) => void
  onAppointmentClick?: (apt: CalendarAppointment) => void
}

export function MonthDayCell({
  date,
  isCurrentMonth,
  isToday,
  isWeekend,
  appointments,
  onDayClick,
  onAppointmentClick,
}: MonthDayCellProps) {
  const locale = useLocale()
  const t = useTranslations('calendar')
  const [showPopover, setShowPopover] = useState(false)

  const dateKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`

  const timeFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { hour: '2-digit', minute: '2-digit', hour12: false }),
    [locale]
  )

  const dayNumberFormatter = useMemo(
    () => new Intl.DateTimeFormat(locale, { day: 'numeric' }),
    [locale]
  )

  const visible = appointments.slice(0, MAX_VISIBLE)
  const remaining = appointments.length - MAX_VISIBLE

  function handleAppointmentClick(e: React.MouseEvent, apt: CalendarAppointment) {
    e.stopPropagation()
    onAppointmentClick?.(apt)
  }

  function handleMoreClick(e: React.MouseEvent) {
    e.stopPropagation()
    setShowPopover((prev) => !prev)
  }

  return (
    <div
      data-testid={`month-day-cell-${dateKey}`}
      className={`min-h-24 border-b border-e border-border p-1 cursor-pointer transition-all duration-200 ease-in-out hover:bg-muted/50 relative ${
        isToday ? 'bg-blue-50 dark:bg-blue-950/20' : ''
      } ${isWeekend ? 'bg-muted/40' : ''} ${!isCurrentMonth ? 'opacity-50' : ''}`}
      onClick={() => onDayClick(date)}
    >
      {/* Day number */}
      <div className="flex justify-center mb-0.5">
        <span
          className={`text-xs font-medium w-6 h-6 flex items-center justify-center rounded-full transition-all duration-200 ${
            isToday
              ? 'bg-primary text-primary-foreground animate-pulse'
              : !isCurrentMonth
                ? 'text-muted-foreground'
                : ''
          }`}
        >
          {dayNumberFormatter.format(date)}
        </span>
      </div>

      {/* Appointments (max 3) */}
      <div className="space-y-0.5">
        {visible.map((apt) => {
          const color = getConsultationColor(apt.consultationType)
          const scheduledDate = new Date(apt.scheduledAt)
          return (
            <button
              key={apt.id}
              data-testid={`month-appointment-${apt.id}`}
              className={`w-full text-start flex items-center gap-1 rounded px-1 py-0.5 text-[10px] truncate transition-all duration-200 ease-in-out hover:shadow-sm hover:scale-[1.02] ${color.bg}`}
              onClick={(e) => handleAppointmentClick(e, apt)}
            >
              <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${color.border.replace('border-l-', 'bg-')}`} />
              <span className="text-muted-foreground flex-shrink-0">
                {timeFormatter.format(scheduledDate)}
              </span>
              <span className={`truncate ${color.text}`}>{apt.patientName}</span>
            </button>
          )
        })}
      </div>

      {/* "+N more" link */}
      {remaining > 0 && (
        <div className="relative">
          <button
            data-testid={`month-more-link-${dateKey}`}
            className="text-[10px] text-primary font-medium hover:underline px-1 mt-0.5 transition-colors duration-200"
            onClick={handleMoreClick}
          >
            {t('moreAppointments', { count: remaining })}
          </button>

          {/* Popover with full list and animation */}
          <div
            className={`absolute start-0 top-full z-50 min-w-48 max-w-64 rounded-lg border border-border bg-popover p-2 shadow-lg transition-all duration-200 ease-in-out origin-top-start ${
              showPopover
                ? 'opacity-100 scale-100 translate-y-0'
                : 'opacity-0 scale-95 -translate-y-1 pointer-events-none'
            }`}
            onClick={(e) => e.stopPropagation()}
          >
            <div className="space-y-1 max-h-48 overflow-y-auto">
              {appointments.map((apt) => {
                const color = getConsultationColor(apt.consultationType)
                const scheduledDate = new Date(apt.scheduledAt)
                return (
                  <button
                    key={apt.id}
                    className={`w-full text-start flex items-center gap-1 rounded px-1.5 py-1 text-xs truncate transition-all duration-200 hover:shadow-sm hover:scale-[1.02] ${color.bg}`}
                    onClick={(e) => handleAppointmentClick(e, apt)}
                    data-testid={`month-popover-appointment-${apt.id}`}
                  >
                    <span className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${color.border.replace('border-l-', 'bg-')}`} />
                    <span className="text-muted-foreground flex-shrink-0">
                      {timeFormatter.format(scheduledDate)}
                    </span>
                    <span className={`truncate ${color.text}`}>
                      {apt.patientName} — {apt.ownerName}
                    </span>
                  </button>
                )
              })}
            </div>
            <button
              className="mt-1 text-[10px] text-muted-foreground hover:text-foreground w-full text-center transition-colors duration-200"
              onClick={(e) => {
                e.stopPropagation()
                setShowPopover(false)
              }}
              data-testid={`month-popover-close-${dateKey}`}
            >
              {t('close')}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
