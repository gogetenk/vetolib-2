'use client'

import { useTranslations } from 'next-intl'
import { CheckCircle2 } from 'lucide-react'

export function ChecklistComplete() {
  const t = useTranslations('onboarding.checklist')

  return (
    <div
      data-testid="checklist-complete-message"
      className="flex flex-col items-center gap-3 py-6 text-center"
    >
      <CheckCircle2
        className="h-12 w-12 text-green-500"
        aria-hidden="true"
      />
      <div className="space-y-1">
        <p className="text-base font-semibold text-stone-900">
          {t('completed_title')}
        </p>
        <p className="text-sm text-muted-foreground">
          {t('completed_body')}
        </p>
      </div>
    </div>
  )
}
