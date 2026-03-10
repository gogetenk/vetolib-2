'use client'

import { useState, useEffect, useCallback } from 'react'
import {
  getOnboardingState,
  completeStep as apiCompleteStep,
  dismissBanner as apiDismissBanner,
  dismissChecklist as apiDismissChecklist,
} from '@/lib/api/onboarding'
import type { OnboardingStateDto } from '@/lib/api/onboarding'

interface UseOnboardingReturn {
  state: OnboardingStateDto | null
  loading: boolean
  dismissBanner: () => Promise<void>
  dismissChecklist: () => Promise<void>
  completeStep: (stepId: string) => Promise<void>
}

export function useOnboarding(): UseOnboardingReturn {
  const [state, setState] = useState<OnboardingStateDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    getOnboardingState()
      .then((data) => {
        if (!cancelled) {
          setState(data)
        }
      })
      .catch(() => {
        // Fail silently — onboarding is non-critical
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })
    return () => {
      cancelled = true
    }
  }, [])

  const dismissBanner = useCallback(async () => {
    await apiDismissBanner()
    setState((prev) =>
      prev ? { ...prev, welcomeBannerVisible: false } : prev
    )
  }, [])

  const dismissChecklist = useCallback(async () => {
    await apiDismissChecklist()
    setState((prev) =>
      prev ? { ...prev, checklistVisible: false } : prev
    )
  }, [])

  const completeStep = useCallback(async (stepId: string) => {
    await apiCompleteStep(stepId)
    setState((prev) => {
      if (!prev) return prev
      const updatedSteps = prev.steps.map((s) =>
        s.id === stepId ? { ...s, completed: true } : s
      )
      const completed = updatedSteps.filter((s) => s.completed).length
      const total = updatedSteps.length
      return {
        ...prev,
        steps: updatedSteps,
        progress: {
          totalSteps: total,
          completedSteps: completed,
          percentComplete: total === 0 ? 0 : Math.round((completed / total) * 100),
        },
      }
    })
  }, [])

  return { state, loading, dismissBanner, dismissChecklist, completeStep }
}

export function useDismissBanner() {
  const { dismissBanner } = useOnboarding()
  return dismissBanner
}
