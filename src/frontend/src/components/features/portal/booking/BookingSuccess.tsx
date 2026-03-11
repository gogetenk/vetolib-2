'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { CheckCircle2, Calendar, ArrowRight } from 'lucide-react'
import type { BookingAppointmentDto } from '@/lib/api/booking-types'

interface BookingSuccessProps {
  appointment: BookingAppointmentDto
}

function formatScheduledAt(scheduledAt: string): string {
  const d = new Date(scheduledAt)
  return d.toLocaleString('en-AE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    timeZone: 'Asia/Dubai',
  })
}

function generateIcal(appointment: BookingAppointmentDto): string {
  const start = new Date(appointment.scheduledAt)
  const end = new Date(start.getTime() + appointment.durationMinutes * 60 * 1000)

  function toIcalDate(d: Date): string {
    return d.toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '')
  }

  const uid = `booking-${appointment.id}@vetolib.ae`
  const summary = `${appointment.consultationTypeName} — ${appointment.petName}`
  const description = `Veterinary appointment for ${appointment.petName}\\nVet: ${appointment.veterinarianName}`
  const location = `${appointment.clinicName}, ${appointment.clinicAddress}`

  return [
    'BEGIN:VCALENDAR',
    'VERSION:2.0',
    'PRODID:-//Vetolib//Vetolib Booking//EN',
    'CALSCALE:GREGORIAN',
    'METHOD:PUBLISH',
    'BEGIN:VEVENT',
    `UID:${uid}`,
    `DTSTART:${toIcalDate(start)}`,
    `DTEND:${toIcalDate(end)}`,
    `SUMMARY:${summary}`,
    `DESCRIPTION:${description}`,
    `LOCATION:${location}`,
    'STATUS:CONFIRMED',
    'END:VEVENT',
    'END:VCALENDAR',
  ].join('\r\n')
}

const REDIRECT_SECONDS = 5

export function BookingSuccess({ appointment }: BookingSuccessProps) {
  const t = useTranslations('portal.booking.wizard.success')
  const router = useRouter()
  const params = useParams<{ locale: string; clinicSlug: string }>()

  const [countdown, setCountdown] = useState(REDIRECT_SECONDS)

  useEffect(() => {
    const interval = setInterval(() => {
      setCountdown(prev => {
        if (prev <= 1) {
          clearInterval(interval)
          router.push(`/${params.locale}/portal/${params.clinicSlug}/book/appointments`)
          return 0
        }
        return prev - 1
      })
    }, 1000)
    return () => clearInterval(interval)
  }, [router, params.locale, params.clinicSlug])

  function handleAddToCalendar() {
    const ical = generateIcal(appointment)
    const blob = new Blob([ical], { type: 'text/calendar;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `vetolib-appointment-${appointment.id}.ics`
    a.click()
    URL.revokeObjectURL(url)
  }

  function handleViewAppointments() {
    router.push(`/${params.locale}/portal/${params.clinicSlug}/book/appointments`)
  }

  return (
    <div data-testid="booking-success" className="space-y-6 text-center py-4">
      <div className="flex justify-center">
        <div className="rounded-full bg-green-100 p-4">
          <CheckCircle2 className="h-12 w-12 text-green-600" />
        </div>
      </div>

      <div className="space-y-2">
        <h2 data-testid="success-title" className="text-xl font-bold text-foreground">
          {t('title')}
        </h2>
        <p data-testid="success-subtitle" className="text-sm text-muted-foreground">
          {t('subtitle')}
        </p>
      </div>

      <div data-testid="success-recap" className="rounded-lg border bg-card text-left divide-y">
        <RecapRow testId="success-pet" label={t('pet')} value={appointment.petName} />
        <RecapRow
          testId="success-type"
          label={t('consultation_type')}
          value={appointment.consultationTypeName}
        />
        <RecapRow testId="success-vet" label={t('vet')} value={appointment.veterinarianName} />
        <RecapRow
          testId="success-datetime"
          label={t('date_time')}
          value={formatScheduledAt(appointment.scheduledAt)}
        />
        <RecapRow
          testId="success-duration"
          label={t('duration')}
          value={`${appointment.durationMinutes} min`}
        />
        <RecapRow testId="success-clinic" label={t('clinic')} value={appointment.clinicName} />
        <RecapRow testId="success-address" label={t('address')} value={appointment.clinicAddress} />
      </div>

      <div className="flex flex-col gap-3">
        <button
          data-testid="add-to-calendar-btn"
          type="button"
          onClick={handleAddToCalendar}
          className="flex items-center justify-center gap-2 w-full rounded-lg border border-primary px-4 py-3 text-sm font-medium text-primary hover:bg-primary/5 transition-colors focus:outline-none focus:ring-2 focus:ring-primary"
        >
          <Calendar className="h-4 w-4" />
          {t('add_to_calendar')}
        </button>

        <button
          data-testid="view-appointments-btn"
          type="button"
          onClick={handleViewAppointments}
          className="flex items-center justify-center gap-2 w-full rounded-lg bg-primary px-4 py-3 text-sm font-semibold text-primary-foreground hover:bg-primary/90 transition-colors focus:outline-none focus:ring-2 focus:ring-primary"
        >
          {t('view_appointments')}
          <ArrowRight className="h-4 w-4" />
        </button>
      </div>

      <p data-testid="redirect-countdown" className="text-xs text-muted-foreground">
        {t('redirect_notice', { seconds: countdown })}
      </p>
    </div>
  )
}

function RecapRow({ testId, label, value }: { testId: string; label: string; value: string }) {
  return (
    <div data-testid={testId} className="flex justify-between gap-4 px-4 py-3">
      <span className="text-sm text-muted-foreground flex-shrink-0">{label}</span>
      <span className="text-sm text-foreground text-right">{value}</span>
    </div>
  )
}
