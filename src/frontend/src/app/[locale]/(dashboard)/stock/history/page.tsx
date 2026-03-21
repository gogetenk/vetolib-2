'use client'

import { useEffect, useState, useCallback, useMemo } from 'react'
import { ArrowLeft, Download } from 'lucide-react'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { useRouter } from 'next/navigation'
import { PageContainer } from '@/components/ui/page-container'
import { Button } from '@/components/ui/button'
import { StockMovementTable } from '@/components/features/stock/StockMovementTable'
import { StockMovementFilters } from '@/components/features/stock/StockMovementFilters'
import { getStockMovements } from '@/lib/api/stock'
import type { StockMovementHistoryDto, FullMovementType } from '@/lib/api/stock'

function exportToCsv(movements: StockMovementHistoryDto[], filename: string) {
  const headers = ['Date', 'Item', 'Type', 'Quantity', 'Previous Qty', 'New Qty', 'Performed By', 'Patient', 'Reason']
  const rows = movements.map((m) => [
    m.createdAt,
    m.stockItemName,
    m.type,
    String(m.quantity),
    String(m.previousQuantity),
    String(m.newQuantity),
    m.performedBy,
    m.patientName ?? '',
    m.reason ?? '',
  ])

  const csvContent = [
    headers.join(','),
    ...rows.map((row) => row.map((cell) => `"${cell.replace(/"/g, '""')}"`).join(',')),
  ].join('\n')

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.click()
  URL.revokeObjectURL(url)
}

export default function StockHistoryPage() {
  const t = useTranslations('stock.history')
  const tStock = useTranslations('stock')
  const router = useRouter()

  const [movements, setMovements] = useState<StockMovementHistoryDto[]>([])
  const [isLoading, setIsLoading] = useState(true)

  // Filters
  const [typeFilter, setTypeFilter] = useState('all')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [searchQuery, setSearchQuery] = useState('')

  const fetchMovements = useCallback(async () => {
    setIsLoading(true)
    try {
      const filters: Record<string, string> = {}
      if (typeFilter !== 'all') filters.type = typeFilter
      if (dateFrom) filters.dateFrom = dateFrom
      if (dateTo) filters.dateTo = dateTo
      if (searchQuery.trim()) filters.search = searchQuery.trim()

      const data = await getStockMovements(
        Object.keys(filters).length > 0
          ? (filters as { type?: FullMovementType; dateFrom?: string; dateTo?: string; search?: string })
          : undefined
      )
      setMovements(data)
    } catch {
      toast.error(tStock('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [typeFilter, dateFrom, dateTo, searchQuery, tStock])

  useEffect(() => {
    fetchMovements()
  }, [fetchMovements])

  const filteredMovements = useMemo(() => {
    // Client-side search (for instant feedback while server also filters)
    if (!searchQuery.trim()) return movements
    const q = searchQuery.toLowerCase()
    return movements.filter(
      (m) =>
        m.stockItemName.toLowerCase().includes(q) ||
        (m.reason?.toLowerCase().includes(q) ?? false) ||
        m.performedBy.toLowerCase().includes(q) ||
        (m.patientName?.toLowerCase().includes(q) ?? false)
    )
  }, [movements, searchQuery])

  const handleExportCsv = () => {
    const date = new Date().toISOString().slice(0, 10)
    exportToCsv(filteredMovements, `stock-movements-${date}.csv`)
    toast.success(t('export_success'))
  }

  return (
    <PageContainer data-testid="stock-history-page">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.push('/stock')}
            data-testid="btn-back-stock"
            className="rounded-xl border-border/80 hover:bg-muted"
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1
            className="text-[22px] font-bold text-foreground flex items-center gap-2"
            data-testid="stock-history-title"
          >
            <span className="w-1 h-5 bg-primary rounded-full"></span>
            {t('title')}
          </h1>
        </div>
        <Button
          onClick={handleExportCsv}
          data-testid="btn-export-csv"
          variant="outline"
          className="rounded-xl border-border/80 text-[13px] font-semibold hover:bg-muted"
          disabled={filteredMovements.length === 0}
        >
          <Download className="me-1.5 h-4 w-4" />
          {t('export_csv')}
        </Button>
      </div>

      {/* Filters */}
      <StockMovementFilters
        typeFilter={typeFilter}
        onTypeChange={setTypeFilter}
        dateFrom={dateFrom}
        onDateFromChange={setDateFrom}
        dateTo={dateTo}
        onDateToChange={setDateTo}
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
      />

      {/* Summary count */}
      {!isLoading && (
        <p className="text-[13px] text-muted-foreground" data-testid="movement-count">
          {t('count', { count: filteredMovements.length })}
        </p>
      )}

      {/* Table */}
      {isLoading ? (
        <div className="space-y-2" data-testid="movement-loading">
          {[1, 2, 3, 4, 5].map((i) => (
            <div
              key={i}
              className="h-12 rounded-md bg-muted animate-pulse"
              style={{ animationDelay: `${(i - 1) * 100}ms` }}
            />
          ))}
        </div>
      ) : (
        <StockMovementTable movements={filteredMovements} />
      )}
    </PageContainer>
  )
}
