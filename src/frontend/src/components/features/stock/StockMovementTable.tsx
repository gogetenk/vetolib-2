'use client'

import { useTranslations } from 'next-intl'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { LtrText } from '@/components/ui/ltr-text'
import { MovementTypeBadge } from './MovementTypeBadge'
import type { StockMovementHistoryDto } from '@/lib/api/stock'

interface StockMovementTableProps {
  movements: StockMovementHistoryDto[]
}

function formatDateTime(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleDateString('en-AE', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function getQuantitySign(type: string): string {
  if (type === 'INCOMING') return '+'
  if (type === 'OUTGOING' || type === 'LOSS' || type === 'RETURN') return '-'
  return ''
}

export function StockMovementTable({ movements }: StockMovementTableProps) {
  const t = useTranslations('stock.history')

  if (movements.length === 0) {
    return (
      <p
        className="py-12 text-center text-sm text-muted-foreground animate-in fade-in duration-300"
        data-testid="movement-empty"
      >
        {t('no_movements')}
      </p>
    )
  }

  return (
    <>
      {/* Mobile card layout */}
      <div className="md:hidden space-y-3" data-testid="movement-cards">
        {movements.map((m) => (
          <div
            key={m.id}
            className="bg-white border border-border/80 rounded-xl p-4 shadow-sm transition-all duration-200 ease-in-out hover:-translate-y-0.5 hover:shadow-md"
            data-testid={`movement-card-${m.id}`}
          >
            <div className="flex items-start justify-between gap-2">
              <div className="min-w-0 flex-1">
                <p className="text-[14px] font-bold text-[#061e44]">{m.stockItemName}</p>
                <p className="text-[12px] text-muted-foreground mt-0.5">{formatDateTime(m.createdAt)}</p>
              </div>
              <MovementTypeBadge type={m.type} translatedLabel={t(`types.${m.type.toLowerCase()}`)} />
            </div>
            <div className="mt-2 text-[13px] space-y-1">
              <p>
                <span className="text-muted-foreground">{t('columns.quantity')}: </span>
                <span className={`font-semibold tabular-nums ${m.type === 'INCOMING' ? 'text-emerald-700' : m.type === 'LOSS' ? 'text-red-700' : 'text-[#061e44]'}`}>
                  {getQuantitySign(m.type)}{m.quantity}
                </span>
                <span className="text-muted-foreground"> ({m.previousQuantity} &rarr; {m.newQuantity})</span>
              </p>
              <p>
                <span className="text-muted-foreground">{t('columns.performed_by')}: </span>
                <span className="text-[#061e44]">{m.performedBy}</span>
              </p>
              {m.patientName && (
                <p>
                  <span className="text-muted-foreground">{t('columns.patient')}: </span>
                  <span className="text-[#061e44]">{m.patientName}</span>
                </p>
              )}
              {m.reason && (
                <p className="text-muted-foreground italic">{m.reason}</p>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Desktop table */}
      <div
        className="hidden md:block bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden"
        data-testid="movement-table"
      >
        <Table>
          <TableHeader>
            <TableRow className="bg-[#f4f6f9] hover:bg-[#f4f6f9] border-b border-border/50">
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-date">
                {t('columns.date')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-item">
                {t('columns.item')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-type">
                {t('columns.type')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-quantity">
                {t('columns.quantity')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-stock-change">
                {t('columns.stock_change')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-performed-by">
                {t('columns.performed_by')}
              </TableHead>
              <TableHead className="text-[11px] font-bold uppercase tracking-wider text-[#061e44]" data-testid="th-reason">
                {t('columns.reason')}
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {movements.map((m) => (
              <TableRow
                key={m.id}
                data-testid={`movement-row-${m.id}`}
                className="group hover:bg-[#f4f6f9]/50 border-border/30 transition-colors"
              >
                <TableCell className="text-[13px] text-muted-foreground whitespace-nowrap" data-testid={`movement-date-${m.id}`}>
                  {formatDateTime(m.createdAt)}
                </TableCell>
                <TableCell className="text-[13px] font-semibold text-[#061e44]" data-testid={`movement-item-${m.id}`}>
                  {m.stockItemName}
                  {m.patientName && (
                    <span className="block text-[11px] text-muted-foreground font-normal mt-0.5">
                      {m.patientName}
                    </span>
                  )}
                </TableCell>
                <TableCell data-testid={`movement-type-${m.id}`}>
                  <MovementTypeBadge type={m.type} translatedLabel={t(`types.${m.type.toLowerCase()}`)} />
                </TableCell>
                <TableCell className="text-[13px] tabular-nums" data-testid={`movement-qty-${m.id}`}>
                  <span className={`font-semibold ${m.type === 'INCOMING' ? 'text-emerald-700' : m.type === 'LOSS' ? 'text-red-700' : m.type === 'RETURN' ? 'text-purple-700' : 'text-[#061e44]'}`}>
                    {getQuantitySign(m.type)}{m.quantity}
                  </span>
                </TableCell>
                <TableCell className="text-[13px] text-muted-foreground tabular-nums" data-testid={`movement-change-${m.id}`}>
                  <LtrText>{m.previousQuantity} &rarr; {m.newQuantity}</LtrText>
                </TableCell>
                <TableCell className="text-[13px] text-[#061e44]" data-testid={`movement-by-${m.id}`}>
                  {m.performedBy}
                </TableCell>
                <TableCell className="text-[13px] text-muted-foreground max-w-[200px] truncate" data-testid={`movement-reason-${m.id}`}>
                  {m.reason ?? '—'}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </>
  )
}
