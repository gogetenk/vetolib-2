'use client'

import { useEffect, useState, useCallback } from 'react'
import { Plus, History } from 'lucide-react'
import Link from 'next/link'
import { toast } from 'sonner'
import { useTranslations } from 'next-intl'
import { Button } from '@/components/ui/button'
import { StockTable } from '@/components/features/stock/StockTable'
import { StockAlerts } from '@/components/features/stock/StockAlerts'
import { StockItemForm } from '@/components/features/stock/StockItemForm'
import { StockMovementForm } from '@/components/features/stock/StockMovementForm'
import {
  getStockItems,
  getStockAlerts,
  createStockItem,
  updateStockItem,
  createStockMovement,
} from '@/lib/api/stock'
import type { StockItemDto, StockAlertDto } from '@/lib/api/stock'

export default function StockPageClient() {
  const t = useTranslations('stock')

  const [items, setItems] = useState<StockItemDto[]>([])
  const [alerts, setAlerts] = useState<StockAlertDto[]>([])
  const [isLoading, setIsLoading] = useState(true)

  // Dialog state
  const [showAddForm, setShowAddForm] = useState(false)
  const [editItem, setEditItem] = useState<StockItemDto | null>(null)
  const [movementItem, setMovementItem] = useState<StockItemDto | null>(null)

  const fetchData = useCallback(async () => {
    setIsLoading(true)
    try {
      const [stockItems, stockAlerts] = await Promise.all([
        getStockItems(),
        getStockAlerts(),
      ])
      setItems(stockItems)
      setAlerts(stockAlerts)
    } catch {
      toast.error(t('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [t])

  useEffect(() => {
    fetchData()
  }, [fetchData])

  const handleAddSubmit = async (data: Parameters<typeof createStockItem>[0]) => {
    try {
      await createStockItem(data)
      toast.success(t('success.item_added'))
      setShowAddForm(false)
      await fetchData()
    } catch {
      toast.error(t('errors.save_failed'))
    }
  }

  const handleEditSubmit = async (data: Parameters<typeof createStockItem>[0]) => {
    if (!editItem) return
    try {
      await updateStockItem(editItem.id, data)
      toast.success(t('success.item_updated'))
      setEditItem(null)
      await fetchData()
    } catch {
      toast.error(t('errors.save_failed'))
    }
  }

  const handleMovementSubmit = async (data: {
    type: 'IN' | 'OUT' | 'ADJUSTMENT'
    quantity: number
    reason?: string
  }) => {
    if (!movementItem) return
    try {
      await createStockMovement(movementItem.id, data)
      toast.success(t('success.movement_recorded'))
      setMovementItem(null)
      await fetchData()
    } catch {
      toast.error(t('errors.movement_failed'))
    }
  }

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="stock-page">
      <div className="flex items-center justify-between">
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="stock-title">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {t('title')}
        </h1>
        <div className="flex items-center gap-2">
          <Link href="/stock/history">
            <Button
              variant="outline"
              data-testid="btn-stock-history"
              className="rounded-xl border-border/80 text-[13px] font-semibold hover:bg-[#f4f6f9] h-10 px-5"
            >
              <History className="me-1.5 h-4 w-4" />
              {t('history_link')}
            </Button>
          </Link>
          <Button
            onClick={() => setShowAddForm(true)}
            data-testid="btn-add-stock-item"
            className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
          >
            <Plus className="me-1.5 h-4 w-4" />
            {t('add_item')}
          </Button>
        </div>
      </div>

      {/* Alerts banner */}
      {!isLoading && <StockAlerts alerts={alerts} />}

      {/* Table */}
      {isLoading ? (
        <div
          className="space-y-2"
          data-testid="stock-loading"
        >
          {[1, 2, 3].map(i => (
            <div key={i} className="h-12 rounded-md bg-muted animate-pulse" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
          ))}
        </div>
      ) : (
        <StockTable
          items={items}
          onEdit={(item) => setEditItem(item)}
          onMovement={(item) => setMovementItem(item)}
        />
      )}

      {/* Add form dialog */}
      <StockItemForm
        open={showAddForm}
        onOpenChange={setShowAddForm}
        onSubmit={handleAddSubmit}
      />

      {/* Edit form dialog */}
      <StockItemForm
        open={!!editItem}
        onOpenChange={(open) => { if (!open) setEditItem(null) }}
        item={editItem ?? undefined}
        onSubmit={handleEditSubmit}
      />

      {/* Movement form dialog */}
      <StockMovementForm
        key={movementItem?.id ?? 'no-item'}
        open={!!movementItem}
        onOpenChange={(open) => { if (!open) setMovementItem(null) }}
        item={movementItem}
        onSubmit={handleMovementSubmit}
      />
    </div>
  )
}
