'use client'

import { useState, useRef, useEffect } from 'react'
import { useTranslations } from 'next-intl'
import { cn } from '@/lib/utils'
import { StepPetSelection } from './StepPetSelection'
import { StepConsultationType } from './StepConsultationType'
import { StepSlotSelection } from './StepSlotSelection'
import { StepConfirmation } from './StepConfirmation'
import { BookingSuccess } from './BookingSuccess'
import type {
  BookingPetDto,
  ConsultationTypeDto,
  VeterinarianDto,
  BookingSlot,
  BookingAppointmentDto,
} from '@/lib/api/booking'

// ─── Types ────────────────────────────────────────────────────────────────────

interface BookingWizardProps {
  locale: string
  clinicSlug: string
}

type WizardStep = 1 | 2 | 3 | 4

interface WizardState {
  currentStep: WizardStep
  selectedPet: BookingPetDto | null
  selectedConsultationType: ConsultationTypeDto | null
  /** null = no preference; set when user picks a specific vet */
  selectedVet: VeterinarianDto | null
  reason: string
  selectedSlot: BookingSlot | null
  createdAppointment: BookingAppointmentDto | null
}

// ─── Step indicator ──────────────────────────────────────────────────────────

const STEP_KEYS: { step: WizardStep; key: string }[] = [
  { step: 1, key: 'stepPet' },
  { step: 2, key: 'stepType' },
  { step: 3, key: 'stepSlot' },
  { step: 4, key: 'stepConfirm' },
]

function StepIndicator({
  currentStep,
  onStepClick,
}: {
  currentStep: WizardStep
  onStepClick: (step: WizardStep) => void
}) {
  const t = useTranslations('portal.booking.wizard')
  return (
    <nav aria-label="Booking wizard steps" data-testid="wizard-stepper">
      <ol className="flex items-center gap-0">
        {STEP_KEYS.map(({ step, key }, idx) => {
          const label = t(key)
          const isDone = currentStep > step
          const isCurrent = currentStep === step
          const isClickable = step < currentStep

          return (
            <li key={step} className="flex items-center flex-1">
              <button
                type="button"
                onClick={() => isClickable && onStepClick(step)}
                disabled={!isClickable}
                data-testid={`wizard-step-${step}`}
                aria-current={isCurrent ? 'step' : undefined}
                aria-label={`Step ${step}: ${label}${isDone ? ' (completed)' : isCurrent ? ' (current)' : ''}`}
                className={cn(
                  'flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-xs font-bold transition-all duration-300 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1',
                  isDone
                    ? 'bg-emerald-600 text-white cursor-pointer hover:bg-emerald-700 scale-100'
                    : isCurrent
                    ? 'bg-emerald-600 text-white cursor-default shadow-md ring-4 ring-emerald-100 scale-110'
                    : 'bg-gray-200 text-gray-400 cursor-not-allowed scale-100'
                )}
              >
                {isDone ? (
                  <svg viewBox="0 0 16 16" fill="none" className="h-4 w-4 animate-[scale-in_0.3s_ease-out]" aria-hidden="true">
                    <path
                      d="M3 8l3.5 3.5L13 5"
                      stroke="currentColor"
                      strokeWidth="2"
                      strokeLinecap="round"
                      strokeLinejoin="round"
                    />
                  </svg>
                ) : (
                  step
                )}
              </button>

              <span
                className={cn(
                  'ml-1 mr-2 hidden sm:block text-xs font-medium whitespace-nowrap transition-colors duration-300',
                  isCurrent ? 'text-emerald-700' : isDone ? 'text-gray-600' : 'text-gray-400'
                )}
                aria-hidden="true"
              >
                {label}
              </span>

              {idx < STEP_KEYS.length - 1 && (
                <div
                  className="flex-1 h-0.5 mx-1 bg-gray-200 rounded-full overflow-hidden"
                  aria-hidden="true"
                >
                  <div
                    className={cn(
                      'h-full rounded-full transition-all duration-500 ease-out',
                      currentStep > step ? 'w-full bg-emerald-400' : 'w-0 bg-emerald-400'
                    )}
                  />
                </div>
              )}
            </li>
          )
        })}
      </ol>
    </nav>
  )
}

// ─── Step titles ──────────────────────────────────────────────────────────────

const STEP_TITLE_KEYS: Record<WizardStep, string> = {
  1: 'step1Title',
  2: 'step2Title',
  3: 'step3Title',
  4: 'step4Title',
}

// ─── BookingWizard ────────────────────────────────────────────────────────────

export function BookingWizard({ locale, clinicSlug }: BookingWizardProps) {
  const t = useTranslations('portal.booking.wizard')
  const [state, setState] = useState<WizardState>({
    currentStep: 1,
    selectedPet: null,
    selectedConsultationType: null,
    selectedVet: null,
    reason: '',
    selectedSlot: null,
    createdAppointment: null,
  })

  // Track direction for slide animation
  const [slideDirection, setSlideDirection] = useState<'left' | 'right'>('left')
  const [isTransitioning, setIsTransitioning] = useState(false)
  const transitionTimeout = useRef<NodeJS.Timeout | null>(null)

  // Stable cached vet list — populated when step 2 mounts; used for name resolution
  const [cachedVets, setCachedVets] = useState<VeterinarianDto[]>([])

  function goToStep(step: WizardStep) {
    const direction = step > state.currentStep ? 'left' : 'right'
    setSlideDirection(direction)
    setIsTransitioning(true)

    if (transitionTimeout.current) clearTimeout(transitionTimeout.current)

    transitionTimeout.current = setTimeout(() => {
      setState((s) => ({ ...s, currentStep: step }))
      setIsTransitioning(false)
    }, 200)
  }

  // Clean up timeout on unmount
  useEffect(() => {
    return () => {
      if (transitionTimeout.current) clearTimeout(transitionTimeout.current)
    }
  }, [])

  function handleNext() {
    if (state.currentStep < 4) {
      goToStep((state.currentStep + 1) as WizardStep)
    }
  }

  function handleBack() {
    if (state.currentStep > 1) {
      goToStep((state.currentStep - 1) as WizardStep)
    }
  }

  // Step 1
  function handlePetSelect(pet: BookingPetDto) {
    setState((s) => ({ ...s, selectedPet: pet }))
  }

  // Step 2
  function handleConsultationTypeSelect(type: ConsultationTypeDto) {
    setState((s) => ({ ...s, selectedConsultationType: type }))
  }

  function handleVetChange(vetId: string | null) {
    const vet = vetId === null ? null : (cachedVets.find((v) => v.id === vetId) ?? null)
    setState((s) => ({ ...s, selectedVet: vet }))
  }

  function handleReasonChange(reason: string) {
    setState((s) => ({ ...s, reason }))
  }

  // Step 3
  function handleSlotSelect(slot: BookingSlot) {
    setState((s) => ({ ...s, selectedSlot: slot }))
  }

  // Step 4
  function handleBookingSuccess(appointment: BookingAppointmentDto) {
    setState((s) => ({ ...s, createdAppointment: appointment }))
  }

  const canProceedStep1 = state.selectedPet !== null
  const canProceedStep2 = state.selectedConsultationType !== null
  const canProceedStep3 = state.selectedSlot !== null

  // ─── Success screen ───────────────────────────────────────────────────────

  if (state.createdAppointment && state.selectedPet && state.selectedConsultationType) {
    const vetName =
      state.selectedVet?.name ||
      state.selectedSlot?.vetName ||
      t('assignedVet')

    return (
      <BookingSuccess
        appointment={state.createdAppointment}
        pet={state.selectedPet}
        consultationType={state.selectedConsultationType}
        vetName={vetName}
        locale={locale}
        clinicSlug={clinicSlug}
      />
    )
  }

  // ─── Step 2 wrapper that also captures vet list ───────────────────────────

  function renderStep2() {
    return (
      <StepConsultationTypeWithVetCache
        selectedTypeId={state.selectedConsultationType?.id ?? null}
        selectedVetId={state.selectedVet?.id ?? null}
        reason={state.reason}
        onTypeSelect={handleConsultationTypeSelect}
        onVetChange={handleVetChange}
        onReasonChange={handleReasonChange}
        onVetsCached={setCachedVets}
      />
    )
  }

  // ─── Wizard layout ────────────────────────────────────────────────────────

  return (
    <div className="space-y-6" data-testid="booking-wizard">
      <StepIndicator currentStep={state.currentStep} onStepClick={goToStep} />

      <h2 className="text-lg font-semibold text-gray-900" data-testid="wizard-step-title">
        {t(STEP_TITLE_KEYS[state.currentStep])}
      </h2>

      <div
        data-testid="wizard-step-content"
        className={cn(
          'transition-all duration-300 ease-out',
          isTransitioning
            ? slideDirection === 'left'
              ? 'opacity-0 -translate-x-4'
              : 'opacity-0 translate-x-4'
            : 'opacity-100 translate-x-0'
        )}
      >
        {state.currentStep === 1 && (
          <StepPetSelection
            selectedPetId={state.selectedPet?.id ?? null}
            onSelect={handlePetSelect}
          />
        )}

        {state.currentStep === 2 && renderStep2()}

        {state.currentStep === 3 && (
          <StepSlotSelection
            selectedSlot={state.selectedSlot}
            vetId={state.selectedVet?.id ?? null}
            reason={state.reason}
            onSlotSelect={handleSlotSelect}
          />
        )}

        {state.currentStep === 4 &&
          state.selectedPet &&
          state.selectedConsultationType &&
          state.selectedSlot && (
            <StepConfirmation
              pet={state.selectedPet}
              consultationType={state.selectedConsultationType}
              vet={state.selectedVet}
              slot={state.selectedSlot}
              reason={state.reason}
              onSuccess={handleBookingSuccess}
              onBack={handleBack}
            />
          )}
      </div>

      {/* Navigation buttons — not shown on step 4 (StepConfirmation has its own) */}
      {state.currentStep < 4 && (
        <div className="flex flex-col-reverse sm:flex-row gap-3">
          {state.currentStep > 1 && (
            <button
              type="button"
              onClick={handleBack}
              data-testid="wizard-back-btn"
              aria-label="Go to previous step"
              className="flex-1 sm:flex-none sm:w-28 rounded-lg border border-gray-300 px-4 py-2.5 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
            >
              {t('back')}
            </button>
          )}

          <button
            type="button"
            onClick={handleNext}
            disabled={
              (state.currentStep === 1 && !canProceedStep1) ||
              (state.currentStep === 2 && !canProceedStep2) ||
              (state.currentStep === 3 && !canProceedStep3)
            }
            data-testid="wizard-next-btn"
            aria-label={state.currentStep === 3 ? 'Review booking' : 'Go to next step'}
            className={cn(
              'flex-1 rounded-lg px-4 py-2.5 text-sm font-semibold text-white transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 focus-visible:ring-offset-1',
              (state.currentStep === 1 && canProceedStep1) ||
              (state.currentStep === 2 && canProceedStep2) ||
              (state.currentStep === 3 && canProceedStep3)
                ? 'bg-emerald-600 hover:bg-emerald-700 cursor-pointer shadow-sm'
                : 'bg-gray-300 cursor-not-allowed'
            )}
          >
            {state.currentStep === 3 ? t('review') : t('next')}
          </button>
        </div>
      )}
    </div>
  )
}

// ─── Step 2 wrapper — captures vet list for name resolution ───────────────────

/**
 * Wraps StepConsultationType and exposes the loaded vet list
 * so the parent wizard can resolve full VeterinarianDto objects.
 */
function StepConsultationTypeWithVetCache({
  selectedTypeId,
  selectedVetId,
  reason,
  onTypeSelect,
  onVetChange,
  onReasonChange,
  onVetsCached,
}: {
  selectedTypeId: string | null
  selectedVetId: string | null
  reason: string
  onTypeSelect: (type: ConsultationTypeDto) => void
  onVetChange: (vetId: string | null) => void
  onReasonChange: (reason: string) => void
  onVetsCached: (vets: VeterinarianDto[]) => void
}) {
  return (
    <StepConsultationType
      selectedTypeId={selectedTypeId}
      selectedVetId={selectedVetId}
      reason={reason}
      onTypeSelect={onTypeSelect}
      onVetChange={onVetChange}
      onReasonChange={onReasonChange}
      onVetsCached={onVetsCached}
    />
  )
}
