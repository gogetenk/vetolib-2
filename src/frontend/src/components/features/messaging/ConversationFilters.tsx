'use client'

import { useTranslations } from 'next-intl'
import { Search, SlidersHorizontal } from 'lucide-react'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import type { MessageCategory, ConversationStatus } from '@/lib/api/messaging-types'

interface ConversationFiltersProps {
  statusFilter: ConversationStatus | ''
  categoryFilter: MessageCategory | ''
  searchQuery: string
  onStatusChange: (status: ConversationStatus | '') => void
  onCategoryChange: (category: MessageCategory | '') => void
  onSearchChange: (q: string) => void
}

/** Only these statuses are shown as top-level buttons */
const VISIBLE_STATUSES: Array<{ value: ConversationStatus | '', key: string }> = [
  { value: '', key: 'all' },
  { value: 'Open', key: 'Open' },
  { value: 'InProgress', key: 'InProgress' },
]

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

  const activeCategoryLabel = categoryFilter ? t(`category.${categoryFilter}`) : null

  return (
    <div className="flex flex-col gap-3 sm:flex-row sm:items-center" data-testid="conversation-filters">
      <div className="relative flex-1">
        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" aria-hidden="true" />
        <Input
          data-testid="filter-search"
          className="pl-9 bg-white border-border/80 rounded-xl h-10 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md transition-shadow"
          placeholder={t('search_placeholder')}
          aria-label={t('search_placeholder')}
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>

      {/* Status filters — 3 visible buttons: All, Open, In Progress */}
      <div className="flex flex-wrap gap-1" data-testid="filter-status-group">
        {VISIBLE_STATUSES.map(({ value, key }) => {
          const isActive = statusFilter === value
          return (
            <Button
              key={key}
              variant={isActive ? 'default' : 'outline'}
              size="sm"
              data-testid={`filter-status-${key.toLowerCase()}`}
              onClick={() => onStatusChange(value)}
              className={isActive ? 'bg-primary hover:bg-primary/90 text-primary-foreground rounded-xl text-[12px] font-semibold' : 'rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted'}
            >
              {value === '' ? t('status.all') : t(`status.${key}`)}
            </Button>
          )
        })}
      </div>

      {/* Category filters — hidden behind "More filters" dropdown */}
      <DropdownMenu>
        <DropdownMenuTrigger
          data-testid="filter-more-trigger"
          className="inline-flex items-center justify-center gap-1.5 rounded-xl border border-border/80 bg-background px-3 py-1.5 text-[12px] font-semibold text-foreground shadow-sm hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary/30 transition-colors"
        >
          <SlidersHorizontal className="h-3.5 w-3.5" />
          {activeCategoryLabel ?? t('more_filters')}
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-52" data-testid="filter-more-dropdown">
          <DropdownMenuItem
            data-testid="filter-category-all"
            onClick={() => onCategoryChange('')}
            className="text-[12px] font-semibold"
          >
            {t('category.all')}
          </DropdownMenuItem>
          {ALL_CATEGORIES.map((c) => (
            <DropdownMenuItem
              key={c}
              data-testid={`filter-category-${c.toLowerCase()}`}
              onClick={() => onCategoryChange(c)}
              className="text-[12px] font-semibold"
            >
              {t(`category.${c}`)}
            </DropdownMenuItem>
          ))}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  )
}
