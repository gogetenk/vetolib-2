'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { toast } from 'sonner'
import { Baby, Heart, CalendarClock } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { PageContainer } from '@/components/ui/page-container'
import { useRole } from '@/hooks/use-role'
import {
  getActivePregnancies,
  getLitters,
  getHeatCycles,
} from '@/lib/api/breeding'
import type {
  PregnancyDto,
  LitterDto,
  HeatCycleDto,
} from '@/lib/api/breeding'

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

function daysUntil(dateStr: string): number {
  const target = new Date(dateStr)
  const now = new Date()
  return Math.ceil((target.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))
}

export default function BreedingDashboardPage() {
  const role = useRole()
  const canView = role === 'VET' || role === 'ADMIN'

  const [activePregnancies, setActivePregnancies] = useState<PregnancyDto[]>([])
  const [recentLitters, setRecentLitters] = useState<LitterDto[]>([])
  const [recentHeatCycles, setRecentHeatCycles] = useState<HeatCycleDto[]>([])
  const [isLoading, setIsLoading] = useState(true)

  const loadData = useCallback(async () => {
    try {
      const [pregnancies, litters, heatCyclesRaw] = await Promise.all([
        getActivePregnancies(),
        getLitters(),
        getHeatCycles(),
      ])
      setActivePregnancies(pregnancies)
      // Sort litters by date, most recent first, take top 5
      const sortedLitters = [...litters].sort(
        (a, b) => new Date(b.dateOfBirth).getTime() - new Date(a.dateOfBirth).getTime()
      )
      setRecentLitters(sortedLitters.slice(0, 5))
      // Recent heat cycles (already sorted by handler), take top 5
      setRecentHeatCycles(heatCyclesRaw.slice(0, 5))
    } catch {
      toast.error('Failed to load breeding dashboard data.')
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    loadData()
  }, [loadData])

  if (!canView) {
    return (
      <PageContainer data-testid="breeding-dashboard-page">
        <div className="py-12 text-center" data-testid="breeding-access-denied">
          <p className="text-muted-foreground">You do not have access to the breeding dashboard.</p>
        </div>
      </PageContainer>
    )
  }

  if (isLoading) {
    return (
      <PageContainer data-testid="breeding-dashboard-page">
        <div className="space-y-6">
          <div className="h-8 w-64 rounded bg-muted animate-pulse" />
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="h-48 rounded-xl bg-muted animate-pulse" />
            <div className="h-48 rounded-xl bg-muted animate-pulse" />
            <div className="h-48 rounded-xl bg-muted animate-pulse" />
          </div>
        </div>
      </PageContainer>
    )
  }

  return (
    <PageContainer data-testid="breeding-dashboard-page">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-[22px] font-bold text-foreground" data-testid="breeding-dashboard-title">
            Breeding Dashboard
          </h1>
          <p className="text-[13px] text-muted-foreground mt-0.5">
            Active pregnancies, recent litters, and upcoming heat cycles
          </p>
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4" data-testid="breeding-summary-cards">
        <div className="bg-card border border-border/80 rounded-xl shadow-sm p-5" data-testid="breeding-active-pregnancies-card">
          <div className="flex items-center gap-3 mb-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-pink-50 text-pink-600">
              <Heart className="h-5 w-5" />
            </div>
            <div>
              <p className="text-[12px] text-muted-foreground uppercase tracking-wider font-medium">Active Pregnancies</p>
              <p className="text-2xl font-bold text-foreground">{activePregnancies.length}</p>
            </div>
          </div>
        </div>
        <div className="bg-card border border-border/80 rounded-xl shadow-sm p-5" data-testid="breeding-recent-litters-card">
          <div className="flex items-center gap-3 mb-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-50 text-blue-600">
              <Baby className="h-5 w-5" />
            </div>
            <div>
              <p className="text-[12px] text-muted-foreground uppercase tracking-wider font-medium">Recent Litters</p>
              <p className="text-2xl font-bold text-foreground">{recentLitters.length}</p>
            </div>
          </div>
        </div>
        <div className="bg-card border border-border/80 rounded-xl shadow-sm p-5" data-testid="breeding-heat-cycles-card">
          <div className="flex items-center gap-3 mb-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-amber-50 text-amber-600">
              <CalendarClock className="h-5 w-5" />
            </div>
            <div>
              <p className="text-[12px] text-muted-foreground uppercase tracking-wider font-medium">Recent Heat Cycles</p>
              <p className="text-2xl font-bold text-foreground">{recentHeatCycles.length}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Active Pregnancies */}
      <section data-testid="breeding-dashboard-pregnancies">
        <h2 className="text-[15px] font-bold text-foreground mb-3">Active Pregnancies</h2>
        {activePregnancies.length === 0 ? (
          <p className="text-muted-foreground text-[13px] py-6 text-center bg-card border border-border/80 rounded-xl" data-testid="breeding-dashboard-pregnancies-empty">
            No active pregnancies.
          </p>
        ) : (
          <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
              <span>Patient</span>
              <span>Mating Date</span>
              <span>Expected Due</span>
              <span>Days Left</span>
              <span>Checks</span>
            </div>
            {activePregnancies.map((preg) => {
              const days = daysUntil(preg.expectedDueDate)
              return (
                <div
                  key={preg.id}
                  data-testid={`breeding-dashboard-pregnancy-${preg.id}`}
                  className="grid grid-cols-1 md:grid-cols-5 gap-4 items-center text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
                >
                  <Link
                    href={`/patients/${preg.patientId}`}
                    className="font-semibold text-primary hover:underline"
                    data-testid={`breeding-dashboard-pregnancy-patient-${preg.id}`}
                  >
                    {preg.patientName}
                  </Link>
                  <span className="text-muted-foreground">{formatDate(preg.matingDate)}</span>
                  <span className="text-muted-foreground">{formatDate(preg.expectedDueDate)}</span>
                  <Badge
                    variant={days <= 7 ? 'destructive' : days <= 14 ? 'default' : 'secondary'}
                    className="w-fit text-[10px] font-bold"
                  >
                    {days > 0 ? `${days} days` : 'Due now'}
                  </Badge>
                  <span className="text-muted-foreground">{preg.checks.length} check{preg.checks.length !== 1 ? 's' : ''}</span>
                </div>
              )
            })}
          </div>
        )}
      </section>

      {/* Recent Litters */}
      <section data-testid="breeding-dashboard-litters">
        <h2 className="text-[15px] font-bold text-foreground mb-3">Recent Litters</h2>
        {recentLitters.length === 0 ? (
          <p className="text-muted-foreground text-[13px] py-6 text-center bg-card border border-border/80 rounded-xl" data-testid="breeding-dashboard-litters-empty">
            No litters recorded.
          </p>
        ) : (
          <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
              <span>Mother</span>
              <span>Father</span>
              <span>Date of Birth</span>
              <span>Offspring</span>
              <span>Breed</span>
            </div>
            {recentLitters.map((litter) => (
              <div
                key={litter.id}
                data-testid={`breeding-dashboard-litter-${litter.id}`}
                className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
              >
                <Link
                  href={`/patients/${litter.motherId}`}
                  className="font-semibold text-primary hover:underline"
                  data-testid={`breeding-dashboard-litter-mother-${litter.id}`}
                >
                  {litter.motherName}
                </Link>
                <span className="text-muted-foreground">{litter.fatherName || '—'}</span>
                <span className="text-muted-foreground">{formatDate(litter.dateOfBirth)}</span>
                <span className="text-foreground font-medium">{litter.offspringCount}</span>
                <span className="text-muted-foreground">{litter.breed || '—'}</span>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Recent Heat Cycles */}
      <section data-testid="breeding-dashboard-heat-cycles">
        <h2 className="text-[15px] font-bold text-foreground mb-3">Recent Heat Cycles</h2>
        {recentHeatCycles.length === 0 ? (
          <p className="text-muted-foreground text-[13px] py-6 text-center bg-card border border-border/80 rounded-xl" data-testid="breeding-dashboard-heat-cycles-empty">
            No heat cycles recorded.
          </p>
        ) : (
          <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
              <span>Start Date</span>
              <span>End Date</span>
              <span>Duration</span>
              <span>Notes</span>
            </div>
            {recentHeatCycles.map((cycle) => (
              <div
                key={cycle.id}
                data-testid={`breeding-dashboard-heat-cycle-${cycle.id}`}
                className="grid grid-cols-1 md:grid-cols-4 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
              >
                <span className="font-semibold text-foreground">{formatDate(cycle.startDate)}</span>
                <span className="text-muted-foreground">{cycle.endDate ? formatDate(cycle.endDate) : 'Ongoing'}</span>
                <span className="text-muted-foreground">{cycle.durationDays != null ? `${cycle.durationDays} days` : '-'}</span>
                <span className="text-muted-foreground">{cycle.notes ?? '-'}</span>
              </div>
            ))}
          </div>
        )}
      </section>

      <div className="flex justify-center pt-2">
        <Link href="/patients">
          <Button variant="outline" data-testid="breeding-dashboard-view-patients">
            View All Patients
          </Button>
        </Link>
      </div>
    </PageContainer>
  )
}
