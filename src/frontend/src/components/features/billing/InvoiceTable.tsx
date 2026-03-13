'use client'

import { useEffect, useState, useCallback, useMemo } from 'react'
import Link from 'next/link'
import { Receipt, Search } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
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
import { LtrText } from '@/components/ui/ltr-text'
import { cn } from '@/lib/utils'
import { formatAED, formatDate } from '@/lib/utils'
import { getInvoices } from '@/lib/api/billing'
import type { InvoiceDto, InvoiceStatus, PagedResult } from '@/lib/api/billing'
import { useTranslations, useLocale } from 'next-intl'

const STATUS_OPTIONS: { value: InvoiceStatus | 'ALL'; label: string }[] = [
  { value: 'ALL', label: 'All statuses' },
  { value: 'DRAFT', label: 'Draft' },
  { value: 'SENT', label: 'Sent' },
  { value: 'PAID', label: 'Paid' },
  { value: 'CANCELLED', label: 'Cancelled' },
]

const INVOICE_STATUS_STYLES: Record<InvoiceStatus, { variant: 'default' | 'secondary' | 'destructive' | 'outline'; className?: string }> = {
  DRAFT: { variant: 'outline', className: 'text-gray-600 bg-gray-50' },
  SENT: { variant: 'outline', className: 'border-blue-300 text-blue-700 bg-blue-50' },
  PAID: { variant: 'default', className: 'border-green-300 text-green-700 bg-green-50' },
  CANCELLED: { variant: 'destructive' },
}

function StatusBadge({ status }: { status: InvoiceStatus }) {
  const style = INVOICE_STATUS_STYLES[status] ?? { variant: 'secondary' as const }
  return (
    <Badge
      variant={style.variant}
      className={cn('transition-colors duration-200 ease-in-out', style.className)}
      data-testid={`invoice-status-${status.toLowerCase()}`}
    >
      {status}
    </Badge>
  )
}

export function InvoiceTable() {
  const tEmpty = useTranslations('onboarding.empty.billing')
  const locale = useLocale()
  const [data, setData] = useState<PagedResult<InvoiceDto> | null>(null)
  const [statusFilter, setStatusFilter] = useState<InvoiceStatus | 'ALL'>('ALL')
  const [searchQuery, setSearchQuery] = useState('')
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

  const allInvoices = data?.items ?? []
  const invoices = useMemo(() => {
    if (!searchQuery.trim()) return allInvoices
    const q = searchQuery.toLowerCase()
    return allInvoices.filter(
      (inv) =>
        inv.invoiceNumber.toLowerCase().includes(q) ||
        inv.patientName.toLowerCase().includes(q)
    )
  }, [allInvoices, searchQuery])
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
        <div className="flex flex-wrap gap-3 mt-2">
          <div className="relative">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              data-testid="invoice-search"
              placeholder="Search invoice # or patient..."
              className="w-64 pl-9 transition-shadow duration-200 ease-in-out focus:ring-2 focus:ring-primary/20 focus:shadow-md"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
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
              <Skeleton key={i} className="h-10 w-full" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
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
            {/* Mobile card layout */}
            {invoices.length > 0 && (
              <div className="md:hidden space-y-3" data-testid="invoice-cards">
                {invoices.map((inv) => (
                   <Link
                    key={inv.id}
                    href={`/${locale}/billing/${inv.id}`}
                    data-testid={`invoice-card-${inv.id}`}
                    className="block rounded-lg border bg-card p-4 hover:shadow-md hover:-translate-y-0.5 transition-all duration-200 ease-in-out min-h-[44px]"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0 flex-1">
                        <p className="font-mono text-sm font-medium"><LtrText>{inv.invoiceNumber}</LtrText></p>
                        <p className="text-sm text-muted-foreground mt-0.5">{inv.patientName}</p>
                      </div>
                      <StatusBadge status={inv.status} />
                    </div>
                    <div className="mt-2 flex items-center justify-between text-sm">
                      <span className="text-muted-foreground"><LtrText>{formatDate(inv.createdAt)}</LtrText></span>
                      <span className="font-semibold"><LtrText>{formatAED(inv.total)}</LtrText></span>
                    </div>
                  </Link>
                ))}
              </div>
            )}

            {/* Desktop table */}
            <Table className="hidden md:table" data-testid="invoice-table">
              <TableHeader className="sticky top-0 z-10 bg-background shadow-[0_1px_3px_0_rgba(0,0,0,0.05)]">
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
                  <TableRow key={inv.id} data-testid={`invoice-row-${inv.id}`} className="group transition-colors duration-150 ease-in-out hover:bg-muted/50">
                    <TableCell className="font-mono text-sm" data-testid="invoice-number">
                      <LtrText>{inv.invoiceNumber}</LtrText>
                    </TableCell>
                    <TableCell data-testid="invoice-patient">{inv.patientName}</TableCell>
                    <TableCell data-testid="invoice-date"><LtrText>{formatDate(inv.createdAt)}</LtrText></TableCell>
                    <TableCell className="text-right" data-testid="invoice-subtotal">
                      <LtrText>{formatAED(inv.subtotal)}</LtrText>
                    </TableCell>
                    <TableCell className="text-right" data-testid="invoice-vat">
                      <LtrText>{formatAED(inv.vatAmount)}</LtrText>
                    </TableCell>
                    <TableCell className="text-right font-semibold" data-testid="invoice-total">
                      <LtrText>{formatAED(inv.total)}</LtrText>
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={inv.status} />
                    </TableCell>
                    <TableCell>
                      <Link href={`/${locale}/billing/${inv.id}`} className="opacity-0 group-hover:opacity-100 transition-opacity duration-200 ease-in-out">
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
                <div className="text-right space-y-1" dir="ltr">
                  <p className="text-sm text-muted-foreground">
                    {invoices.length} invoice{invoices.length !== 1 ? 's' : ''} — Total filtered:
                  </p>
                  <p className="text-lg font-bold" data-testid="invoice-grand-total">
                    <LtrText>{formatAED(grandTotal)}</LtrText>
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
