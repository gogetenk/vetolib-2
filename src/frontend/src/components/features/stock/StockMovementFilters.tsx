'use client'

import { Search } from 'lucide-react'
import { useTranslations } from 'next-intl'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { FullMovementType } from '@/lib/api/stock'

const MOVEMENT_TYPES: FullMovementType[] = [
  'INCOMING',
  'OUTGOING',
  'ADJUSTMENT',
  'LOSS',
  'RETURN',
]

interface StockMovementFiltersProps {
  typeFilter: string
  onTypeChange: (value: string) => void
  dateFrom: string
  onDateFromChange: (value: string) => void
  dateTo: string
  onDateToChange: (value: string) => void
  searchQuery: string
  onSearchChange: (value: string) => void
}

export function StockMovementFilters({
  typeFilter,
  onTypeChange,
  dateFrom,
  onDateFromChange,
  dateTo,
  onDateToChange,
  searchQuery,
  onSearchChange,
}: StockMovementFiltersProps) {
  const t = useTranslations('stock.history')

  return (
    <div className="flex flex-wrap gap-3" data-testid="movement-filters">
      {/* Search */}
      <div className="relative">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
        <Input
          data-testid="movement-search"
          placeholder={t('search_placeholder')}
          className="w-56 ps-9 bg-card border-border/80 rounded-xl h-10 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md transition-shadow"
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>

      {/* Type filter */}
      <Select
        value={typeFilter}
        onValueChange={(v) => onTypeChange(v ?? 'all')}
      >
        <SelectTrigger
          className="w-44 rounded-xl border-border/80 text-[13px]"
          data-testid="filter-movement-type-trigger"
        >
          <SelectValue placeholder={t('all_types')} />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all" data-testid="filter-movement-type-all">
            {t('all_types')}
          </SelectItem>
          {MOVEMENT_TYPES.map((mt) => (
            <SelectItem
              key={mt}
              value={mt}
              data-testid={`filter-movement-type-${mt.toLowerCase()}`}
            >
              {t(`types.${mt.toLowerCase()}`)}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {/* Date from */}
      <Input
        type="date"
        data-testid="filter-date-from"
        className="w-40 rounded-xl border-border/80 text-[13px] h-10 focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
        value={dateFrom}
        onChange={(e) => onDateFromChange(e.target.value)}
        placeholder={t('date_from')}
      />

      {/* Date to */}
      <Input
        type="date"
        data-testid="filter-date-to"
        className="w-40 rounded-xl border-border/80 text-[13px] h-10 focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
        value={dateTo}
        onChange={(e) => onDateToChange(e.target.value)}
        placeholder={t('date_to')}
      />
    </div>
  )
}
