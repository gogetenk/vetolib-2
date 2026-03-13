'use client'

import { useTranslations } from 'next-intl'
import type { MessageCategory } from '@/lib/api/messaging-types'

// Map from user-friendly option key to the API category
const CATEGORY_OPTIONS: { label: string; category: MessageCategory }[] = [
  { label: 'MedicalUrgency', category: 'MedicalUrgency' },
  { label: 'PostOperativeFollowUp', category: 'PostOperativeFollowUp' },
  { label: 'AppointmentRequest', category: 'AppointmentRequest' },
  { label: 'Administrative', category: 'Administrative' },
  { label: 'Feedback', category: 'Feedback' },
  { label: 'Other', category: 'Other' },
]

interface CategorySelectorProps {
  value: MessageCategory | null
  onChange: (category: MessageCategory) => void
  error?: string
}

export function CategorySelector({ value, onChange, error }: CategorySelectorProps) {
  const t = useTranslations('portal.new_message')

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor="category-selector" className="text-sm font-medium text-stone-700">
        {t('category_label')}
      </label>
      <select
        id="category-selector"
        data-testid="category-selector"
        value={value ?? ''}
        onChange={(e) => onChange(e.target.value as MessageCategory)}
        className={`block w-full rounded-md border bg-white px-3 py-2 text-sm shadow-sm focus:outline-none focus:ring-1 ${
          error
            ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
            : 'border-stone-300 focus:border-emerald-500 focus:ring-emerald-500'
        }`}
      >
        <option value="" disabled>
          {t('category_placeholder')}
        </option>
        {CATEGORY_OPTIONS.map((opt) => (
          <option key={opt.category} value={opt.category}>
            {t(`categories.${opt.label}`)}
          </option>
        ))}
      </select>
      {error && (
        <p className="text-xs text-red-600" data-testid="category-error">
          {error}
        </p>
      )}
    </div>
  )
}
