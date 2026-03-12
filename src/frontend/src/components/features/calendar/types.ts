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

export interface CalendarAppointment extends AppointmentDto {
  consultationType: string
  durationMinutes: number
}

export type CalendarView = 'day' | 'week' | 'month'
