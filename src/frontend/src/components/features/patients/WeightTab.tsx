'use client'

import { useEffect, useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { toast } from 'sonner'
import { Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { useRole } from '@/hooks/use-role'
import {
  getPatientWeights,
  getPatientWeightCurve,
  addPatientWeight,
} from '@/lib/api/weights'
import type { WeightEntryDto, WeightCurvePointDto } from '@/lib/api/weights'
import { WeightChart } from './WeightChart'
import { AddWeightDialog } from './AddWeightDialog'

interface WeightTabProps {
  patientId: string
}

const PAGE_SIZE = 10

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export function WeightTab({ patientId }: WeightTabProps) {
  const t = useTranslations('patients.detail.weight')
  const role = useRole()
  const canWrite = role === 'VET' || role === 'ADMIN'

  const [weights, setWeights] = useState<WeightEntryDto[]>([])
  const [curveData, setCurveData] = useState<WeightCurvePointDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)

  const loadData = useCallback(async () => {
    try {
      const [weightsData, curve] = await Promise.all([
        getPatientWeights(patientId),
        getPatientWeightCurve(patientId),
      ])
      setWeights(weightsData)
      setCurveData(curve)
    } catch {
      toast.error(t('load_error'))
    } finally {
      setIsLoading(false)
    }
  }, [patientId, t])

  useEffect(() => {
    loadData()
  }, [loadData])

  const handleWeightAdded = async (data: {
    weightKg: number
    recordedAt: string
    note?: string | null
  }) => {
    try {
      await addPatientWeight(patientId, data)
      toast.success(t('add_success'))
      setIsDialogOpen(false)
      setCurrentPage(1)
      setIsLoading(true)
      await loadData()
    } catch {
      toast.error(t('add_error'))
    }
  }

  const currentWeight = weights.length > 0 ? weights[0].weightKg : null

  // Pagination
  const totalPages = Math.ceil(weights.length / PAGE_SIZE)
  const paginatedWeights = weights.slice(
    (currentPage - 1) * PAGE_SIZE,
    currentPage * PAGE_SIZE
  )
  const showPagination = weights.length > PAGE_SIZE

  if (isLoading) {
    return (
      <div data-testid="weight-tab" className="space-y-6">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-64 rounded-lg bg-muted animate-pulse" />
        <div className="h-40 rounded-lg bg-muted animate-pulse" />
      </div>
    )
  }

  return (
    <div data-testid="weight-tab" className="space-y-6">
      {/* Header: current weight + add button */}
      <div className="flex items-center justify-between">
        <div>
          <p className="text-[12px] text-muted-foreground font-medium uppercase tracking-wider">
            {t('current_weight')}
          </p>
          <p
            className="text-3xl font-bold text-foreground"
            data-testid="weight-current-value"
          >
            {currentWeight != null ? `${currentWeight} kg` : t('no_data')}
          </p>
        </div>
        {canWrite && (
          <Button
            onClick={() => setIsDialogOpen(true)}
            data-testid="weight-add-button"
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
          >
            <Plus className="h-4 w-4 mr-1.5" />
            {t('add_weight')}
          </Button>
        )}
      </div>

      {/* Weight chart */}
      {curveData.length > 1 && (
        <div
          className="bg-card border border-border/80 rounded-xl shadow-sm p-4"
          data-testid="weight-chart"
        >
          <h3 className="text-[13px] font-bold text-foreground mb-3">
            {t('chart_title')}
          </h3>
          <WeightChart data={curveData} />
        </div>
      )}

      {/* Weight history table */}
      {weights.length === 0 ? (
        <p className="text-muted-foreground text-[13px] py-8 text-center">
          {t('no_data')}
        </p>
      ) : (
        <div
          className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden"
          data-testid="weight-history-table"
        >
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 text-[11px] font-bold text-foreground uppercase tracking-wider px-4 py-3 bg-muted border-b border-border/50">
            <span>{t('columns.date')}</span>
            <span>{t('columns.weight')}</span>
            <span>{t('columns.note')}</span>
            <span>{t('columns.recorded_by')}</span>
          </div>
          {paginatedWeights.map((entry) => (
            <div
              key={entry.id}
              data-testid={`weight-entry-${entry.id}`}
              className="grid grid-cols-1 md:grid-cols-4 gap-4 text-[13px] px-4 py-3 border-b border-border/30 last:border-b-0 hover:bg-muted/50 transition-colors"
            >
              <span className="font-semibold text-foreground">
                {formatDate(entry.recordedAt)}
              </span>
              <span className="text-foreground font-medium">
                {entry.weightKg} kg
              </span>
              <span className="text-muted-foreground truncate">
                {entry.note || '—'}
              </span>
              <span className="text-muted-foreground">{entry.recordedBy}</span>
            </div>
          ))}

          {/* Pagination controls */}
          {showPagination && (
            <div className="flex items-center justify-between px-4 py-3 border-t border-border/50 bg-muted/30">
              <p className="text-[12px] text-muted-foreground">
                {t('page_info', {
                  current: currentPage,
                  total: totalPages,
                })}
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === 1}
                  onClick={() => setCurrentPage((p) => p - 1)}
                  data-testid="weight-page-prev"
                >
                  {t('previous')}
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={currentPage === totalPages}
                  onClick={() => setCurrentPage((p) => p + 1)}
                  data-testid="weight-page-next"
                >
                  {t('next')}
                </Button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Add weight dialog */}
      <AddWeightDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        onSubmit={handleWeightAdded}
      />
    </div>
  )
}
