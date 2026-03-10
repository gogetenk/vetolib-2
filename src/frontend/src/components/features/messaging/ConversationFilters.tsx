'use client'

import { useTranslations } from 'next-intl'
import { Search } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import type { MessageCategory, ConversationStatus } from '@/lib/api/messaging-types'

interface ConversationFiltersProps {
  statusFilter: ConversationStatus | ''
  categoryFilter: MessageCategory | ''
  searchQuery: string
  onStatusChange: (status: ConversationStatus | '') => void
  onCategoryChange: (category: MessageCategory | '') => void
  onSearchChange: (q: string) => void
}

const ALL_STATUSES: ConversationStatus[] = ['Open', 'InProgress', 'Resolved', 'Closed']
const ALL_CATEGORIES: MessageCategory[] = [
  'MedicalUrgency',
  'PostOperativeFollowUp',
  'MedicalQuestion',
  'AppointmentRequest',
  'Administrative',
  'Feedback',
  'Other',
]

export function ConversationFilters({
  statusFilter,
  categoryFilter,
  searchQuery,
  onStatusChange,
  onCategoryChange,
  onSearchChange,
}: ConversationFiltersProps) {
  const t = useTranslations('messaging')

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center" data-testid="conversation-filters">
      {/* Search */}
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          data-testid="filter-search"
          className="pl-9"
          placeholder={t('search_placeholder')}
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>

      {/* Status filter */}
      <div className="flex flex-wrap gap-1" data-testid="filter-status-group">
        <Button
          variant={statusFilter === '' ? 'default' : 'outline'}
          size="sm"
          data-testid="filter-status-all"
          onClick={() => onStatusChange('')}
        >
          {t('status.all')}
        </Button>
        {ALL_STATUSES.map((s) => (
          <Button
            key={s}
            variant={statusFilter === s ? 'default' : 'outline'}
            size="sm"
            data-testid={`filter-status-${s.toLowerCase()}`}
            onClick={() => onStatusChange(s)}
          >
            {t(`status.${s}`)}
          </Button>
        ))}
      </div>

      {/* Category filter */}
      <div className="flex flex-wrap gap-1" data-testid="filter-category-group">
        <Button
          variant={categoryFilter === '' ? 'secondary' : 'ghost'}
          size="sm"
          data-testid="filter-category-all"
          onClick={() => onCategoryChange('')}
        >
          {t('category.all')}
        </Button>
        {ALL_CATEGORIES.map((c) => (
          <Button
            key={c}
            variant={categoryFilter === c ? 'secondary' : 'ghost'}
            size="sm"
            data-testid={`filter-category-${c.toLowerCase()}`}
            onClick={() => onCategoryChange(c)}
          >
            {t(`category.${c}`)}
          </Button>
        ))}
      </div>
    </div>
  )
}
