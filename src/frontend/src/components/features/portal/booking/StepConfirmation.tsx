'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { CalendarDays, Clock, Stethoscope, PawPrint, User } from 'lucide-react'
import { cn } from '@/lib/utils'
import { createBookingAppointment } from '@/lib/api/booking'
import type { BookingPetDto, ConsultationTypeDto, VeterinarianDto, BookingSlot, BookingAppointmentDto } from '@/lib/api/booking'

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatDateTime(isoStr: string, locale: string): { date: string; time: string } {
  const date = new Date(isoStr).toLocaleDateString(locale, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    timeZone: 'Asia/Dubai',
  })
  const time = new Date(isoStr).toLocaleTimeString(locale, {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
    timeZone: 'Asia/Dubai',
  })
  return { date, time }
}

// ─── Types ────────────────────────────────────────────────────────────────────

interface StepConfirmationProps {
  pet: BookingPetDto
  consultationType: ConsultationTypeDto
  vet: VeterinarianDto | null
  slot: BookingSlot
  reason: string
  onSuccess: (appointment: BookingAppointmentDto) => void
  onBack: () => void
}

// ─── Component ────────────────────────────────────────────────────────────────

export function StepConfirmation({
  pet,
  consultationType,
  vet,
  slot,
  reason,
  onSuccess,
  onBack,
}: StepConfirmationProps) {
  const t = useTranslations('portal.booking.wizard.confirmationSection')
  const tw = useTranslations('portal.booking.wizard')
  const [termsAccepted, setTermsAccepted] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const { date, time } = formatDateTime(slot.startsAt, 'en-AE')
  const vetName = vet?.name ?? slot.vetName

  async function handleConfirm() {
    if (!termsAccepted || isSubmitting) return

    setIsSubmitting(true)
    setSubmitError(null)

    try {
      const appointment = await createBookingAppointment({
        petId: pet.id,
        consultationTypeId: consultationType.id,
        vetId: vet?.id ?? null,
        slotStartsAt: slot.startsAt,
        slotEndsAt: slot.endsAt,
        reason: reason || undefined,
      })
      onSuccess(appointment)
    } catch {
      setSubmitError(t('submitError'))
      setIsSubmitting(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="step-confirmation">
      {/* Summary card */}
      <div
        className="rounded-xl border border-border/80 bg-muted overflow-hidden"
        data-testid="booking-summary-card"
      >
        <div className="bg-primary px-5 py-3">
          <h3 className="text-sm font-semibold text-white">{t('summaryTitle')}</h3>
        </div>
        <dl className="divide-y divide-gray-200">
          {/* Pet */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <PawPrint className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-muted-foreground">{t('petLabel')}</dt>
              <dd
                className="text-sm font-medium text-foreground truncate"
                data-testid="summary-pet-name"
              >
                {pet.name} <span className="font-normal text-muted-foreground">({pet.species})</span>
              </dd>
            </div>
          </div>

          {/* Consultation type */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <Stethoscope className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-muted-foreground">{t('typeLabel')}</dt>
              <dd
                className="text-sm font-medium text-foreground"
                data-testid="summary-consultation-type"
              >
                {consultationType.name}
              </dd>
            </div>
          </div>

          {/* Vet */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <User className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-muted-foreground">{t('vetLabel')}</dt>
              <dd
                className="text-sm font-medium text-foreground"
                data-testid="summary-vet-name"
              >
                {vetName}
              </dd>
            </div>
          </div>

          {/* Date */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <CalendarDays className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-muted-foreground">{t('dateLabel')}</dt>
              <dd
                className="text-sm font-medium text-foreground"
                data-testid="summary-date"
              >
                {date}
              </dd>
            </div>
          </div>

          {/* Time */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <Clock className="h-4 w-4 text-muted-foreground shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-muted-foreground">{t('timeLabel')}</dt>
              <dd
                className="text-sm font-medium text-foreground"
                data-testid="summary-time"
              >
                {time} <span className="text-xs text-muted-foreground">({t('duration_minutes', { n: consultationType.durationMinutes })})</span>
              </dd>
            </div>
          </div>

          {/* Reason — only if provided */}
          {reason && (
            <div className="flex items-start gap-3 px-5 py-3.5">
              <div className="w-4 shrink-0" />
              <div className="min-w-0 flex-1">
                <dt className="text-xs text-muted-foreground">{t('reasonLabel')}</dt>
                <dd
                  className="text-sm text-foreground mt-0.5"
                  data-testid="summary-reason"
                >
                  {reason}
                </dd>
              </div>
            </div>
          )}
        </dl>
      </div>

      {/* Terms checkbox */}
      <label
        className="flex items-start gap-3 cursor-pointer group"
        data-testid="terms-label"
      >
        <input
          type="checkbox"
          checked={termsAccepted}
          onChange={(e) => setTermsAccepted(e.target.checked)}
          data-testid="terms-checkbox"
          aria-label="I agree to the appointment terms and conditions"
          className={cn(
            'mt-0.5 h-4 w-4 shrink-0 rounded border-border/80 text-primary focus:ring-primary cursor-pointer',
            'accent-primary'
          )}
        />
        <span className="text-sm text-muted-foreground group-hover:text-foreground transition-colors">
          {t('termsText')}
        </span>
      </label>

      {/* Error message */}
      {submitError && (
        <p
          className="text-sm text-red-600 text-center"
          role="alert"
          data-testid="confirmation-error"
        >
          {submitError}
        </p>
      )}

      {/* Actions */}
      <div className="flex flex-col-reverse sm:flex-row gap-3">
        <button
          type="button"
          onClick={onBack}
          disabled={isSubmitting}
          data-testid="confirmation-back-btn"
          aria-label="Go back to slot selection"
          className="flex-1 rounded-xl border border-border/80 px-4 py-2.5 text-sm font-medium text-foreground hover:bg-muted transition-colors disabled:opacity-50 disabled:cursor-not-allowed focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
        >
          {tw('back')}
        </button>
        <button
          type="button"
          onClick={handleConfirm}
          disabled={!termsAccepted || isSubmitting}
          data-testid="confirmation-submit-btn"
          aria-label="Confirm booking"
          aria-busy={isSubmitting}
          className={cn(
            'flex-1 rounded-xl px-4 py-2.5 text-sm font-semibold text-white transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-1',
            termsAccepted && !isSubmitting
              ? 'bg-primary hover:bg-primary/90 cursor-pointer shadow-sm'
              : 'bg-muted-foreground/30 cursor-not-allowed'
          )}
        >
          {isSubmitting ? tw('confirming') : tw('confirm')}
        </button>
      </div>
    </div>
  )
}
