'use client'

import { useTranslations } from 'next-intl'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
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
      <label className="text-sm font-medium text-foreground">
        {t('category_label')}
      </label>
      <div data-testid="category-selector">
        <Select
          value={value ?? undefined}
          onValueChange={(v) => onChange(v as MessageCategory)}
        >
          <SelectTrigger className={`w-full rounded-md text-sm shadow-sm ${
            error
              ? 'border-red-500 focus:border-red-500 focus:ring-red-500'
              : 'border-border/80'
          }`}>
            <SelectValue>{value ? t(`categories.${value}`) : t('category_placeholder')}</SelectValue>
          </SelectTrigger>
          <SelectContent>
            {CATEGORY_OPTIONS.map((opt) => (
              <SelectItem key={opt.category} value={opt.category}>
                {t(`categories.${opt.label}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      {error && (
        <p className="text-xs text-red-600" data-testid="category-error">
          {error}
        </p>
      )}
    </div>
  )
}
