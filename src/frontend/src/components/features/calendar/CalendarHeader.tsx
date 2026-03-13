'use client'

import { useEffect, useRef, useState } from 'react'
import { ChevronDown, ChevronLeft, ChevronRight } from 'lucide-react'
import { useTranslations, useLocale } from 'next-intl'
import { Button } from '@/components/ui/button'
import type { CalendarView } from './types'
import type { VetDto } from '@/lib/api/appointments'

interface CalendarHeaderProps {
  dateLabel: string
  onPrev: () => void
  onNext: () => void
  onToday: () => void
  activeView: CalendarView
  onViewChange: (view: CalendarView) => void
  vets: VetDto[]
  selectedVetIds: string[]
  onVetFilterChange: (vetIds: string[]) => void
}

export function CalendarHeader({
  dateLabel,
  onPrev,
  onNext,
  onToday,
  activeView,
  onViewChange,
  vets,
  selectedVetIds,
  onVetFilterChange,
}: CalendarHeaderProps) {
  const t = useTranslations('calendar')
  const locale = useLocale()
  const isRtl = locale === 'ar'

  const [isFilterOpen, setIsFilterOpen] = useState(false)
  const filterRef = useRef<HTMLDivElement>(null)

  // Close dropdown on outside click
  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (filterRef.current && !filterRef.current.contains(e.target as Node)) {
        setIsFilterOpen(false)
      }
    }
    if (isFilterOpen) {
      document.addEventListener('mousedown', handleClickOutside)
      return () => document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [isFilterOpen])

  const views: { key: CalendarView; label: string; testId: string }[] = [
    { key: 'day', label: t('views.day'), testId: 'calendar-view-day-btn' },
    { key: 'week', label: t('views.week'), testId: 'calendar-view-week-btn' },
    { key: 'month', label: t('views.month'), testId: 'calendar-view-month-btn' },
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
          className="transition-all duration-200 ease-in-out hover:scale-105 active:scale-95"
        >
          {isRtl ? <ChevronRight className="size-4" /> : <ChevronLeft className="size-4" />}
        </Button>
        <Button
          variant="outline"
          size="sm"
          onClick={onToday}
          data-testid="calendar-today-btn"
          className="transition-all duration-200 ease-in-out hover:scale-105 active:scale-95"
        >
          {t('today')}
        </Button>
        <Button
          variant="outline"
          size="icon-sm"
          onClick={onNext}
          data-testid="calendar-next-btn"
          aria-label={t('next')}
          className="transition-all duration-200 ease-in-out hover:scale-105 active:scale-95"
        >
          {isRtl ? <ChevronLeft className="size-4" /> : <ChevronRight className="size-4" />}
        </Button>
        <span className="text-sm font-semibold ms-2 transition-all duration-200 ease-in-out">{dateLabel}</span>
      </div>

      {/* View toggle + Vet filter */}
      <div className="flex items-center gap-3">
        {/* View toggle */}
        <div className="flex rounded-lg border border-border overflow-hidden" data-testid="calendar-view-toggle">
          {views.map((view) => (
            <button
              key={view.key}
              data-testid={view.testId}
              className={`px-3 py-1 text-xs font-medium transition-all duration-200 ease-in-out ${
                activeView === view.key
                  ? 'bg-primary text-primary-foreground shadow-sm'
                  : 'bg-background text-muted-foreground hover:bg-muted'
              }`}
              onClick={() => onViewChange(view.key)}
            >
              {view.label}
            </button>
          ))}
        </div>

        {/* Vet filter dropdown with smooth animation */}
        <div className="relative" ref={filterRef} data-testid="calendar-vet-filter">
          <button
            className="flex items-center gap-1.5 rounded-lg border border-input bg-transparent px-2.5 py-1 text-sm cursor-pointer select-none hover:bg-muted transition-all duration-200 ease-in-out"
            onClick={() => setIsFilterOpen((prev) => !prev)}
            data-testid="calendar-vet-filter-btn"
          >
            {t('filterByVet')}
            <ChevronDown
              className={`size-3 text-muted-foreground transition-transform duration-200 ease-in-out ${
                isFilterOpen ? 'rotate-180' : ''
              }`}
            />
          </button>

          {/* Dropdown with fade + slide */}
          <div
            className={`absolute end-0 top-full mt-1 z-50 min-w-48 rounded-lg border border-border bg-popover p-1 shadow-md transition-all duration-200 ease-in-out origin-top ${
              isFilterOpen
                ? 'opacity-100 scale-y-100 translate-y-0'
                : 'opacity-0 scale-y-95 -translate-y-1 pointer-events-none'
            }`}
          >
            <button
              className={`w-full text-start rounded-md px-2 py-1.5 text-sm transition-colors duration-150 hover:bg-muted ${
                selectedVetIds.length === 0 ? 'font-semibold text-primary' : ''
              }`}
              onClick={() => onVetFilterChange([])}
              data-testid="calendar-vet-filter-all"
            >
              {t('allVets')}
            </button>
            {vets.map((vet) => (
              <button
                key={vet.id}
                className={`w-full text-start rounded-md px-2 py-1.5 text-sm transition-colors duration-150 hover:bg-muted ${
                  selectedVetIds.includes(vet.id) ? 'font-semibold text-primary' : ''
                }`}
                onClick={() => handleVetToggle(vet.id)}
                data-testid={`calendar-vet-filter-${vet.id}`}
              >
                {vet.name}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
