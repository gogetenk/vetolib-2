'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { Pencil, ArrowLeftRight } from 'lucide-react'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { StockItemDto, StockCategory } from '@/lib/api/stock'

interface StockTableProps {
  items: StockItemDto[]
  onEdit: (item: StockItemDto) => void
  onMovement: (item: StockItemDto) => void
}

export function StockTable({ items, onEdit, onMovement }: StockTableProps) {
  const t = useTranslations('stock')
  const [categoryFilter, setCategoryFilter] = useState<string>('all')
  const [statusFilter, setStatusFilter] = useState<string>('all')

  const filtered = items.filter(item => {
    if (categoryFilter !== 'all' && item.category !== categoryFilter) return false
    if (statusFilter === 'low-stock' && !item.isLowStock) return false
    if (statusFilter === 'expiring-soon' && !item.isExpiringSoon) return false
    return true
  })

  function getStatusBadge(item: StockItemDto) {
    if (item.isLowStock) {
      return (
        <Badge variant="destructive" data-testid={`badge-low-stock-${item.id}`}>
          {t('status.low_stock')}
        </Badge>
      )
    }
    if (item.isExpiringSoon) {
      return (
        <Badge
          className="border-orange-300 bg-orange-100 text-orange-700"
          data-testid={`badge-expiring-${item.id}`}
        >
          {t('status.expiring_soon')}
        </Badge>
      )
    }
    return (
      <Badge variant="secondary" data-testid={`badge-ok-${item.id}`}>
        {t('status.ok')}
      </Badge>
    )
  }

  const categories: StockCategory[] = ['Medication', 'Vaccine', 'Supply']

  return (
    <div className="space-y-4" data-testid="stock-table-container">
      {/* Filters */}
      <div className="flex flex-wrap gap-3" data-testid="stock-filters">
        <Select
          value={categoryFilter}
          onValueChange={(v) => setCategoryFilter(v ?? 'all')}
          data-testid="filter-category"
        >
          <SelectTrigger className="w-44" data-testid="filter-category-trigger">
            <SelectValue placeholder={t('filters.all_categories')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all" data-testid="filter-category-all">
              {t('filters.all_categories')}
            </SelectItem>
            {categories.map(cat => (
              <SelectItem
                key={cat}
                value={cat}
                data-testid={`filter-category-${cat.toLowerCase()}`}
              >
                {t(`categories.${cat.toLowerCase()}`)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={statusFilter}
          onValueChange={(v) => setStatusFilter(v ?? 'all')}
          data-testid="filter-status"
        >
          <SelectTrigger className="w-44" data-testid="filter-status-trigger">
            <SelectValue placeholder={t('filters.all_statuses')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all" data-testid="filter-status-all">
              {t('filters.all_statuses')}
            </SelectItem>
            <SelectItem value="low-stock" data-testid="filter-status-low-stock">
              {t('status.low_stock')}
            </SelectItem>
            <SelectItem value="expiring-soon" data-testid="filter-status-expiring">
              {t('status.expiring_soon')}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      {filtered.length === 0 ? (
        <p
          className="py-12 text-center text-sm text-muted-foreground"
          data-testid="stock-empty"
        >
          {t('no_items')}
        </p>
      ) : (
        <div className="rounded-md border" data-testid="stock-table">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead data-testid="th-name">{t('columns.name')}</TableHead>
                <TableHead data-testid="th-category">{t('columns.category')}</TableHead>
                <TableHead data-testid="th-quantity">{t('columns.quantity')}</TableHead>
                <TableHead data-testid="th-threshold">{t('columns.threshold')}</TableHead>
                <TableHead data-testid="th-expiry">{t('columns.expiry')}</TableHead>
                <TableHead data-testid="th-status">{t('columns.status')}</TableHead>
                <TableHead data-testid="th-actions">{t('columns.actions')}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map(item => (
                <TableRow key={item.id} data-testid={`stock-row-${item.id}`}>
                  <TableCell
                    className="font-medium"
                    data-testid={`stock-name-${item.id}`}
                  >
                    {item.name}
                  </TableCell>
                  <TableCell data-testid={`stock-category-${item.id}`}>
                    {t(`categories.${item.category.toLowerCase()}`)}
                  </TableCell>
                  <TableCell data-testid={`stock-quantity-${item.id}`}>
                    <span className={item.isLowStock ? 'font-semibold text-destructive' : ''}>
                      {item.quantity} {item.unit}
                    </span>
                  </TableCell>
                  <TableCell data-testid={`stock-threshold-${item.id}`}>
                    {item.threshold} {item.unit}
                  </TableCell>
                  <TableCell data-testid={`stock-expiry-${item.id}`}>
                    {item.expiryDate ? (
                      <span className={item.isExpiringSoon ? 'font-semibold text-orange-600' : ''}>
                        {item.expiryDate}
                      </span>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                  </TableCell>
                  <TableCell data-testid={`stock-status-${item.id}`}>
                    {getStatusBadge(item)}
                  </TableCell>
                  <TableCell>
                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => onEdit(item)}
                        data-testid={`btn-edit-${item.id}`}
                        aria-label={t('actions.edit')}
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => onMovement(item)}
                        data-testid={`btn-movement-${item.id}`}
                        aria-label={t('actions.movement')}
                      >
                        <ArrowLeftRight className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  )
}
