import type { AppointmentDto } from '@/lib/api/appointments'

export interface CalendarDay {
  date: Date
  dayIndex: number // 0=Sunday, 6=Saturday
  isToday: boolean
  isWeekend: boolean // Friday or Saturday (UAE)
}

export interface CalendarWeek {
  days: CalendarDay[]
  start: Date
  end: Date
  label: string
}

/** Extended appointment used by calendar components. Adds UI-only fields not present in backend DTO. */
export interface CalendarAppointment extends AppointmentDto {
  /** UI-only: consultation category for color-coding calendar blocks */
  consultationType: string
}

export type CalendarView = 'day' | 'week' | 'month'
