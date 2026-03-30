'use client'

import { useEffect, useState, useCallback } from 'react'
import { toast } from 'sonner'
import { Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { useRole } from '@/hooks/use-role'
import {
  getLitters,
  getPregnanciesByPatient,
  getHeatCycles,
  getHeatCyclePrediction,
  getPedigree,
  createLitter,
  createPregnancy,
  createHeatCycle,
} from '@/lib/api/breeding'
import type {
  LitterDto,
  PregnancyDto,
  HeatCycleDto,
  HeatCyclePredictionDto,
  PedigreeNodeDto,
  CreateLitterRequest,
  CreatePregnancyRequest,
  CreateHeatCycleRequest,
} from '@/lib/api/breeding'
import { PedigreeTree } from '@/components/features/breeding/PedigreeTree'
import { RegisterLitterDialog } from '@/components/features/breeding/RegisterLitterDialog'
import { RecordPregnancyDialog } from '@/components/features/breeding/RecordPregnancyDialog'
import { RecordHeatCycleDialog } from '@/components/features/breeding/RecordHeatCycleDialog'

interface BreedingTabProps {
  patientId: string
  patientSex: string
  patientSpecies: string
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

function pregnancyStatusVariant(status: string): 'default' | 'secondary' | 'destructive' {
  if (status === 'Active') return 'default'
  if (status === 'Lost') return 'destructive'
  return 'secondary'
}

export function BreedingTab({ patientId, patientSex, patientSpecies }: BreedingTabProps) {
  const role = useRole()
  const canWrite = role === 'VET' || role === 'ADMIN'
  const isFemale = patientSex === 'Female' || patientSex === 'Intact Female'

  const [litters, setLitters] = useState<LitterDto[]>([])
  const [pregnancies, setPregnancies] = useState<PregnancyDto[]>([])
  const [heatCycles, setHeatCycles] = useState<HeatCycleDto[]>([])
  const [prediction, setPrediction] = useState<HeatCyclePredictionDto | null>(null)
  const [pedigree, setPedigree] = useState<PedigreeNodeDto | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [litterDialogOpen, setLitterDialogOpen] = useState(false)
  const [pregnancyDialogOpen, setPregnancyDialogOpen] = useState(false)
  const [heatCycleDialogOpen, setHeatCycleDialogOpen] = useState(false)

  const loadData = useCallback(async () => {
    try {
      const promises: Promise<unknown>[] = [
        getLitters(patientId).then(setLitters),
        getPedigree(patientId).then(setPedigree),
      ]
      if (isFemale) {
        promises.push(
          getPregnanciesByPatient(patientId).then(setPregnancies),
          getHeatCycles(patientId).then(setHeatCycles),
          getHeatCyclePrediction(patientId).then(setPrediction).catch(() => setPrediction(null)),
        )
      }
      await Promise.all(promises)
    } catch {
      toast.error('Failed to load breeding data.')
    } finally {
      setIsLoading(false)
    }
  }, [patientId, isFemale])

  useEffect(() => {
    loadData()
  }, [loadData])

  const handleCreateLitter = async (data: CreateLitterRequest) => {
    try {
      await createLitter(data)
      toast.success('Litter registered successfully.')
      setLitterDialogOpen(false)
      setIsLoading(true)
      await loadData()
    } catch {
      toast.error('Failed to register litter.')
    }
  }

  const handleCreatePregnancy = async (data: CreatePregnancyRequest) => {
    try {
      await createPregnancy(data)
      toast.success('Pregnancy recorded successfully.')
      setPregnancyDialogOpen(false)
      setIsLoading(true)
      await loadData()
    } catch {
      toast.error('Failed to record pregnancy.')
    }
  }

  const handleCreateHeatCycle = async (data: CreateHeatCycleRequest) => {
    try {
      await createHeatCycle(data)
      toast.success('Heat cycle recorded successfully.')
      setHeatCycleDialogOpen(false)
      setIsLoading(true)
      await loadData()
    } catch {
      toast.error('Failed to record heat cycle.')
    }
  }

  if (isLoading) {
    return (
      <div data-testid="breeding-tab" className="space-y-6">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-64 rounded-lg bg-muted animate-pulse" />
      </div>
    )
  }

  return (
    <div data-testid="breeding-tab" className="space-y-8">
      {/* Litters Section */}
      <section data-testid="breeding-litters-section">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-[15px] font-bold text-foreground">
            Litters {isFemale ? '(as Mother)' : '(as Father)'}
          </h3>
          {canWrite && isFemale && (
            <Button
              size="sm"
              onClick={() => setLitterDialogOpen(true)}
              data-testid="breeding-register-litter-btn"
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
            >
              <Plus className="h-4 w-4 mr-1.5" />
              Register Litter
            </Button>
          )}
        </div>
        {litters.length === 0 ? (
          <p className="text-muted-foreground text-[13px] py-6 text-center" data-testid="breeding-litters-empty">
            No litters recorded.
          </p>
        ) : (
          <div className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="breeding-litters-list">
            <div className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
              <span>Date of Birth</span>
              <span>{isFemale ? 'Father' : 'Mother'}</span>
              <span>Breed</span>
              <span>Offspring</span>
              <span>Notes</span>
            </div>
            {litters.map((litter) => (
              <div
                key={litter.id}
                data-testid={`breeding-litter-${litter.id}`}
                className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
              >
                <span className="font-semibold text-foreground">{formatDate(litter.dateOfBirth)}</span>
                <span className="text-muted-foreground">{(isFemale ? litter.fatherName : litter.motherName) || '—'}</span>
                <span className="text-muted-foreground">{litter.breed || '—'}</span>
                <span className="text-foreground font-medium">{litter.offspringCount}</span>
                <span className="text-muted-foreground truncate">{litter.notes || '—'}</span>
              </div>
            ))}
          </div>
        )}
      </section>

      {/* Pregnancies Section (Female only) */}
      {isFemale && (
        <section data-testid="breeding-pregnancies-section">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-[15px] font-bold text-foreground">Pregnancies</h3>
            {canWrite && (
              <Button
                size="sm"
                onClick={() => setPregnancyDialogOpen(true)}
                data-testid="breeding-record-pregnancy-btn"
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                <Plus className="h-4 w-4 mr-1.5" />
                Record Pregnancy
              </Button>
            )}
          </div>
          {pregnancies.length === 0 ? (
            <p className="text-muted-foreground text-[13px] py-6 text-center" data-testid="breeding-pregnancies-empty">
              No pregnancies recorded.
            </p>
          ) : (
            <div className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="breeding-pregnancies-list">
              <div className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
                <span>Mating Date</span>
                <span>Expected Due</span>
                <span>Delivery</span>
                <span>Status</span>
                <span>Checks</span>
              </div>
              {pregnancies.map((preg) => (
                <div
                  key={preg.id}
                  data-testid={`breeding-pregnancy-${preg.id}`}
                  className="grid grid-cols-1 md:grid-cols-5 gap-4 items-center text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
                >
                  <span className="font-semibold text-foreground">{formatDate(preg.matingDate)}</span>
                  <span className="text-muted-foreground">{formatDate(preg.expectedDueDate)}</span>
                  <span className="text-muted-foreground">{preg.actualDeliveryDate ? formatDate(preg.actualDeliveryDate) : '—'}</span>
                  <Badge
                    variant={pregnancyStatusVariant(preg.status)}
                    data-testid={`breeding-pregnancy-status-${preg.id}`}
                    className="w-fit rounded-md text-[10px] font-bold uppercase tracking-wider"
                  >
                    {preg.status}
                  </Badge>
                  <span className="text-muted-foreground">{preg.checks.length} check{preg.checks.length !== 1 ? 's' : ''}</span>
                </div>
              ))}
            </div>
          )}
        </section>
      )}

      {/* Heat Cycles Section (Female only) */}
      {isFemale && (
        <section data-testid="breeding-heat-cycles-section">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-[15px] font-bold text-foreground">Heat Cycles</h3>
            {canWrite && (
              <Button
                size="sm"
                onClick={() => setHeatCycleDialogOpen(true)}
                data-testid="breeding-record-heat-cycle-btn"
                className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
              >
                <Plus className="h-4 w-4 mr-1.5" />
                Record Heat Cycle
              </Button>
            )}
          </div>

          {/* Prediction card */}
          {prediction && (
            <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-4" data-testid="breeding-heat-prediction">
              <p className="text-[13px] font-semibold text-amber-900">Next Heat Cycle Prediction</p>
              <p className="text-[13px] text-amber-800 mt-1">
                Predicted start: <span className="font-bold">{formatDate(prediction.predictedNextStartDate)}</span>
                {' '}&bull; Average cycle: {prediction.averageCycleDays} days
                {' '}&bull; Confidence: <Badge variant="secondary" className="ml-1 text-[10px]">{prediction.confidence}</Badge>
              </p>
            </div>
          )}

          {heatCycles.length === 0 ? (
            <p className="text-muted-foreground text-[13px] py-6 text-center" data-testid="breeding-heat-cycles-empty">
              No heat cycles recorded.
            </p>
          ) : (
            <div className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="breeding-heat-cycles-list">
              <div className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
                <span>Start Date</span>
                <span>End Date</span>
                <span>Phase</span>
                <span>Intensity</span>
                <span>Recorded By</span>
              </div>
              {heatCycles.map((cycle) => (
                <div
                  key={cycle.id}
                  data-testid={`breeding-heat-cycle-${cycle.id}`}
                  className="grid grid-cols-1 md:grid-cols-5 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
                >
                  <span className="font-semibold text-foreground">{formatDate(cycle.startDate)}</span>
                  <span className="text-muted-foreground">{cycle.endDate ? formatDate(cycle.endDate) : 'Ongoing'}</span>
                  <Badge variant="secondary" className="w-fit text-[10px]">{cycle.phase}</Badge>
                  <span className="text-muted-foreground">{cycle.intensity}</span>
                  <span className="text-muted-foreground">{cycle.recordedBy}</span>
                </div>
              ))}
            </div>
          )}
        </section>
      )}

      {/* Pedigree Section */}
      <section data-testid="breeding-pedigree-section">
        <h3 className="text-[15px] font-bold text-foreground mb-4">Pedigree</h3>
        {pedigree ? (
          <PedigreeTree node={pedigree} />
        ) : (
          <p className="text-muted-foreground text-[13px] py-6 text-center" data-testid="breeding-pedigree-empty">
            No pedigree data available.
          </p>
        )}
      </section>

      {/* Dialogs */}
      <RegisterLitterDialog
        open={litterDialogOpen}
        onOpenChange={setLitterDialogOpen}
        motherId={patientId}
        species={patientSpecies}
        onSubmit={handleCreateLitter}
      />
      {isFemale && (
        <>
          <RecordPregnancyDialog
            open={pregnancyDialogOpen}
            onOpenChange={setPregnancyDialogOpen}
            patientId={patientId}
            onSubmit={handleCreatePregnancy}
          />
          <RecordHeatCycleDialog
            open={heatCycleDialogOpen}
            onOpenChange={setHeatCycleDialogOpen}
            patientId={patientId}
            onSubmit={handleCreateHeatCycle}
          />
        </>
      )}
    </div>
  )
}
