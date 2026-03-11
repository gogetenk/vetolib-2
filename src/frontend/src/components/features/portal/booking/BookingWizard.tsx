'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useParams } from 'next/navigation'
import { CheckCircle2 } from 'lucide-react'
import { getConsultationTypes, getVeterinarians } from '@/lib/api/booking'
import type { ConsultationTypeDto, VeterinarianDto, BookingAppointmentDto } from '@/lib/api/booking-types'
import { StepPetSelection } from './StepPetSelection'
import { StepConsultationType } from './StepConsultationType'
import { StepSlotSelection } from './StepSlotSelection'
import { StepConfirmation } from './StepConfirmation'
import { BookingSuccess } from './BookingSuccess'

type WizardStep = 'pet' | 'type' | 'slot' | 'confirm'

const STEPS: WizardStep[] = ['pet', 'type', 'slot', 'confirm']

interface WizardState {
  petId: string | null
  petName: string
  consultationTypeId: string | null
  vetId: string | null
  date: string | null
  time: string | null
  reason: string
}

const INITIAL_STATE: WizardState = {
  petId: null,
  petName: '',
  consultationTypeId: null,
  vetId: null,
  date: null,
  time: null,
  reason: '',
}

export function BookingWizard() {
  const t = useTranslations('portal.booking.wizard')
  const params = useParams<{ clinicSlug: string }>()
  const clinicSlug = params.clinicSlug

  const [currentStep, setCurrentStep] = useState<WizardStep>('pet')
  const [state, setState] = useState<WizardState>(INITIAL_STATE)
  const [completedAppointment, setCompletedAppointment] = useState<BookingAppointmentDto | null>(null)

  // Resolved objects fetched before confirming
  const [resolvedType, setResolvedType] = useState<ConsultationTypeDto | null>(null)
  const [resolvedVet, setResolvedVet] = useState<VeterinarianDto | null>(null)

  const currentStepIndex = STEPS.indexOf(currentStep)

  const stepKeys: Record<WizardStep, string> = {
    pet: t('step_pet'),
    type: t('step_type'),
    slot: t('step_slot'),
    confirm: t('step_confirm'),
  }

  function canGoNext(): boolean {
    switch (currentStep) {
      case 'pet': return !!state.petId
      case 'type': return !!state.consultationTypeId
      case 'slot': return !!state.date && !!state.time
      case 'confirm': return false
      default: return false
    }
  }

  async function handleNext() {
    if (!canGoNext()) return

    const nextIndex = currentStepIndex + 1
    if (nextIndex >= STEPS.length) return

    const nextStep = STEPS[nextIndex]

    // Before confirm step: resolve type and vet objects
    if (nextStep === 'confirm' && state.consultationTypeId) {
      try {
        const [types, vets] = await Promise.all([
          getConsultationTypes(clinicSlug),
          getVeterinarians(clinicSlug),
        ])
        setResolvedType(types.find(ct => ct.id === state.consultationTypeId) ?? null)
        setResolvedVet(state.vetId ? (vets.find(v => v.id === state.vetId) ?? null) : null)
      } catch {
        // Proceed; StepConfirmation handles null gracefully
      }
    }

    setCurrentStep(nextStep)
  }

  function handleBack() {
    const prevIndex = currentStepIndex - 1
    if (prevIndex < 0) return
    setCurrentStep(STEPS[prevIndex])
  }

  function handleSlotUnavailable() {
    setState(s => ({ ...s, date: null, time: null }))
    setCurrentStep('slot')
  }

  if (completedAppointment) {
    return (
      <div className="mx-auto max-w-lg px-4 py-8">
        <BookingSuccess appointment={completedAppointment} />
      </div>
    )
  }

  return (
    <div data-testid="booking-wizard" className="mx-auto max-w-lg px-4 py-8 space-y-6">
      {/* Header */}
      <div>
        <h1 data-testid="wizard-title" className="text-2xl font-bold text-foreground">
          {t('title')}
        </h1>
        <p className="text-sm text-muted-foreground mt-1">
          {t('step_of', { current: currentStepIndex + 1, total: STEPS.length })}
        </p>
      </div>

      {/* Stepper */}
      <nav data-testid="wizard-stepper" aria-label="Booking steps">
        <ol className="flex items-center">
          {STEPS.map((step, index) => {
            const isCompleted = index < currentStepIndex
            const isActive = step === currentStep

            return (
              <li key={step} className="flex items-center flex-1">
                <div
                  data-testid={`step-indicator-${step}`}
                  className={[
                    'flex items-center gap-1.5 text-xs font-medium',
                    isActive
                      ? 'text-primary'
                      : isCompleted
                      ? 'text-primary/70'
                      : 'text-muted-foreground',
                  ].join(' ')}
                >
                  <div
                    className={[
                      'flex-shrink-0 flex items-center justify-center w-6 h-6 rounded-full',
                      isActive
                        ? 'bg-primary text-primary-foreground'
                        : isCompleted
                        ? 'bg-primary/20 text-primary'
                        : 'bg-muted text-muted-foreground',
                    ].join(' ')}
                  >
                    {isCompleted ? (
                      <CheckCircle2 className="h-4 w-4" />
                    ) : (
                      <span className="text-xs">{index + 1}</span>
                    )}
                  </div>
                  <span className="hidden sm:block text-xs">{stepKeys[step]}</span>
                </div>
                {index < STEPS.length - 1 && (
                  <div
                    className={[
                      'flex-1 h-0.5 mx-2',
                      index < currentStepIndex ? 'bg-primary/50' : 'bg-border',
                    ].join(' ')}
                  />
                )}
              </li>
            )
          })}
        </ol>
      </nav>

      {/* Step content */}
      <div data-testid="wizard-step-content" className="space-y-2">
        <h2 data-testid="step-title" className="text-lg font-semibold text-foreground">
          {stepKeys[currentStep]}
        </h2>

        {currentStep === 'pet' && (
          <StepPetSelection
            selectedPetId={state.petId}
            onSelect={(petId, petName) => setState(s => ({ ...s, petId, petName }))}
          />
        )}

        {currentStep === 'type' && (
          <StepConsultationType
            clinicSlug={clinicSlug}
            selectedTypeId={state.consultationTypeId}
            selectedVetId={state.vetId}
            reason={state.reason}
            onTypeSelect={consultationTypeId => setState(s => ({ ...s, consultationTypeId }))}
            onVetSelect={vetId => setState(s => ({ ...s, vetId }))}
            onReasonChange={reason => setState(s => ({ ...s, reason }))}
          />
        )}

        {currentStep === 'slot' && state.consultationTypeId && (
          <StepSlotSelection
            clinicSlug={clinicSlug}
            consultationTypeId={state.consultationTypeId}
            veterinarianId={state.vetId}
            selectedDate={state.date}
            selectedTime={state.time}
            onSlotSelect={(date, time) => setState(s => ({ ...s, date, time }))}
          />
        )}

        {currentStep === 'confirm' && state.petId && resolvedType && state.date && state.time && (
          <StepConfirmation
            petId={state.petId}
            petName={state.petName}
            consultationType={resolvedType}
            vet={resolvedVet}
            date={state.date}
            time={state.time}
            reason={state.reason}
            onSuccess={appointment => setCompletedAppointment(appointment)}
            onSlotUnavailable={handleSlotUnavailable}
          />
        )}
      </div>

      {/* Navigation — not shown on confirm (it has its own submit) */}
      {currentStep !== 'confirm' && (
        <div className="flex gap-3 pt-2">
          {currentStepIndex > 0 && (
            <button
              data-testid="wizard-back-btn"
              type="button"
              onClick={handleBack}
              className="flex-1 rounded-lg border border-border px-4 py-3 text-sm font-medium hover:bg-muted transition-colors focus:outline-none focus:ring-2 focus:ring-primary"
            >
              {t('back')}
            </button>
          )}
          <button
            data-testid="wizard-next-btn"
            type="button"
            onClick={handleNext}
            disabled={!canGoNext()}
            className={[
              'flex-1 rounded-lg px-4 py-3 text-sm font-semibold transition-colors',
              'focus:outline-none focus:ring-2 focus:ring-primary',
              canGoNext()
                ? 'bg-primary text-primary-foreground hover:bg-primary/90'
                : 'bg-muted text-muted-foreground cursor-not-allowed opacity-60',
            ].join(' ')}
          >
            {t('next')}
          </button>
        </div>
      )}

      {/* Back button on confirm step */}
      {currentStep === 'confirm' && (
        <button
          data-testid="wizard-back-btn"
          type="button"
          onClick={handleBack}
          className="w-full rounded-lg border border-border px-4 py-3 text-sm font-medium hover:bg-muted transition-colors focus:outline-none focus:ring-2 focus:ring-primary mt-2"
        >
          {t('back')}
        </button>
      )}
    </div>
  )
}
