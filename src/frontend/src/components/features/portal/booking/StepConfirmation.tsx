'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { CalendarDays, Clock, Stethoscope, PawPrint, User } from 'lucide-react'
import { cn } from '@/lib/utils'
import { createBookingAppointment } from '@/lib/api/booking'
import type { BookingPetDto, ConsultationTypeDto, VeterinarianDto, BookingSlot, BookingAppointmentDto } from '@/lib/api/booking'

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

  const { date, time } = formatDateTime(slot.startsAt)
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
        className="rounded-xl border border-stone-200 bg-stone-50 overflow-hidden"
        data-testid="booking-summary-card"
      >
        <div className="bg-emerald-600 px-5 py-3">
          <h3 className="text-sm font-semibold text-white">{t('summaryTitle')}</h3>
        </div>
        <dl className="divide-y divide-gray-200">
          {/* Pet */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <PawPrint className="h-4 w-4 text-stone-400 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-stone-500">{t('petLabel')}</dt>
              <dd
                className="text-sm font-medium text-stone-900 truncate"
                data-testid="summary-pet-name"
              >
                {pet.name} <span className="font-normal text-stone-500">({pet.species})</span>
              </dd>
            </div>
          </div>

          {/* Consultation type */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <Stethoscope className="h-4 w-4 text-stone-400 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-stone-500">{t('typeLabel')}</dt>
              <dd
                className="text-sm font-medium text-stone-900"
                data-testid="summary-consultation-type"
              >
                {consultationType.name}
              </dd>
            </div>
          </div>

          {/* Vet */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <User className="h-4 w-4 text-stone-400 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-stone-500">{t('vetLabel')}</dt>
              <dd
                className="text-sm font-medium text-stone-900"
                data-testid="summary-vet-name"
              >
                {vetName}
              </dd>
            </div>
          </div>

          {/* Date */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <CalendarDays className="h-4 w-4 text-stone-400 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-stone-500">{t('dateLabel')}</dt>
              <dd
                className="text-sm font-medium text-stone-900"
                data-testid="summary-date"
              >
                {date}
              </dd>
            </div>
          </div>

          {/* Time */}
          <div className="flex items-center gap-3 px-5 py-3.5">
            <Clock className="h-4 w-4 text-stone-400 shrink-0" aria-hidden="true" />
            <div className="min-w-0 flex-1">
              <dt className="text-xs text-stone-500">{t('timeLabel')}</dt>
              <dd
                className="text-sm font-medium text-stone-900"
                data-testid="summary-time"
              >
                {time} <span className="text-xs text-stone-500">({consultationType.durationMinutes} min)</span>
              </dd>
            </div>
          </div>

          {/* Reason — only if provided */}
          {reason && (
            <div className="flex items-start gap-3 px-5 py-3.5">
              <div className="w-4 shrink-0" />
              <div className="min-w-0 flex-1">
                <dt className="text-xs text-stone-500">{t('reasonLabel')}</dt>
                <dd
                  className="text-sm text-stone-700 mt-0.5"
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
            'mt-0.5 h-4 w-4 shrink-0 rounded border-stone-300 text-emerald-600 focus:ring-emerald-500 cursor-pointer',
            'accent-emerald-600'
          )}
        />
        <span className="text-sm text-stone-600 group-hover:text-stone-900 transition-colors">
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
          className="flex-1 rounded-lg border border-stone-300 px-4 py-2.5 text-sm font-medium text-stone-700 hover:bg-stone-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
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
            'flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold text-white transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1',
            termsAccepted && !isSubmitting
              ? 'bg-emerald-600 hover:bg-emerald-700 cursor-pointer shadow-sm'
              : 'bg-stone-300 cursor-not-allowed'
          )}
        >
          {isSubmitting ? tw('confirming') : tw('confirm')}
        </button>
      </div>
    </div>
  )
}
