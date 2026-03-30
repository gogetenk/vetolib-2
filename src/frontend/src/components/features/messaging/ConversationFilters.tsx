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
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          data-testid="filter-search"
          className="ps-9 bg-card border-border/80 rounded-xl h-10 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md transition-shadow"
          placeholder={t('search_placeholder')}
          aria-label={t('search_placeholder')}
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>

      <div className="flex flex-wrap gap-1" data-testid="filter-status-group">
        <Button
          variant={statusFilter === '' ? 'default' : 'outline'}
          size="sm"
          data-testid="filter-status-all"
          onClick={() => onStatusChange('')}
          className={statusFilter === '' ? 'bg-primary hover:bg-primary/90 text-primary-foreground rounded-xl text-[12px] font-semibold' : 'rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted'}
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
            className={statusFilter === s ? 'bg-primary hover:bg-primary/90 text-primary-foreground rounded-xl text-[12px] font-semibold' : 'rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted'}
          >
            {t(`status.${s}`)}
          </Button>
        ))}
      </div>

      <div className="flex flex-wrap gap-1" data-testid="filter-category-group">
        <Button
          variant={categoryFilter === '' ? 'secondary' : 'ghost'}
          size="sm"
          data-testid="filter-category-all"
          onClick={() => onCategoryChange('')}
          className="rounded-xl text-[12px] font-semibold"
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
            className="rounded-xl text-[12px] font-semibold"
          >
            {t(`category.${c}`)}
          </Button>
        ))}
      </div>
    </div>
  )
}
