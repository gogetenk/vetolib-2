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
              className="flex items-center gap-2 rounded-full px-4 py-1.5 text-[13px] font-semibold text-[#061e44] hover:bg-[#f4f6f9] transition-all duration-200"
              onClick={() => setIsFilterOpen((prev) => !prev)}
              data-testid="calendar-vet-filter-btn"
            >
              {t('personal')}
              <ChevronDown className={`size-4 text-muted-foreground transition-transform duration-200 ${isFilterOpen ? 'rotate-180' : ''}`} />
            </button>

            {/* Dropdown */}
            <div className={`absolute left-0 top-full mt-2 z-50 min-w-56 rounded-xl border border-border/80 bg-white p-2 shadow-lg transition-all duration-200 origin-top-left ${isFilterOpen ? 'opacity-100 scale-y-100 translate-y-0' : 'opacity-0 scale-y-95 -translate-y-2 pointer-events-none'}`}>
              <button
                className={`w-full text-start rounded-xl px-3 py-2.5 text-[13px] transition-colors hover:bg-[#f4f6f9] ${selectedVetIds.length === 0 ? 'font-semibold text-[#303ef5] bg-[#eef2fd]' : 'text-[#061e44]'}`}
                onClick={() => onVetFilterChange([])}
              >
                {t('allVets')}
              </button>
              {vets.map((vet) => (
                <button
                  key={vet.id}
                  className={`w-full text-start rounded-xl px-3 py-2.5 text-[13px] transition-colors hover:bg-[#f4f6f9] mt-1 ${selectedVetIds.includes(vet.id) ? 'font-semibold text-[#303ef5] bg-[#eef2fd]' : 'text-[#061e44]'}`}
                  onClick={() => handleVetToggle(vet.id)}
                >
                  {vet.name}
                </button>
              ))}
            </div>
          </div>
          <button className="px-4 py-1.5 text-[13px] font-medium text-muted-foreground hover:text-[#061e44] transition-colors rounded-full hover:bg-[#f4f6f9]">
            {t('team')}
          </button>
        </div>
      </div>

      {/* Bottom row: Today, Navigation, View Toggle, New Appointment */}
      <div className="flex items-center justify-between px-2">
        {/* Left: Aujourd'hui */}
        <div className="flex-1 flex justify-start">
          <Button variant="outline" onClick={onToday} className="rounded-full px-6 font-semibold bg-[#303ef5] text-white border-[#303ef5] hover:bg-[#2530c4] shadow-sm h-10">
            {t('today')}
          </Button>
        </div>

        {/* Center: Navigation & Date */}
        <div className="flex-1 flex justify-center items-center gap-4">
          <Button variant="ghost" size="icon" onClick={onPrev} aria-label="Previous" className="rounded-full hover:bg-[#f4f6f9] h-8 w-8 text-muted-foreground hover:text-[#061e44]">
            {isRtl ? <ChevronRight className="size-5" /> : <ChevronLeft className="size-5" />}
          </Button>
          <span className="text-[15px] font-semibold min-w-[200px] text-center text-[#061e44]">{dateLabel}</span>
          <Button variant="ghost" size="icon" onClick={onNext} aria-label="Next" className="rounded-full hover:bg-[#f4f6f9] h-8 w-8 text-muted-foreground hover:text-[#061e44]">
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
                    ? 'bg-[#303ef5] text-white shadow-sm'
                    : 'text-muted-foreground hover:text-[#061e44] hover:bg-[#f4f6f9]'
                }`}
                onClick={() => onViewChange(view.key)}
              >
                {view.label}
              </button>
            ))}
          </div>

          {/* New Appointment Button with Glow */}
          <Button 
            className="rounded-full gap-2 px-6 h-10 font-semibold bg-[#303ef5] hover:bg-[#2530c4] text-white shadow-[0_4px_14px_0_rgba(48,62,245,0.39)] hover:shadow-[0_6px_20px_rgba(48,62,245,0.23)] hover:-translate-y-0.5 transition-all duration-200"
            onClick={onNewAppointment}
          >
            <Plus className="size-4" />
            {t('newAppointment')}
          </Button>
        </div>
      </div>
    </div>
  )
}
