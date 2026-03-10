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
  const [refetchKey, setRefetchKey] = useState(0)

  const fetchState = useCallback(async () => {
    let cancelled = false
    try {
      const data = await getOnboardingState()
      if (!cancelled) setState(data)
    } catch {
      // Fail silently — onboarding is non-critical
    } finally {
      if (!cancelled) setLoading(false)
    }
    return () => { cancelled = true }
  }, [])

  useEffect(() => {
    let cancelled = false
    setLoading(true)
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
  }, [refetchKey])

  // Expose a refetch function on the window object for E2E tests to trigger re-fetches
  // after direct API mutations (e.g. completing steps via page.evaluate in Playwright).
  useEffect(() => {
    if (typeof window !== 'undefined') {
      ;(window as unknown as Record<string, unknown>).__onboardingRefetch__ = () => {
        setRefetchKey((k) => k + 1)
      }
    }
    return () => {
      if (typeof window !== 'undefined') {
        delete (window as unknown as Record<string, unknown>).__onboardingRefetch__
      }
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

  void fetchState // suppress unused warning — used in refetch pattern

  return { state, loading, dismissBanner, dismissChecklist, completeStep }
}

export function useDismissBanner() {
  const { dismissBanner } = useOnboarding()
  return dismissBanner
}
