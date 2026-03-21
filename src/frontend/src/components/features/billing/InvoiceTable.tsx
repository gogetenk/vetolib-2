'use client'

import { useEffect, useState, useCallback, useMemo } from 'react'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Receipt, Search } from 'lucide-react'
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

const INVOICE_STATUS_STYLES: Record<InvoiceStatus, string> = {
  DRAFT: 'bg-amber-50 text-amber-700',
  SENT: 'bg-primary/10 text-primary',
  PAID: 'bg-emerald-50 text-emerald-700',
  CANCELLED: 'bg-muted text-muted-foreground',
}

function StatusBadge({ status }: { status: InvoiceStatus }) {
  const style = INVOICE_STATUS_STYLES[status] ?? 'bg-muted text-muted-foreground'
  return (
    <span
      className={cn('inline-flex items-center rounded-md px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider', style)}
      data-testid={`invoice-status-${status.toLowerCase()}`}
    >
      {status}
    </span>
  )
}

export function InvoiceTable() {
  const tEmpty = useTranslations('onboarding.empty.billing')
  const locale = useLocale()
  const router = useRouter()
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
    <Card className="bg-white border-border/80 rounded-xl shadow-sm">
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-[15px] font-bold text-foreground">Invoices</CardTitle>
          <Link href={`/${locale}/billing/new`}>
            <Button className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-10 px-5 shadow-sm" data-testid="new-invoice-btn">+ New Invoice</Button>
          </Link>
        </div>
        <div className="flex flex-wrap gap-3 mt-3">
          <div className="relative">
            <Search className="absolute start-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              data-testid="invoice-search"
              placeholder="Search invoice # or patient..."
              aria-label="Search invoices"
              className="w-64 ps-9 bg-white border-border/80 rounded-xl h-10 transition-shadow duration-200 ease-in-out focus:ring-2 focus:ring-primary/20 focus:border-primary/50 focus:shadow-md"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>
          <Select
            value={statusFilter}
            onValueChange={(val) => setStatusFilter(val as InvoiceStatus | 'ALL')}
          >
            <SelectTrigger className="w-48 rounded-xl h-10 border-border/80" aria-label="Filter by status" data-testid="status-filter">
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
                    className="block rounded-xl border border-border/80 bg-white p-4 hover:bg-muted/50 transition-colors duration-200 ease-in-out cursor-pointer min-h-[44px] shadow-sm"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0 flex-1">
                        <p className="font-mono text-[13px] font-semibold text-foreground"><LtrText>{inv.invoiceNumber}</LtrText></p>
                        <p className="text-[13px] text-muted-foreground mt-0.5">{inv.patientName}</p>
                      </div>
                      <StatusBadge status={inv.status} />
                    </div>
                    <div className="mt-2 flex items-center justify-between text-sm">
                      <span className="text-muted-foreground text-[13px]"><LtrText>{formatDate(inv.createdAt)}</LtrText></span>
                      <span className="font-bold text-foreground text-[13px] tabular-nums"><LtrText>{formatAED(inv.total)}</LtrText></span>
                    </div>
                  </Link>
                ))}
              </div>
            )}

            {/* Desktop table */}
            <Table className="hidden md:table" data-testid="invoice-table">
              <TableHeader>
                <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
                  <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground"># Invoice</TableHead>
                  <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">Patient</TableHead>
                  <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">Date</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">Subtotal (excl. VAT)</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">VAT (5%)</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">Total AED</TableHead>
                  <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoices.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7} className="p-0">
                      <EmptyState
                        icon={<Receipt className="h-16 w-16" />}
                        title={tEmpty('title')}
                        description={tEmpty('description')}
                        primaryCta={{ label: tEmpty('cta'), href: `/${locale}/billing/new` }}
                        tip={tEmpty('tip')}
                        data-testid-prefix="billing"
                      />
                    </TableCell>
                  </TableRow>
                )}
                {invoices.map((inv) => (
                  <TableRow
                    key={inv.id}
                    data-testid={`invoice-row-${inv.id}`}
                    className="hover:bg-muted/50 cursor-pointer transition-colors border-border/30"
                    onClick={() => router.push(`/${locale}/billing/${inv.id}`)}
                  >
                    <TableCell className="font-mono text-[13px] font-semibold text-foreground" data-testid="invoice-number">
                      <LtrText>{inv.invoiceNumber}</LtrText>
                    </TableCell>
                    <TableCell className="text-[13px] text-foreground font-medium" data-testid="invoice-patient">{inv.patientName}</TableCell>
                    <TableCell className="text-[13px] text-muted-foreground" data-testid="invoice-date"><LtrText>{formatDate(inv.createdAt)}</LtrText></TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] text-foreground" data-testid="invoice-subtotal">
                      <LtrText>{formatAED(inv.subtotal)}</LtrText>
                    </TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] text-muted-foreground" data-testid="invoice-vat">
                      <LtrText>{formatAED(inv.vatAmount)}</LtrText>
                    </TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] font-bold text-foreground" data-testid="invoice-total">
                      <LtrText>{formatAED(inv.total)}</LtrText>
                    </TableCell>
                    <TableCell>
                      <StatusBadge status={inv.status} />
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            {invoices.length > 0 && (
              <div className="mt-4 flex justify-end border-t border-border/30 pt-3" data-testid="invoice-summary">
                <div className="text-end space-y-1" dir="ltr">
                  <p className="text-[13px] text-muted-foreground font-medium">
                    {invoices.length} invoice{invoices.length !== 1 ? 's' : ''} — Total filtered:
                  </p>
                  <p className="text-lg font-bold text-foreground" data-testid="invoice-grand-total">
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
