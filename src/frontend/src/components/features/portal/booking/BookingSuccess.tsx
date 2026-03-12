'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { CheckCircle2, CalendarDays, Clock, Download } from 'lucide-react'
import type { BookingAppointmentDto, BookingPetDto, ConsultationTypeDto } from '@/lib/api/booking'

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatDateTime(isoStr: string): { date: string; time: string } {
  const date = new Date(isoStr).toLocaleDateString('en-AE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  const time = new Date(isoStr).toLocaleTimeString('en-AE', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
    timeZone: 'Asia/Dubai',
  })
  return { date, time }
}

/** Escape a string for iCal (RFC 5545) */
function icalEscape(str: string): string {
  return str.replace(/\\/g, '\\\\').replace(/;/g, '\\;').replace(/,/g, '\\,').replace(/\n/g, '\\n')
}

/** Generate an iCal string for the appointment */
function generateIcal(
  appointment: BookingAppointmentDto,
  pet: BookingPetDto,
  consultationType: ConsultationTypeDto,
  vetName: string
): string {
  const start = new Date(appointment.slotStartsAt)
  const end = new Date(appointment.slotEndsAt)

  function toIcalDate(d: Date): string {
    return d.toISOString().replace(/[-:]/g, '').split('.')[0] + 'Z'
  }

  const uid = `booking-${appointment.id}@vetolib.ae`
  const summary = icalEscape(`Vet Appointment — ${pet.name} (${consultationType.name})`)
  const description = icalEscape(`Pet: ${pet.name}\nConsultation: ${consultationType.name}\nVet: ${vetName}${appointment.reason ? `\nReason: ${appointment.reason}` : ''}`)

  return [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Vetolib//Booking//EN',
    'BEGIN:VEVENT',
    `UID:${uid}`,
    `DTSTAMP:${toIcalDate(new Date())}`,
    `DTSTART:${toIcalDate(start)}`,
    `DTEND:${toIcalDate(end)}`,
    `SUMMARY:${summary}`,
    `DESCRIPTION:${description}`,
    'END:VEVENT',
    'END:VCALENDAR',
  ].join('\r\n')
}

// ─── Types ────────────────────────────────────────────────────────────────────

interface BookingSuccessProps {
  appointment: BookingAppointmentDto
  pet: BookingPetDto
  consultationType: ConsultationTypeDto
  vetName: string
  locale: string
  clinicSlug: string
}

// ─── Component ────────────────────────────────────────────────────────────────

const REDIRECT_DELAY_MS = 5000

export function BookingSuccess({
  appointment,
  pet,
  consultationType,
  vetName,
  locale,
  clinicSlug,
}: BookingSuccessProps) {
  const router = useRouter()
  const t = useTranslations('portal.booking.wizard.success')
  const [secondsLeft, setSecondsLeft] = useState(Math.round(REDIRECT_DELAY_MS / 1000))

  const { date, time } = formatDateTime(appointment.slotStartsAt)

  // Auto-redirect to portal home after delay
  useEffect(() => {
    const interval = setInterval(() => {
      setSecondsLeft((s) => {
        if (s <= 1) {
          clearInterval(interval)
          router.push(`/${locale}/portal/${clinicSlug}`)
          return 0
        }
        return s - 1
      })
    }, 1000)

    return () => clearInterval(interval)
  }, [router, locale, clinicSlug])

  function handleDownloadIcal() {
    const ical = generateIcal(appointment, pet, consultationType, vetName)
    const blob = new Blob([ical], { type: 'text/calendar;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = `appointment-${pet.name.toLowerCase()}.ics`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }

  return (
    <div
      className="flex flex-col items-center text-center gap-6 py-4"
      data-testid="booking-success"
    >
      {/* Success icon */}
      <div
        className="flex h-20 w-20 items-center justify-center rounded-full bg-emerald-100"
        data-testid="booking-success-icon"
        aria-hidden="true"
      >
        <CheckCircle2 className="h-10 w-10 text-emerald-600" />
      </div>

      {/* Title */}
      <div>
        <h2
          className="text-2xl font-bold text-gray-900"
          data-testid="booking-success-title"
        >
          {t('title')}
        </h2>
        <p className="mt-1 text-sm text-gray-500" data-testid="booking-success-subtitle">
          {t('subtitle')}
        </p>
      </div>

      {/* Appointment summary */}
      <div
        className="w-full rounded-xl border border-emerald-200 bg-emerald-50 p-5 text-left space-y-3"
        data-testid="booking-success-details"
      >
        <div className="flex items-center gap-3">
          <CalendarDays className="h-4 w-4 text-emerald-600 shrink-0" aria-hidden="true" />
          <div>
            <p className="text-xs text-gray-500">{t('dateLabel')}</p>
            <p className="text-sm font-medium text-gray-900" data-testid="success-date">
              {date}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Clock className="h-4 w-4 text-emerald-600 shrink-0" aria-hidden="true" />
          <div>
            <p className="text-xs text-gray-500">{t('timeLabel')}</p>
            <p className="text-sm font-medium text-gray-900" data-testid="success-time">
              {time}
            </p>
          </div>
        </div>
        <p className="text-xs text-gray-600 border-t border-emerald-200 pt-3">
          <span className="font-medium">{pet.name}</span> will see{' '}
          <span className="font-medium">{vetName}</span> for{' '}
          <span className="font-medium">{consultationType.name}</span>
        </p>
      </div>

      {/* iCal download */}
      <button
        type="button"
        onClick={handleDownloadIcal}
        data-testid="download-ical-btn"
        aria-label="Download appointment to calendar"
        className="flex items-center gap-2 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 w-full sm:w-auto justify-center"
      >
        <Download className="h-4 w-4" aria-hidden="true" />
        {t('addToCalendar')}
      </button>

      {/* Auto-redirect notice */}
      <p
        className="text-xs text-gray-400"
        aria-live="polite"
        data-testid="booking-success-redirect-notice"
      >
        {t('redirectNotice', { n: secondsLeft })}
      </p>

      {/* Manual link */}
      <button
        type="button"
        onClick={() => router.push(`/${locale}/portal/${clinicSlug}`)}
        data-testid="booking-success-portal-link"
        aria-label="Go to My Appointments now"
        className="text-sm text-emerald-600 hover:text-emerald-700 underline transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 rounded"
      >
        {t('goNow')}
      </button>
    </div>
  )
}
