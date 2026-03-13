'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { X } from 'lucide-react'
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { useOnboarding } from '@/hooks/use-onboarding'
import { ChecklistItem } from './ChecklistItem'
import { ChecklistComplete } from './ChecklistComplete'
import type { UserRole } from '@/hooks/use-role'

const STORAGE_KEY = 'vetolib-checklist-dismissed'

// Maps each step ID to the navigation href and role
const STEP_HREFS: Record<string, string> = {
  // Admin steps
  invite_team_member: '/settings/team',
  add_first_patient: '/patients/new',
  book_first_appointment: '/appointments/new',
  create_first_invoice: '/billing/new',
  explore_dashboard: '/dashboard',
  // Vet steps
  view_appointments: '/appointments',
  open_patient_record: '/patients',
  add_medical_record: '/patients',
  write_prescription: '/patients',
  // Receptionist steps
  book_appointment: '/appointments/new',
  check_in_patient: '/appointments',
  create_invoice: '/billing/new',
  send_invoice: '/billing',
  // Assistant steps
  browse_patients: '/patients',
  view_medical_record: '/patients',
  check_today_schedule: '/appointments',
}

interface SetupChecklistProps {
  role: UserRole
}

// eslint-disable-next-line @typescript-eslint/no-unused-vars
export function SetupChecklist({ role }: SetupChecklistProps) {
  const t = useTranslations('onboarding.checklist')
  const { state, loading, completeStep, dismissChecklist } = useOnboarding()
  const [localDismissed, setLocalDismissed] = useState(() => {
    try {
      return localStorage.getItem(STORAGE_KEY) === 'true'
    } catch {
      return false
    }
  })

  if (loading || !state) return null
  if (!state.checklistVisible) return null
  if (localDismissed) return null

  const allCompleted =
    state.steps.length > 0 &&
    state.steps.every((s) => s.completed)

  // Only show the checklist if there are incomplete items
  const hasIncompleteItems = state.steps.some((s) => !s.completed)
  if (!hasIncompleteItems) return null

  const stepsWithHref = state.steps.map((s) => ({
    ...s,
    href: STEP_HREFS[s.id] ?? '/dashboard',
  }))

  function handleDismiss() {
    try {
      localStorage.setItem(STORAGE_KEY, 'true')
    } catch {
      // localStorage unavailable — ignore
    }
    setLocalDismissed(true)
    dismissChecklist().catch(() => {
      // Fail silently
    })
  }

  return (
    <Card data-testid="setup-checklist" id="setup-checklist" className="w-full">
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <h2 className="text-base font-semibold leading-none tracking-tight text-stone-900">
            {t('title')}
          </h2>
          <div className="flex items-center gap-2">
            {!allCompleted && (
              <span
                data-testid="checklist-progress"
                className="text-xs text-stone-500"
              >
                {t('progress', {
                  completed: state.progress.completedSteps,
                  total: state.progress.totalSteps,
                })}
              </span>
            )}
            <button
              type="button"
              data-testid="checklist-close-btn"
              aria-label="Close setup checklist"
              onClick={handleDismiss}
              className="rounded-full p-1 text-stone-400 transition-colors hover:bg-stone-100 hover:text-stone-600 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
            >
              <X className="h-4 w-4" />
            </button>
          </div>
        </div>
      </CardHeader>

      <CardContent className="pt-0">
        {allCompleted ? (
          <ChecklistComplete />
        ) : (
          <>
            <ul className="space-y-1" role="list">
              {stepsWithHref.map((step) => (
                <li key={step.id} role="listitem">
                  <ChecklistItem step={step} onComplete={completeStep} />
                </li>
              ))}
            </ul>

            <div className="mt-4 border-t pt-3">
              <button
                type="button"
                data-testid="checklist-dismiss"
                onClick={handleDismiss}
                className="text-xs text-stone-500 underline-offset-4 hover:text-stone-900 hover:underline"
              >
                {t('dismiss')}
              </button>
            </div>
          </>
        )}
      </CardContent>
    </Card>
  )
}
