'use client'

import { useEffect, useState, useCallback } from 'react'
import Link from 'next/link'
import { Receipt } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { EmptyState } from '@/components/features/onboarding/EmptyState'
import { ErrorState } from '@/components/ui/error-state'
import { Skeleton } from '@/components/ui/skeleton'
import { formatAED, formatDate } from '@/lib/utils'
import { getInvoices } from '@/lib/api/billing'
import type { InvoiceDto, InvoiceStatus, PagedResult } from '@/lib/api/billing'
import { useTranslations } from 'next-intl'

const STATUS_OPTIONS: { value: InvoiceStatus | 'ALL'; label: string }[] = [
  { value: 'ALL', label: 'All statuses' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'SENT', label: 'Sent' },
  { value: 'PAID', label: 'Paid' },
  { value: 'CANCELLED', label: 'Cancelled' },
]

function StatusBadge({ status }: { status: InvoiceStatus }) {
  const variants: Record<InvoiceStatus, string> = {
    DRAFT: 'secondary',
    SENT: 'default',
    PAID: 'outline',
    CANCELLED: 'destructive',
  }
  return (
    <Badge variant={variants[status] as 'secondary' | 'default' | 'outline' | 'destructive'} data-testid={`invoice-status-${status.toLowerCase()}`}>
      {status}
    </Badge>
  )
}

export function InvoiceTable() {
  const tEmpty = useTranslations('onboarding.empty.billing')
  const [data, setData] = useState<PagedResult<InvoiceDto> | null>(null)
  const [statusFilter, setStatusFilter] = useState<InvoiceStatus | 'ALL'>('ALL')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const loadInvoices = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const result = await getInvoices(statusFilter !== 'ALL' ? { status: statusFilter } : undefined)
      setData(result)
    } catch {
      setError('Failed to load invoices')
    } finally {
      setLoading(false)
    }
  }, [statusFilter])

  useEffect(() => {
    loadInvoices()
  }, [loadInvoices])

  const invoices = data?.items ?? []
  const grandTotal = invoices.reduce((sum, inv) => sum + inv.total, 0)

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Invoices</CardTitle>
          <Link href="/billing/new">
            <Button data-testid="new-invoice-btn">+ New Invoice</Button>
          </Link>
        </div>
        <div className="flex gap-3 mt-2">
          <Select
            value={statusFilter}
            onValueChange={(val) => setStatusFilter(val as InvoiceStatus | 'ALL')}
          >
            <SelectTrigger className="w-48" data-testid="status-filter">
              <SelectValue placeholder="All statuses" />
            </SelectTrigger>
            <SelectContent>
              {STATUS_OPTIONS.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </CardHeader>
      <CardContent>
        {loading && (
          <div data-testid="invoices-loading" className="space-y-3 py-2">
            {[1, 2, 3, 4].map((i) => (
              <Skeleton key={i} className="h-10 w-full" />
            ))}
          </div>
        )}
        {error && (
          <ErrorState
            data-testid="invoices-error"
            title="Failed to load invoices"
            description={error}
            onRetry={loadInvoices}
          />
        )}
        {!loading && !error && (
          <>
            <Table data-testid="invoice-table">
              <TableHeader>
                <TableRow>
                  <TableHead># Invoice</TableHead>
                  <TableHead>Patient</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead className="text-right">Subtotal (excl. VAT)</TableHead>
                  <TableHead className="text-right">VAT (5%)</TableHead>
                  <TableHead className="text-right">Total AED</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoices.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} className="p-0">
                      <EmptyState
                        icon={<Receipt className="h-16 w-16" />}
                        title={tEmpty('title')}
                        description={tEmpty('description')}
                        primaryCta={{ label: tEmpty('cta'), href: '/billing/new' }}
                        tip={tEmpty('tip')}
                        data-testid-prefix="billing"
                      />
                    </TableCell>
                  </TableRow>
                )}
                {invoices.map((inv) => (
                  <TableRow key={inv.id} data-testid={`invoice-row-${inv.id}`}>
                    <TableCell className="font-mono text-sm" data-testid="invoice-number">
                      {inv.invoiceNumber}
                    </TableCell>
                    <TableCell data-testid="invoice-patient">{inv.patientName}</TableCell>
                    <TableCell data-testid="invoice-date">{formatDate(inv.createdAt)}</TableCell>
                    <TableCell className="text-right" data-testid="invoice-subtotal">
                      {formatAED(inv.subtotal)}
                    </TableCell>
                    <TableCell className="text-right" data-testid="invoice-vat">
                      {formatAED(inv.vatAmount)}
                    </TableCell>
                    <TableCell className="text-right font-semibold" data-testid="invoice-total">
                      {formatAED(inv.total)}
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={inv.status} />
                    </TableCell>
                    <TableCell>
                      <Link href={`/billing/${inv.id}`}>
                        <Button variant="outline" size="sm" data-testid={`view-invoice-${inv.id}`}>
                          View
                        </Button>
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {invoices.length > 0 && (
              <div className="mt-4 flex justify-end border-t pt-3" data-testid="invoice-summary">
                <div className="text-right space-y-1">
                  <p className="text-sm text-muted-foreground">
                    {invoices.length} invoice{invoices.length !== 1 ? 's' : ''} — Total filtered:
                  </p>
                  <p className="text-lg font-bold" data-testid="invoice-grand-total">
                    {formatAED(grandTotal)}
                  </p>
                </div>
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}
