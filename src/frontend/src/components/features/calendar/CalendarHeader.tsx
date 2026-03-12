'use client'

import { ChevronLeft, ChevronRight } from 'lucide-react'
import { useTranslations, useLocale } from 'next-intl'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import type { CalendarView } from './types'
import type { VetDto } from '@/lib/api/appointments'

interface CalendarHeaderProps {
  weekLabel: string
  onPrev: () => void
  onNext: () => void
  onToday: () => void
  activeView: CalendarView
  vets: VetDto[]
  selectedVetIds: string[]
  onVetFilterChange: (vetIds: string[]) => void
}

export function CalendarHeader({
  weekLabel,
  onPrev,
  onNext,
  onToday,
  activeView,
  vets,
  selectedVetIds,
  onVetFilterChange,
}: CalendarHeaderProps) {
  const t = useTranslations('calendar')
  const locale = useLocale()
  const isRtl = locale === 'ar'

  const views: { key: CalendarView; label: string; enabled: boolean }[] = [
    { key: 'day', label: t('views.day'), enabled: false },
    { key: 'week', label: t('views.week'), enabled: true },
    { key: 'month', label: t('views.month'), enabled: false },
  ]

  function handleVetToggle(vetId: string) {
    if (selectedVetIds.includes(vetId)) {
      onVetFilterChange(selectedVetIds.filter((id) => id !== vetId))
    } else {
      onVetFilterChange([...selectedVetIds, vetId])
    }
  }

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 py-3" data-testid="calendar-header">
      {/* Navigation */}
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="icon-sm"
          onClick={onPrev}
          data-testid="calendar-prev-btn"
          aria-label={t('prev')}
        >
          {isRtl ? <ChevronRight className="size-4" /> : <ChevronLeft className="size-4" />}
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={onToday}
          data-testid="calendar-today-btn"
        >
          {t('today')}
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          onClick={onNext}
          data-testid="calendar-next-btn"
          aria-label={t('next')}
        >
          {isRtl ? <ChevronLeft className="size-4" /> : <ChevronRight className="size-4" />}
        </Button>
        <span className="text-sm font-semibold ms-2">{weekLabel}</span>
      </div>

      {/* View toggle + Vet filter */}
      <div className="flex items-center gap-3">
        {/* View toggle */}
        <div className="flex rounded-lg border border-border overflow-hidden" data-testid="calendar-view-toggle">
          <TooltipProvider>
            {views.map((view) =>
              view.enabled ? (
                <button
                  key={view.key}
                  className={`px-3 py-1 text-xs font-medium transition-colors ${
                    activeView === view.key
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-background text-muted-foreground hover:bg-muted'
                  }`}
                >
                  {view.label}
                </button>
              ) : (
                <Tooltip key={view.key}>
                  <TooltipTrigger
                    className="px-3 py-1 text-xs font-medium text-muted-foreground/50 cursor-not-allowed bg-background"
                  >
                    {view.label}
                  </TooltipTrigger>
                  <TooltipContent>{t('comingSoon')}</TooltipContent>
                </Tooltip>
              )
            )}
          </TooltipProvider>
        </div>

        {/* Vet filter dropdown */}
        <div className="relative" data-testid="calendar-vet-filter">
          <details className="group">
            <summary className="flex items-center gap-1.5 rounded-lg border border-input bg-transparent px-2.5 py-1 text-sm cursor-pointer select-none hover:bg-muted">
              {t('filterByVet')}
              <ChevronRight className="size-3 text-muted-foreground transition-transform group-open:rotate-90" />
            </summary>
            <div className="absolute end-0 top-full mt-1 z-50 min-w-48 rounded-lg border border-border bg-popover p-1 shadow-md">
              <button
                className={`w-full text-start rounded-md px-2 py-1 text-sm hover:bg-muted ${
                  selectedVetIds.length === 0 ? 'font-semibold text-primary' : ''
                }`}
                onClick={() => onVetFilterChange([])}
              >
                {t('allVets')}
              </button>
              {vets.map((vet) => (
                <button
                  key={vet.id}
                  className={`w-full text-start rounded-md px-2 py-1 text-sm hover:bg-muted ${
                    selectedVetIds.includes(vet.id) ? 'font-semibold text-primary' : ''
                  }`}
                  onClick={() => handleVetToggle(vet.id)}
                >
                  {vet.name}
                </button>
              ))}
            </div>
          </details>
        </div>
      </div>
    </div>
  )
}
