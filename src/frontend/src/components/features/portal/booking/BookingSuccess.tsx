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

  const uid = `booking-${appointment.id}@vetara.ae`
  const summary = icalEscape(`Vet Appointment — ${pet.name} (${consultationType.name})`)
  const description = icalEscape(`Pet: ${pet.name}\nConsultation: ${consultationType.name}\nVet: ${vetName}${appointment.reason ? `\nReason: ${appointment.reason}` : ''}`)

  return [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Vetara//Booking//EN',
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
      className="flex flex-col items-center text-center gap-6 py-4 animate-[fadeIn_0.5s_ease-out]"
      data-testid="booking-success"
    >
      {/* Success icon */}
      <div
        className="flex h-20 w-20 items-center justify-center rounded-full bg-primary/10 animate-[bounceIn_0.6s_ease-out]"
        data-testid="booking-success-icon"
        aria-hidden="true"
      >
        <CheckCircle2 className="h-10 w-10 text-primary" />
      </div>

      {/* Title */}
      <div className="animate-[fadeIn_0.5s_ease-out_0.3s_both]">
        <h2
          className="text-2xl font-bold text-foreground"
          data-testid="booking-success-title"
        >
          {t('title')}
        </h2>
        <p className="mt-1 text-sm text-muted-foreground" data-testid="booking-success-subtitle">
          {t('subtitle')}
        </p>
      </div>

      {/* Appointment summary */}
      <div
        className="w-full rounded-xl border border-primary/20 bg-primary/5 p-5 text-start space-y-3 animate-[fadeIn_0.5s_ease-out_0.5s_both]"
        data-testid="booking-success-details"
      >
        <div className="flex items-center gap-3">
          <CalendarDays className="h-4 w-4 text-primary shrink-0" aria-hidden="true" />
          <div>
            <p className="text-xs text-muted-foreground">{t('dateLabel')}</p>
            <p className="text-sm font-medium text-foreground" data-testid="success-date">
              {date}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-3">
          <Clock className="h-4 w-4 text-primary shrink-0" aria-hidden="true" />
          <div>
            <p className="text-xs text-muted-foreground">{t('timeLabel')}</p>
            <p className="text-sm font-medium text-foreground" data-testid="success-time">
              {time}
            </p>
          </div>
        </div>
        <p className="text-xs text-muted-foreground border-t border-primary/20 pt-3">
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
        className="flex items-center gap-2 rounded-xl border border-border/80 px-4 py-2.5 text-sm font-medium text-foreground hover:bg-muted transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary w-full sm:w-auto justify-center"
      >
        <Download className="h-4 w-4" aria-hidden="true" />
        {t('addToCalendar')}
      </button>

      {/* Auto-redirect notice */}
      <p
        className="text-xs text-muted-foreground/60"
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
        className="text-sm text-primary hover:text-primary/90 underline transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary rounded"
      >
        {t('goNow')}
      </button>
    </div>
  )
}
