'use client'

import { useCallback } from 'react'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { trackEvent } from '@/lib/analytics'

/**
 * Aha moment types — each triggers once per user (persisted in localStorage).
 */
export type AhaMomentType =
  | 'first_appointment'
  | 'first_soap'
  | 'first_whatsapp_reminder'
  | 'ten_patients'
  | 'first_invoice_paid'

const AHA_EVENTS: Record<AhaMomentType, string> = {
  first_appointment: 'aha_first_appointment',
  first_soap: 'aha_first_soap',
  first_whatsapp_reminder: 'aha_first_whatsapp_reminder',
  ten_patients: 'aha_ten_patients',
  first_invoice_paid: 'aha_first_invoice_paid',
}

function storageKey(type: AhaMomentType): string {
  return `aha_seen_${type}`
}

function hasSeenAha(type: AhaMomentType): boolean {
  if (typeof window === 'undefined') return true
  return localStorage.getItem(storageKey(type)) === '1'
}

function markAhaSeen(type: AhaMomentType): void {
  if (typeof window === 'undefined') return
  localStorage.setItem(storageKey(type), '1')
}

/**
 * Hook to trigger "aha moments" — celebratory toasts shown once per milestone.
 *
 * Usage:
 *   const { triggerAha } = useAhaMoment()
 *   triggerAha('first_appointment')
 */
export function useAhaMoment() {
  const t = useTranslations('aha_moments')

  const triggerAha = useCallback(
    (type: AhaMomentType) => {
      if (hasSeenAha(type)) return

      markAhaSeen(type)
      trackEvent(AHA_EVENTS[type])

      toast.success(t(`${type}.title`), {
        description: t(`${type}.description`),
        duration: 6000,
      })
    },
    [t],
  )

  return { triggerAha }
}
