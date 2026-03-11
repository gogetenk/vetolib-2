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
