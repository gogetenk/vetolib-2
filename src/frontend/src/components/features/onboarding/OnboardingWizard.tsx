'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Building2, Users, PawPrint, ChevronRight, ChevronLeft, Check, X } from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { useOnboarding } from '@/hooks/use-onboarding'
import type { CompleteWizardRequest } from '@/lib/api/onboarding'

const TIMEZONES = [
  'Asia/Dubai',
  'Asia/Riyadh',
  'Asia/Kuwait',
  'Asia/Bahrain',
  'Asia/Qatar',
  'Asia/Muscat',
  'Europe/London',
  'Europe/Paris',
  'America/New_York',
  'America/Los_Angeles',
] as const

type WizardStep = 'welcome' | 'team' | 'patient'

const STEPS: WizardStep[] = ['welcome', 'team', 'patient']

interface OnboardingWizardProps {
  initialClinicName?: string
}

export function OnboardingWizard({ initialClinicName = '' }: OnboardingWizardProps) {
  const t = useTranslations('onboarding.wizard')
  const { state, loading, completeWizard, skipWizard } = useOnboarding()

  const [currentStep, setCurrentStep] = useState<WizardStep>('welcome')
  const [submitting, setSubmitting] = useState(false)

  // Form state
  const [clinicName, setClinicName] = useState(initialClinicName)
  const [timezone, setTimezone] = useState('Asia/Dubai')
  const [teamEmail, setTeamEmail] = useState('')
  const [teamName, setTeamName] = useState('')
  const [teamRole, setTeamRole] = useState<'VET' | 'ASSISTANT' | 'RECEPTIONIST'>('VET')
  const [patientName, setPatientName] = useState('')
  const [patientSpecies, setPatientSpecies] = useState('')
  const [ownerName, setOwnerName] = useState('')
  const [ownerPhone, setOwnerPhone] = useState('')

  if (loading || !state) return null
  if (state.wizardCompleted) return null

  const stepIndex = STEPS.indexOf(currentStep)
  const isFirstStep = stepIndex === 0
  const isLastStep = stepIndex === STEPS.length - 1

  function handleNext() {
    if (!isLastStep) {
      setCurrentStep(STEPS[stepIndex + 1])
    }
  }

  function handleBack() {
    if (!isFirstStep) {
      setCurrentStep(STEPS[stepIndex - 1])
    }
  }

  async function handleFinish() {
    setSubmitting(true)
    try {
      const data: CompleteWizardRequest = {
        clinicName: clinicName || initialClinicName,
        timezone,
      }
      if (teamEmail.trim()) {
        data.teamMemberEmail = teamEmail.trim()
        data.teamMemberName = teamName.trim() || undefined
        data.teamMemberRole = teamRole
      }
      if (patientName.trim()) {
        data.firstPatientName = patientName.trim()
        data.firstPatientSpecies = patientSpecies.trim() || undefined
        data.firstPatientOwnerName = ownerName.trim() || undefined
        data.firstPatientOwnerPhone = ownerPhone.trim() || undefined
      }
      await completeWizard(data)
    } catch {
      // Fail silently — wizard is non-critical
    } finally {
      setSubmitting(false)
    }
  }

  async function handleSkip() {
    setSubmitting(true)
    try {
      await skipWizard()
    } catch {
      // Fail silently
    } finally {
      setSubmitting(false)
    }
  }

  function getStepIcon(step: WizardStep) {
    switch (step) {
      case 'welcome':
        return <Building2 className="h-5 w-5" />
      case 'team':
        return <Users className="h-5 w-5" />
      case 'patient':
        return <PawPrint className="h-5 w-5" />
    }
  }

  return (
    <div
      data-testid="onboarding-wizard"
      className="fixed inset-0 z-50 flex items-center justify-center bg-background/80 backdrop-blur-sm"
    >
      <Card className="w-full max-w-lg mx-4 shadow-xl border-border/80">
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <h2
              data-testid="onboarding-wizard-title"
              className="text-[18px] font-bold text-foreground"
            >
              {t('title')}
            </h2>
            <button
              type="button"
              data-testid="onboarding-wizard-skip-btn"
              onClick={handleSkip}
              disabled={submitting}
              aria-label={t('skip')}
              className="rounded-full p-1 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30"
            >
              <X className="h-4 w-4" />
            </button>
          </div>

          {/* Step indicators */}
          <div className="flex items-center gap-2 mt-3" data-testid="onboarding-wizard-steps">
            {STEPS.map((step, idx) => {
              const isActive = idx === stepIndex
              const isCompleted = idx < stepIndex
              return (
                <div key={step} className="flex items-center gap-2 flex-1">
                  <div
                    data-testid={`onboarding-wizard-step-indicator-${step}`}
                    className={[
                      'flex h-8 w-8 shrink-0 items-center justify-center rounded-full border-2 transition-all',
                      isActive
                        ? 'border-primary bg-primary text-primary-foreground'
                        : isCompleted
                          ? 'border-green-500 bg-green-500 text-white'
                          : 'border-border/80 bg-card text-muted-foreground',
                    ].join(' ')}
                  >
                    {isCompleted ? (
                      <Check className="h-4 w-4" strokeWidth={3} />
                    ) : (
                      getStepIcon(step)
                    )}
                  </div>
                  <span
                    className={[
                      'text-[12px] font-medium hidden sm:inline',
                      isActive ? 'text-foreground' : 'text-muted-foreground',
                    ].join(' ')}
                  >
                    {t(`steps.${step}.label`)}
                  </span>
                  {idx < STEPS.length - 1 && (
                    <div
                      className={[
                        'flex-1 h-0.5 rounded-full',
                        isCompleted ? 'bg-green-500' : 'bg-border/60',
                      ].join(' ')}
                    />
                  )}
                </div>
              )
            })}
          </div>
        </CardHeader>

        <CardContent className="pt-0">
          {/* Step: Welcome */}
          {currentStep === 'welcome' && (
            <div data-testid="onboarding-wizard-step-welcome" className="space-y-4">
              <p className="text-[14px] text-muted-foreground">
                {t('steps.welcome.description')}
              </p>
              <div className="space-y-2">
                <Label htmlFor="wizard-clinic-name">{t('steps.welcome.clinic_name')}</Label>
                <Input
                  id="wizard-clinic-name"
                  data-testid="onboarding-wizard-clinic-name"
                  value={clinicName}
                  onChange={(e) => setClinicName(e.target.value)}
                  placeholder={t('steps.welcome.clinic_name_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-timezone">{t('steps.welcome.timezone')}</Label>
                <select
                  id="wizard-timezone"
                  data-testid="onboarding-wizard-timezone"
                  value={timezone}
                  onChange={(e) => setTimezone(e.target.value)}
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                >
                  {TIMEZONES.map((tz) => (
                    <option key={tz} value={tz}>
                      {tz.replace(/_/g, ' ')}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          )}

          {/* Step: Team */}
          {currentStep === 'team' && (
            <div data-testid="onboarding-wizard-step-team" className="space-y-4">
              <p className="text-[14px] text-muted-foreground">
                {t('steps.team.description')}
              </p>
              <div className="space-y-2">
                <Label htmlFor="wizard-team-email">{t('steps.team.email')}</Label>
                <Input
                  id="wizard-team-email"
                  data-testid="onboarding-wizard-team-email"
                  type="email"
                  value={teamEmail}
                  onChange={(e) => setTeamEmail(e.target.value)}
                  placeholder={t('steps.team.email_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-team-name">{t('steps.team.name')}</Label>
                <Input
                  id="wizard-team-name"
                  data-testid="onboarding-wizard-team-name"
                  value={teamName}
                  onChange={(e) => setTeamName(e.target.value)}
                  placeholder={t('steps.team.name_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-team-role">{t('steps.team.role')}</Label>
                <select
                  id="wizard-team-role"
                  data-testid="onboarding-wizard-team-role"
                  value={teamRole}
                  onChange={(e) => setTeamRole(e.target.value as 'VET' | 'ASSISTANT' | 'RECEPTIONIST')}
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
                >
                  <option value="VET">{t('steps.team.role_vet')}</option>
                  <option value="ASSISTANT">{t('steps.team.role_assistant')}</option>
                  <option value="RECEPTIONIST">{t('steps.team.role_receptionist')}</option>
                </select>
              </div>
            </div>
          )}

          {/* Step: Patient */}
          {currentStep === 'patient' && (
            <div data-testid="onboarding-wizard-step-patient" className="space-y-4">
              <p className="text-[14px] text-muted-foreground">
                {t('steps.patient.description')}
              </p>
              <div className="space-y-2">
                <Label htmlFor="wizard-patient-name">{t('steps.patient.pet_name')}</Label>
                <Input
                  id="wizard-patient-name"
                  data-testid="onboarding-wizard-patient-name"
                  value={patientName}
                  onChange={(e) => setPatientName(e.target.value)}
                  placeholder={t('steps.patient.pet_name_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-patient-species">{t('steps.patient.species')}</Label>
                <Input
                  id="wizard-patient-species"
                  data-testid="onboarding-wizard-patient-species"
                  value={patientSpecies}
                  onChange={(e) => setPatientSpecies(e.target.value)}
                  placeholder={t('steps.patient.species_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-owner-name">{t('steps.patient.owner_name')}</Label>
                <Input
                  id="wizard-owner-name"
                  data-testid="onboarding-wizard-owner-name"
                  value={ownerName}
                  onChange={(e) => setOwnerName(e.target.value)}
                  placeholder={t('steps.patient.owner_name_placeholder')}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="wizard-owner-phone">{t('steps.patient.owner_phone')}</Label>
                <Input
                  id="wizard-owner-phone"
                  data-testid="onboarding-wizard-owner-phone"
                  type="tel"
                  value={ownerPhone}
                  onChange={(e) => setOwnerPhone(e.target.value)}
                  placeholder={t('steps.patient.owner_phone_placeholder')}
                />
              </div>
            </div>
          )}

          {/* Navigation buttons */}
          <div className="flex items-center justify-between mt-6 pt-4 border-t">
            <div>
              {!isFirstStep && (
                <Button
                  type="button"
                  variant="outline"
                  data-testid="onboarding-wizard-back-btn"
                  onClick={handleBack}
                  disabled={submitting}
                  className="rounded-xl"
                >
                  <ChevronLeft className="h-4 w-4 me-1" />
                  {t('back')}
                </Button>
              )}
            </div>
            <div className="flex items-center gap-2">
              {currentStep !== 'welcome' && (
                <Button
                  type="button"
                  variant="ghost"
                  data-testid="onboarding-wizard-skip-step-btn"
                  onClick={isLastStep ? handleFinish : handleNext}
                  disabled={submitting}
                  className="text-muted-foreground"
                >
                  {t('skip_step')}
                </Button>
              )}
              {isLastStep ? (
                <Button
                  type="button"
                  data-testid="onboarding-wizard-finish-btn"
                  onClick={handleFinish}
                  disabled={submitting}
                  className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
                >
                  {submitting ? t('finishing') : t('finish')}
                </Button>
              ) : (
                <Button
                  type="button"
                  data-testid="onboarding-wizard-next-btn"
                  onClick={handleNext}
                  disabled={submitting}
                  className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
                >
                  {t('next')}
                  <ChevronRight className="h-4 w-4 ms-1" />
                </Button>
              )}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
