'use client'

import { getConsultationColor } from './consultation-colors'
import { START_HOUR } from './TimeColumn'
import type { CalendarAppointment } from './types'
import type { AppointmentStatus } from '@/lib/api/appointments'

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
  COMPLETED: 'bg-gray-400',
  CANCELLED: 'bg-red-500',
}

interface AppointmentBlockProps {
  appointment: CalendarAppointment
  onClick?: () => void
}

export function AppointmentBlock({ appointment, onClick }: AppointmentBlockProps) {
  const color = getConsultationColor(appointment.consultationType)

  const scheduledDate = new Date(appointment.scheduledAt)
  const hours = scheduledDate.getHours()
  const minutes = scheduledDate.getMinutes()

  // Calculate position: each hour = 64px (h-16), offset from START_HOUR
  const topPx = (hours - START_HOUR) * 64 + (minutes / 60) * 64
  const heightPx = (appointment.durationMinutes / 60) * 64

  const emoji = SPECIES_EMOJI[appointment.species] ?? '\uD83D\uDC3E'

  function handleClick(e: React.MouseEvent) {
    e.stopPropagation()
    onClick?.()
  }

  return (
    <div
      data-testid={`appointment-block-${appointment.id}`}
      className={`absolute inset-x-0.5 rounded-md border-s-[3px] ${color.bg} ${color.border} px-1.5 py-0.5 cursor-pointer overflow-hidden transition-all duration-200 ease-in-out hover:shadow-lg hover:-translate-y-0.5 hover:z-20 active:scale-[0.98]`}
      style={{ top: `${topPx}px`, height: `${Math.max(heightPx, 24)}px` }}
      onClick={handleClick}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => {
        if (e.key === 'Enter' || e.key === ' ') {
          e.preventDefault()
          onClick?.()
        }
      }}
    >
      <div className="flex items-center gap-1">
        <span className={`w-2 h-2 rounded-full flex-shrink-0 ${STATUS_COLORS[appointment.status]} transition-colors duration-200`} />
        <span className={`text-xs font-medium truncate ${color.text}`}>
          {emoji} {appointment.patientName}
        </span>
      </div>
      {heightPx >= 40 && (
        <p className="text-[10px] text-muted-foreground truncate">
          {appointment.ownerName}
        </p>
      )}
      {heightPx >= 56 && (
        <span className={`inline-block text-[10px] font-medium ${color.text} mt-0.5`}>
          {appointment.consultationType}
        </span>
      )}
    </div>
  )
}
