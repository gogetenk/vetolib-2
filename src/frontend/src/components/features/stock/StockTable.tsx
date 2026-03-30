'use client'

import { useState, useMemo } from 'react'
import { useTranslations } from 'next-intl'
import { Pencil, ArrowLeftRight, Search } from 'lucide-react'
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
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { LtrText } from '@/components/ui/ltr-text'
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
  const [searchQuery, setSearchQuery] = useState('')

  const filtered = useMemo(() => {
    return items.filter(item => {
      if (categoryFilter !== 'all' && item.category !== categoryFilter) return false
      if (statusFilter === 'low-stock' && !item.isLowStock) return false
      if (statusFilter === 'expiring-soon' && !item.isExpiringSoon) return false
      if (searchQuery.trim() && !item.name.toLowerCase().includes(searchQuery.toLowerCase())) return false
      return true
    })
  }, [items, categoryFilter, statusFilter, searchQuery])

  function getStatusBadge(item: StockItemDto) {
    const base = 'rounded-md text-[10px] font-bold uppercase tracking-wider border'
    if (item.isLowStock) {
      return (
        <Badge className={`${base} bg-red-50 text-red-700 border-red-200`} data-testid={`badge-low-stock-${item.id}`}>
          {t('status.low_stock')}
        </Badge>
      )
    }
    if (item.isExpiringSoon) {
      return (
        <Badge
          className={`${base} bg-amber-50 text-amber-700 border-amber-200`}
          data-testid={`badge-expiring-${item.id}`}
        >
          {t('status.expiring_soon')}
        </Badge>
      )
    }
    return (
      <Badge className={`${base} bg-success/10 text-success border-success/25`} data-testid={`badge-ok-${item.id}`}>
        {t('status.ok')}
      </Badge>
    )
  }

  const categories: StockCategory[] = ['Medication', 'Vaccine', 'Supply']

  return (
    <div className="space-y-4" data-testid="stock-table-container">
      {/* Filters */}
      <div className="flex flex-wrap gap-3" data-testid="stock-filters">
        <div className="relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            data-testid="stock-search"
            placeholder={t('search_placeholder') ?? 'Search item name...'}
            aria-label={t('search_placeholder') ?? 'Search item name...'}
            className="w-56 ps-9 bg-card border-border/80 rounded-xl h-10 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md transition-shadow"
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
        <Select
          value={categoryFilter}
          onValueChange={(v) => setCategoryFilter(v ?? 'all')}
          data-testid="filter-category"
        >
          <SelectTrigger className="w-44 rounded-xl border-border/80 text-[13px]" data-testid="filter-category-trigger">
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
          <SelectTrigger className="w-44 rounded-xl border-border/80 text-[13px]" data-testid="filter-status-trigger">
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

      {/* Mobile card layout */}
      {filtered.length > 0 && (
        <div className="md:hidden space-y-3" data-testid="stock-cards">
          {filtered.map(item => (
            <div
              key={item.id}
              className="bg-card border border-border/80 rounded-xl p-4 min-h-[44px] shadow-sm transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md"
              data-testid={`stock-card-${item.id}`}
            >
              <div className="flex items-start justify-between gap-2">
                <div className="min-w-0 flex-1">
                  <p className="text-[14px] font-bold text-foreground">{item.name}</p>
                  <p className="text-[13px] text-muted-foreground">
                    {t(`categories.${item.category.toLowerCase()}`)}
                  </p>
                </div>
                {getStatusBadge(item)}
              </div>
              <div className="mt-2 flex items-center gap-4 text-[13px]">
                <span className={item.isLowStock ? 'font-semibold text-destructive' : 'text-muted-foreground'}>
                  <LtrText>{item.quantity} {item.unit}</LtrText>
                </span>
                {item.expiryDate && (
                  <span className={item.isExpiringSoon ? 'font-semibold text-orange-600' : 'text-muted-foreground'}>
                    <LtrText>{item.expiryDate}</LtrText>
                  </span>
                )}
              </div>
              <div className="mt-3 flex gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onEdit(item)}
                  data-testid={`btn-edit-card-${item.id}`}
                  aria-label={t('actions.edit')}
                  className="min-h-[44px] rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted"
                >
                  <Pencil className="h-3.5 w-3.5 me-1" />
                  {t('actions.edit')}
                </Button>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => onMovement(item)}
                  data-testid={`btn-movement-card-${item.id}`}
                  aria-label={t('actions.movement')}
                  className="min-h-[44px] rounded-xl text-[12px] font-semibold border-border/80 hover:bg-muted"
                >
                  <ArrowLeftRight className="h-3.5 w-3.5 me-1" />
                  {t('actions.movement')}
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Desktop table */}
      {filtered.length === 0 ? (
        <p
          className="py-12 text-center text-sm text-muted-foreground animate-in fade-in duration-300"
          data-testid="stock-empty"
        >
          {t('no_items')}
        </p>
      ) : (
        <div className="hidden md:block bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="stock-table">
          <Table>
            <TableHeader>
              <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-name">{t('columns.name')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-category">{t('columns.category')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-quantity">{t('columns.quantity')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-threshold">{t('columns.threshold')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-expiry">{t('columns.expiry')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-status">{t('columns.status')}</TableHead>
                <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground" data-testid="th-actions">{t('columns.actions')}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filtered.map(item => (
                <TableRow key={item.id} data-testid={`stock-row-${item.id}`} className="group hover:bg-muted/50 border-border/30 transition-colors">
                  <TableCell
                    className="py-3 px-4 text-[13px] font-semibold text-foreground"
                    data-testid={`stock-name-${item.id}`}
                  >
                    {item.name}
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px] text-muted-foreground" data-testid={`stock-category-${item.id}`}>
                    {t(`categories.${item.category.toLowerCase()}`)}
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px]" data-testid={`stock-quantity-${item.id}`}>
                    <LtrText className={item.isLowStock ? 'font-semibold text-red-700' : 'text-foreground font-semibold tabular-nums'}>
                      {item.quantity} {item.unit}
                    </LtrText>
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px] text-muted-foreground tabular-nums" data-testid={`stock-threshold-${item.id}`}>
                    <LtrText>{item.threshold} {item.unit}</LtrText>
                  </TableCell>
                  <TableCell className="py-3 px-4 text-[13px]" data-testid={`stock-expiry-${item.id}`}>
                    {item.expiryDate ? (
                      <span className={item.isExpiringSoon ? 'font-semibold text-amber-700' : 'text-muted-foreground'}>
                        {item.expiryDate}
                      </span>
                    ) : (
                      <span className="text-muted-foreground">—</span>
                    )}
                  </TableCell>
                  <TableCell className="py-3 px-4" data-testid={`stock-status-${item.id}`}>
                    {getStatusBadge(item)}
                  </TableCell>
                  <TableCell className="py-3 px-4">
                    <div className="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-200 ease-in-out">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => onEdit(item)}
                        data-testid={`btn-edit-${item.id}`}
                        aria-label={t('actions.edit')}
                        className="rounded-xl border-border/80 hover:bg-muted"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => onMovement(item)}
                        data-testid={`btn-movement-${item.id}`}
                        aria-label={t('actions.movement')}
                        className="rounded-xl border-border/80 hover:bg-muted"
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
