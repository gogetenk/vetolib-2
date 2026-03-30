import { apiGet, apiPost } from './client'
import type { OnboardingStateDto } from './types'

export type { OnboardingStateDto } from './types'

export async function getOnboardingState(): Promise<OnboardingStateDto> {
  return apiGet<OnboardingStateDto>('/api/v1/onboarding')
}

export async function completeStep(stepId: string): Promise<void> {
  return apiPost<void>(`/api/v1/onboarding/steps/${stepId}/complete`, {})
}

export async function dismissBanner(): Promise<void> {
  return apiPost<void>('/api/v1/onboarding/banner/dismiss', {})
}

export async function dismissChecklist(): Promise<void> {
  return apiPost<void>('/api/v1/onboarding/checklist/dismiss', {})
}

export interface CompleteWizardRequest {
  clinicName: string
  timezone: string
  teamMemberEmail?: string
  teamMemberName?: string
  teamMemberRole?: 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
  firstPatientName?: string
  firstPatientSpecies?: string
  firstPatientOwnerName?: string
  firstPatientOwnerPhone?: string
}

export async function completeWizard(data: CompleteWizardRequest): Promise<void> {
  return apiPost<void>('/api/v1/onboarding/wizard/complete', data)
}

export async function skipWizard(): Promise<void> {
  return apiPost<void>('/api/v1/onboarding/wizard/skip', {})
}
