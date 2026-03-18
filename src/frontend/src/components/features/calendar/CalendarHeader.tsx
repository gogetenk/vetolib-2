'use client'

import { useEffect, useRef, useState } from 'react'
import { ChevronDown, ChevronLeft, ChevronRight, Plus } from 'lucide-react'
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
  onNewAppointment?: () => void
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
  onNewAppointment,
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
    <div className="flex flex-col gap-4 pb-4 bg-white" data-testid="calendar-header">
      {/* Top row: Personnel filter (left) */}
      <div className="flex items-center gap-4 px-2">
        <div className="flex items-center gap-2 bg-white rounded-full border border-border/80 p-1 shadow-sm">
          {/* Vet filter dropdown (like Weda personnel filter) */}
          <div className="relative" ref={filterRef} data-testid="calendar-vet-filter">
            <button
              className="flex items-center gap-2 rounded-full px-4 py-1.5 text-[13px] font-semibold text-foreground hover:bg-muted transition-all duration-200"
              onClick={() => setIsFilterOpen((prev) => !prev)}
              data-testid="calendar-vet-filter-btn"
            >
              Personnel
              <ChevronDown className={`size-4 text-muted-foreground transition-transform duration-200 ${isFilterOpen ? 'rotate-180' : ''}`} />
            </button>

            {/* Dropdown */}
            <div className={`absolute left-0 top-full mt-2 z-50 min-w-56 rounded-xl border border-border/80 bg-white p-2 shadow-lg transition-all duration-200 origin-top-left ${isFilterOpen ? 'opacity-100 scale-y-100 translate-y-0' : 'opacity-0 scale-y-95 -translate-y-2 pointer-events-none'}`}>
              <button
                className={`w-full text-start rounded-xl px-3 py-2.5 text-[13px] transition-colors hover:bg-muted ${selectedVetIds.length === 0 ? 'font-semibold text-primary bg-primary/10' : 'text-foreground'}`}
                onClick={() => onVetFilterChange([])}
              >
                {t('allVets')}
              </button>
              {vets.map((vet) => (
                <button
                  key={vet.id}
                  className={`w-full text-start rounded-xl px-3 py-2.5 text-[13px] transition-colors hover:bg-muted mt-1 ${selectedVetIds.includes(vet.id) ? 'font-semibold text-primary bg-primary/10' : 'text-foreground'}`}
                  onClick={() => handleVetToggle(vet.id)}
                >
                  {vet.name}
                </button>
              ))}
            </div>
          </div>
          <button className="px-4 py-1.5 text-[13px] font-medium text-muted-foreground hover:text-foreground transition-colors rounded-full hover:bg-muted">
            Équipe
          </button>
        </div>
      </div>

      {/* Bottom row: Today, Navigation, View Toggle, New Appointment */}
      <div className="flex items-center justify-between px-2">
        {/* Left: Aujourd'hui */}
        <div className="flex-1 flex justify-start">
          <Button variant="outline" onClick={onToday} className="rounded-full px-6 font-semibold bg-primary text-primary-foreground border-primary hover:bg-primary/90 shadow-sm h-10">
            {t('today')}
          </Button>
        </div>

        {/* Center: Navigation & Date */}
        <div className="flex-1 flex justify-center items-center gap-4">
          <Button variant="ghost" size="icon" onClick={onPrev} className="rounded-full hover:bg-muted h-8 w-8 text-muted-foreground hover:text-foreground">
            {isRtl ? <ChevronRight className="size-5" /> : <ChevronLeft className="size-5" />}
          </Button>
          <span className="text-[15px] font-semibold min-w-[200px] text-center text-foreground">{dateLabel}</span>
          <Button variant="ghost" size="icon" onClick={onNext} className="rounded-full hover:bg-muted h-8 w-8 text-muted-foreground hover:text-foreground">
            {isRtl ? <ChevronLeft className="size-5" /> : <ChevronRight className="size-5" />}
          </Button>
        </div>

        {/* Right: View toggle & New Appt */}
        <div className="flex-1 flex justify-end items-center gap-4">
          {/* View toggle */}
          <div className="flex rounded-full border border-border/80 bg-white p-1 shadow-sm h-10 items-center">
            {views.map((view) => (
              <button
                key={view.key}
                className={`px-4 py-1.5 text-[13px] font-semibold rounded-full transition-all duration-200 h-full ${
                  activeView === view.key
                    ? 'bg-primary text-primary-foreground shadow-sm'
                    : 'text-muted-foreground hover:text-foreground hover:bg-muted'
                }`}
                onClick={() => onViewChange(view.key)}
              >
                {view.label}
              </button>
            ))}
          </div>

          {/* New Appointment Button with Glow */}
          <Button 
            className="rounded-full gap-2 px-6 h-10 font-semibold bg-primary hover:bg-primary/90 text-primary-foreground shadow-[0_4px_14px_0_rgba(48,62,245,0.39)] hover:shadow-[0_6px_20px_rgba(48,62,245,0.23)] hover:-translate-y-0.5 transition-all duration-200"
            onClick={onNewAppointment}
          >
            <Plus className="size-4" />
            Nouveau rendez-vous
          </Button>
        </div>
      </div>
    </div>
  )
}
