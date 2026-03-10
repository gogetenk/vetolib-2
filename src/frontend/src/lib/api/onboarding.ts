import { apiGet, apiPost } from './client'
import type { OnboardingStateDto } from './types'

export type { OnboardingStateDto } from './types'

export async function getOnboardingState(): Promise<OnboardingStateDto> {
  return apiGet<OnboardingStateDto>('/api/onboarding')
}

export async function completeStep(stepId: string): Promise<void> {
  return apiPost<void>(`/api/onboarding/steps/${stepId}/complete`, {})
}

export async function dismissBanner(): Promise<void> {
  return apiPost<void>('/api/onboarding/banner/dismiss', {})
}

export async function dismissChecklist(): Promise<void> {
  return apiPost<void>('/api/onboarding/checklist/dismiss', {})
}
