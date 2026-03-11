'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { createBookingAppointment } from '@/lib/api/booking'
import { ApiError } from '@/lib/api/client'
import type { BookingAppointmentDto, ConsultationTypeDto, VeterinarianDto } from '@/lib/api/booking-types'

interface StepConfirmationProps {
  petId: string
  petName: string
  consultationType: ConsultationTypeDto
  vet: VeterinarianDto | null
  date: string        // "YYYY-MM-DD"
  time: string        // "HH:mm"
  reason: string
  onSuccess: (appointment: BookingAppointmentDto) => void
  onSlotUnavailable: () => void
}

function buildScheduledAt(date: string, time: string): string {
  // Build ISO string with Dubai offset (+04:00)
  return `${date}T${time}:00+04:00`
}

function formatDateTime(date: string, time: string): string {
  const [hour, minute] = time.split(':').map(Number)
  const ampm = hour >= 12 ? 'PM' : 'AM'
  const h = hour % 12 || 12
  const timeStr = `${h}:${String(minute).padStart(2, '0')} ${ampm}`

  const d = new Date(date + 'T00:00:00')
  const dateStr = d.toLocaleDateString('en-AE', {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  return `${dateStr} at ${timeStr}`
}

export function StepConfirmation({
  petId: _petId,
  petName,
  consultationType,
  vet,
  date,
  time,
  reason,
  onSuccess,
  onSlotUnavailable,
}: StepConfirmationProps) {
  const t = useTranslations('portal.booking.wizard.confirmation')
  const [confirmed, setConfirmed] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function handleSubmit() {
    if (!confirmed || submitting) return

    setSubmitting(true)
    setErrorMessage(null)

    try {
      const appointment = await createBookingAppointment({
        consultationTypeId: consultationType.id,
        veterinarianId: vet?.id ?? '',
        petName,
        scheduledAt: buildScheduledAt(date, time),
        notes: reason || null,
      })
      onSuccess(appointment)
    } catch (err) {
      if (err instanceof ApiError && (err.status === 409 || err.status === 422)) {
        onSlotUnavailable()
      } else {
        setErrorMessage(t('generic_error'))
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div data-testid="confirmation-step" className="space-y-5">
      <div data-testid="booking-summary" className="rounded-lg border bg-card divide-y">
        <SummaryRow testId="summary-pet" label={t('pet')} value={petName} />
        <SummaryRow
          testId="summary-type"
          label={t('consultation_type')}
          value={consultationType.name}
        />
        <SummaryRow
          testId="summary-vet"
          label={t('vet')}
          value={vet ? vet.name : 'No preference'}
        />
        <SummaryRow
          testId="summary-datetime"
          label={t('date_time')}
          value={formatDateTime(date, time)}
        />
        <SummaryRow
          testId="summary-duration"
          label={t('duration')}
          value={t('duration_value', { minutes: consultationType.durationMinutes })}
        />
        {reason && (
          <SummaryRow testId="summary-notes" label={t('notes')} value={reason} />
        )}
        <SummaryRow
          testId="summary-price"
          label={t('price')}
          value={t('price_value', { price: consultationType.price })}
          highlight
        />
      </div>

      {errorMessage && (
        <div className="rounded-md bg-destructive/10 border border-destructive/30 px-4 py-3">
          <p className="text-sm text-destructive">{errorMessage}</p>
        </div>
      )}

      <div data-testid="confirm-checkbox-wrapper" className="flex items-start gap-3">
        <input
          type="checkbox"
          id="booking-confirm-checkbox"
          data-testid="booking-confirm-checkbox"
          checked={confirmed}
          onChange={e => setConfirmed(e.target.checked)}
          className="mt-0.5 h-4 w-4 rounded border-input accent-primary cursor-pointer"
        />
        <label
          htmlFor="booking-confirm-checkbox"
          className="text-sm text-muted-foreground cursor-pointer leading-relaxed"
        >
          {t('confirm_checkbox')}
        </label>
      </div>

      <button
        data-testid="confirm-booking-btn"
        type="button"
        disabled={!confirmed || submitting}
        onClick={handleSubmit}
        className={[
          'w-full rounded-lg px-4 py-3 text-sm font-semibold transition-all',
          'focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2',
          confirmed && !submitting
            ? 'bg-primary text-primary-foreground hover:bg-primary/90'
            : 'bg-muted text-muted-foreground cursor-not-allowed opacity-60',
        ].join(' ')}
      >
        {submitting ? t('submitting') : t('submit')}
      </button>
    </div>
  )
}

function SummaryRow({
  testId,
  label,
  value,
  highlight = false,
}: {
  testId: string
  label: string
  value: string
  highlight?: boolean
}) {
  return (
    <div data-testid={testId} className="flex justify-between gap-4 px-4 py-3">
      <span className="text-sm text-muted-foreground flex-shrink-0">{label}</span>
      <span
        className={`text-sm text-right ${highlight ? 'font-semibold text-foreground' : 'text-foreground'}`}
      >
        {value}
      </span>
    </div>
  )
}
