'use client'

import { Video } from 'lucide-react'
import { getConsultationColor } from './consultation-colors'
import { START_HOUR } from './TimeColumn'
import type { CalendarAppointment } from './types'
import type { AppointmentStatus } from '@/lib/api/appointments'

const SPECIES_EMOJI: Record<string, string> = {
  Dog: '🐶',
  Cat: '🐱',
  Bird: '🐦',
  Rabbit: '🐰',
  Horse: '🐴',
  Exotic: '🦎',
}

const STATUS_OPACITY: Record<AppointmentStatus, string> = {
  SCHEDULED: 'opacity-100',
  CHECKED_IN: 'opacity-100 ring-1 ring-inset ring-yellow-400',
  IN_PROGRESS: 'opacity-100 ring-1 ring-inset ring-green-500',
  COMPLETED: 'opacity-60',
  CANCELLED: 'opacity-40 line-through',
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

  const emoji = SPECIES_EMOJI[appointment.species] ?? '🐾'

  function handleClick(e: React.MouseEvent) {
    e.stopPropagation()
    onClick?.()
  }

  return (
    <div
      data-testid={`appointment-block-${appointment.id}`}
      className={`absolute inset-x-1 rounded-xl ${color.bg} p-1.5 cursor-pointer overflow-hidden transition-all duration-150 hover:brightness-95 z-10 hover:z-20 ${STATUS_OPACITY[appointment.status]} flex group shadow-sm border border-black/5`}
      style={{ top: `${topPx}px`, height: `${Math.max(heightPx, 36)}px` }}
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
      {/* Inner vertical bar */}
      <div className={`w-[2.5px] rounded-full my-0.5 flex-shrink-0 ${color.line}`} />
      
      {/* Content */}
      <div className="flex flex-col min-w-0 flex-1 justify-center ms-2 py-0.5">
        <span className="text-[12px] tracking-tight truncate text-[#061e44] leading-tight">
          <span className="font-bold">{appointment.ownerName.split(' ')[0].toUpperCase()}</span>{' '}
          <span className="font-medium text-[#061e44]/90">{appointment.patientName}</span>
        </span>
        
        {heightPx >= 40 && (
          <span className="text-[11px] truncate text-slate-500 font-medium leading-tight mt-[1px]">
            {appointment.consultationType}
          </span>
        )}
      </div>

      {appointment.consultationType === 'Teleconsultation' && (
        <div className="flex-shrink-0 ms-2 flex items-center opacity-0 group-hover:opacity-100 transition-opacity md:opacity-100">
          <div className="bg-white/80 w-[24px] h-[24px] rounded-lg shadow-sm border border-black/5 flex items-center justify-center text-[#061e44]">
            <Video className="w-3.5 h-3.5" />
          </div>
        </div>
      )}
    </div>
  )
}
