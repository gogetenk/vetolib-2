'use client'

import { useEffect, useState, useCallback } from 'react'
import { FileText } from 'lucide-react'
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
import { ErrorState } from '@/components/ui/error-state'
import { Skeleton } from '@/components/ui/skeleton'
import { LtrText } from '@/components/ui/ltr-text'
import { cn } from '@/lib/utils'
import { formatAED, formatDate } from '@/lib/utils'
import { getReportingPeriods, submitReport } from '@/lib/api/e-reporting'
import type { ReportingPeriodDto, ReportStatus } from '@/lib/api/e-reporting'
import { useTranslations } from 'next-intl'

const MONTH_NAMES = [
  'january', 'february', 'march', 'april', 'may', 'june',
  'july', 'august', 'september', 'october', 'november', 'december',
] as const

const REPORT_STATUS_STYLES: Record<ReportStatus, string> = {
  DRAFT: 'bg-amber-50 text-amber-700',
  SUBMITTED: 'bg-blue-50 text-blue-700',
  ACCEPTED: 'bg-success/10 text-success',
  REJECTED: 'bg-destructive/10 text-destructive',
}

function ReportStatusBadge({ status }: { status: ReportStatus }) {
  const t = useTranslations('billing.reporting.status')
  const style = REPORT_STATUS_STYLES[status] ?? 'bg-muted text-muted-foreground'
  return (
    <Badge
      className={cn('text-[10px] font-bold uppercase tracking-wider border-0', style)}
      data-testid={`report-status-${status.toLowerCase()}`}
    >
      {t(status)}
    </Badge>
  )
}

export function ReportingTable() {
  const t = useTranslations('billing.reporting')
  const tMonths = useTranslations('billing.reporting.months')

  const [reports, setReports] = useState<ReportingPeriodDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [yearFilter, setYearFilter] = useState<string>('ALL')
  const [statusFilter, setStatusFilter] = useState<ReportStatus | 'ALL'>('ALL')
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState<string | null>(null)

  const statusOptions: { value: ReportStatus | 'ALL'; label: string }[] = [
    { value: 'ALL', label: t('all_statuses') },
    { value: 'DRAFT', label: t('status.DRAFT') },
    { value: 'SUBMITTED', label: t('status.SUBMITTED') },
    { value: 'ACCEPTED', label: t('status.ACCEPTED') },
    { value: 'REJECTED', label: t('status.REJECTED') },
  ]

  const loadReports = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const filters: { year?: number; status?: ReportStatus } = {}
      if (yearFilter !== 'ALL') filters.year = parseInt(yearFilter)
      if (statusFilter !== 'ALL') filters.status = statusFilter
      const data = await getReportingPeriods(filters)
      setReports(data)
    } catch {
      setError(t('failed_to_load'))
    } finally {
      setLoading(false)
    }
  }, [yearFilter, statusFilter, t])

  useEffect(() => {
    loadReports()
  }, [loadReports])

  const handleSubmit = useCallback(async (id: string) => {
    setSubmitting(id)
    try {
      const updated = await submitReport(id)
      setReports((prev) => prev.map((r) => (r.id === id ? updated : r)))
    } catch {
      setError(t('submit_failed'))
    } finally {
      setSubmitting(null)
    }
  }, [t])

  const years = Array.from(new Set(reports.map((r) => r.year))).sort((a, b) => b - a)
  const yearOptions = [
    { value: 'ALL', label: t('all_years') },
    ...years.map((y) => ({ value: String(y), label: String(y) })),
  ]

  const formatPeriod = (month: number, year: number) => {
    return `${tMonths(MONTH_NAMES[month - 1])} ${year}`
  }

  return (
    <Card className="border-border/80 shadow-sm">
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-[15px] font-bold text-foreground">{t('title')}</CardTitle>
        </div>
        <div className="flex flex-wrap gap-3 mt-3">
          <Select
            value={yearFilter}
            onValueChange={(val) => setYearFilter(val ?? 'ALL')}
          >
            <SelectTrigger className="w-36 rounded-xl h-10 border-border/80" aria-label={t('filter_by_year')} data-testid="year-filter">
              <SelectValue placeholder={t('all_years')} />
            </SelectTrigger>
            <SelectContent>
              {yearOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
          <Select
            value={statusFilter}
            onValueChange={(val) => setStatusFilter(val as ReportStatus | 'ALL')}
          >
            <SelectTrigger className="w-48 rounded-xl h-10 border-border/80" aria-label={t('filter_by_status')} data-testid="report-status-filter">
              <SelectValue placeholder={t('all_statuses')} />
            </SelectTrigger>
            <SelectContent>
              {statusOptions.map((opt) => (
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
          <div data-testid="reports-loading" className="space-y-3 py-2">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-12 w-full" style={{ animationDelay: `${(i - 1) * 100}ms` }} />
            ))}
          </div>
        )}
        {error && (
          <ErrorState
            data-testid="reports-error"
            title={t('failed_to_load')}
            description={error}
            onRetry={loadReports}
          />
        )}
        {!loading && !error && (
          <>
            {/* Mobile card layout */}
            {reports.length > 0 && (
              <div className="md:hidden space-y-3" data-testid="report-cards">
                {reports.map((report) => (
                  <div
                    key={report.id}
                    data-testid={`report-card-${report.id}`}
                    className="rounded-xl border border-border/80 bg-card p-4 shadow-sm"
                  >
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0 flex-1">
                        <p className="font-semibold text-[13px] text-foreground">{formatPeriod(report.month, report.year)}</p>
                        <p className="text-[13px] text-muted-foreground mt-0.5">
                          {t('invoice_count', { count: report.invoiceCount })}
                        </p>
                      </div>
                      <ReportStatusBadge status={report.status} />
                    </div>
                    <div className="mt-2 flex items-center justify-between text-sm">
                      <span className="text-muted-foreground text-[13px]">
                        <LtrText>{formatAED(report.totalVat)}</LtrText> {t('vat_label')}
                      </span>
                      <span className="font-bold text-foreground text-[13px] tabular-nums">
                        <LtrText>{formatAED(report.totalRevenue)}</LtrText>
                      </span>
                    </div>
                    <div className="mt-3 flex items-center gap-2">
                      <Button
                        variant="outline"
                        size="sm"
                        className="rounded-xl text-xs"
                        data-testid={`report-detail-btn-${report.id}`}
                        onClick={() => setExpandedId(expandedId === report.id ? null : report.id)}
                      >
                        {expandedId === report.id ? t('hide_details') : t('view_details')}
                      </Button>
                      {(report.status === 'DRAFT' || report.status === 'REJECTED') && (
                        <Button
                          size="sm"
                          className="rounded-xl text-xs font-semibold"
                          data-testid={`report-submit-btn-${report.id}`}
                          disabled={submitting === report.id}
                          onClick={() => handleSubmit(report.id)}
                        >
                          {submitting === report.id ? t('submitting') : t('submit')}
                        </Button>
                      )}
                    </div>
                    {expandedId === report.id && (
                      <ReportDetail report={report} />
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* Desktop table */}
            <Table className="hidden md:table" data-testid="report-table">
              <TableHeader>
                <TableRow className="bg-muted hover:bg-muted border-b border-border/50">
                  <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.period')}</TableHead>
                  <TableHead className="h-12 px-4 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.invoices')}</TableHead>
                  <TableHead className="h-12 px-4 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.revenue')}</TableHead>
                  <TableHead className="h-12 px-4 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.vat')}</TableHead>
                  <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.status')}</TableHead>
                  <TableHead className="h-12 px-4 text-[11px] font-bold uppercase tracking-wider text-foreground">{t('columns.actions')}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {reports.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-12 text-muted-foreground">
                      <FileText className="h-10 w-10 mx-auto mb-2 opacity-40" />
                      <p className="text-sm">{t('no_reports')}</p>
                    </TableCell>
                  </TableRow>
                )}
                {reports.map((report) => (
                  <TableRow
                    key={report.id}
                    data-testid={`report-row-${report.id}`}
                    className="group hover:bg-muted/50 transition-colors border-border/30"
                  >
                    <TableCell className="py-3 px-4 font-semibold text-[13px] text-foreground">
                      {formatPeriod(report.month, report.year)}
                    </TableCell>
                    <TableCell className="py-3 px-4 text-end tabular-nums text-[13px] text-foreground" data-testid="report-invoice-count">
                      {report.invoiceCount}
                    </TableCell>
                    <TableCell className="py-3 px-4 text-end tabular-nums text-[13px] font-bold text-foreground" data-testid="report-revenue">
                      <LtrText>{formatAED(report.totalRevenue)}</LtrText>
                    </TableCell>
                    <TableCell className="py-3 px-4 text-end tabular-nums text-[13px] text-muted-foreground" data-testid="report-vat">
                      <LtrText>{formatAED(report.totalVat)}</LtrText>
                    </TableCell>
                    <TableCell className="py-3 px-4">
                      <ReportStatusBadge status={report.status} />
                    </TableCell>
                    <TableCell className="py-3 px-4">
                      <div className="flex items-center gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          className="rounded-xl text-xs"
                          data-testid={`report-detail-btn-${report.id}`}
                          onClick={() => setExpandedId(expandedId === report.id ? null : report.id)}
                        >
                          {expandedId === report.id ? t('hide_details') : t('view_details')}
                        </Button>
                        {(report.status === 'DRAFT' || report.status === 'REJECTED') && (
                          <Button
                            size="sm"
                            className="rounded-xl text-xs font-semibold"
                            data-testid={`report-submit-btn-${report.id}`}
                            disabled={submitting === report.id}
                            onClick={() => handleSubmit(report.id)}
                          >
                            {submitting === report.id ? t('submitting') : t('submit')}
                          </Button>
                        )}
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>

            {/* Expanded detail panel */}
            {expandedId && (
              <div className="mt-4" data-testid="report-detail-panel">
                {reports
                  .filter((r) => r.id === expandedId)
                  .map((report) => (
                    <ReportDetail key={report.id} report={report} />
                  ))}
              </div>
            )}
          </>
        )}
      </CardContent>
    </Card>
  )
}

function ReportDetail({ report }: { report: ReportingPeriodDto }) {
  const t = useTranslations('billing.reporting')

  return (
    <div className="mt-4 rounded-lg border border-border/60 bg-muted/30 p-4" data-testid={`report-detail-${report.id}`}>
      <div className="flex flex-wrap gap-4 text-[13px] text-muted-foreground mb-4">
        {report.submittedAt && (
          <span data-testid="report-submitted-at">
            {t('submitted_at')}: <LtrText>{formatDate(report.submittedAt)}</LtrText>
          </span>
        )}
        {report.acceptedAt && (
          <span data-testid="report-accepted-at">
            {t('accepted_at')}: <LtrText>{formatDate(report.acceptedAt)}</LtrText>
          </span>
        )}
        {report.rejectedAt && (
          <span data-testid="report-rejected-at" className="text-destructive">
            {t('rejected_at')}: <LtrText>{formatDate(report.rejectedAt)}</LtrText>
          </span>
        )}
      </div>
      {report.rejectionReason && (
        <div className="mb-4 rounded-lg bg-destructive/5 border border-destructive/20 p-3" data-testid="report-rejection-reason">
          <p className="text-[13px] font-medium text-destructive">{t('rejection_reason')}</p>
          <p className="text-[13px] text-destructive/80 mt-1">{report.rejectionReason}</p>
        </div>
      )}
      <h4 className="text-[13px] font-bold text-foreground mb-2">{t('line_items')}</h4>
      <Table data-testid="report-line-items-table">
        <TableHeader>
          <TableRow className="bg-muted/50 hover:bg-muted/50">
            <TableHead className="h-9 px-3 text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.description')}</TableHead>
            <TableHead className="h-9 px-3 text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.category')}</TableHead>
            <TableHead className="h-9 px-3 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.qty')}</TableHead>
            <TableHead className="h-9 px-3 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.unit_price')}</TableHead>
            <TableHead className="h-9 px-3 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.vat')}</TableHead>
            <TableHead className="h-9 px-3 text-end text-[11px] font-bold uppercase tracking-wider text-foreground">{t('item_columns.total')}</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {report.lineItems.map((item) => (
            <TableRow key={item.id} data-testid={`line-item-${item.id}`} className="border-border/30">
              <TableCell className="py-2 px-3 text-[13px] text-foreground">{item.description}</TableCell>
              <TableCell className="py-2 px-3 text-[13px] text-muted-foreground">{item.category}</TableCell>
              <TableCell className="py-2 px-3 text-end tabular-nums text-[13px] text-foreground">{item.quantity}</TableCell>
              <TableCell className="py-2 px-3 text-end tabular-nums text-[13px] text-foreground">
                <LtrText>{formatAED(item.unitPrice)}</LtrText>
              </TableCell>
              <TableCell className="py-2 px-3 text-end tabular-nums text-[13px] text-muted-foreground">
                <LtrText>{formatAED(item.vatAmount)}</LtrText>
              </TableCell>
              <TableCell className="py-2 px-3 text-end tabular-nums text-[13px] font-bold text-foreground">
                <LtrText>{formatAED(item.total)}</LtrText>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <div className="mt-3 flex justify-end border-t border-border/30 pt-3" data-testid="report-totals">
        <div className="text-end space-y-1" dir="ltr">
          <p className="text-[13px] text-muted-foreground">
            {t('total_vat')}: <span className="font-medium">{formatAED(report.totalVat)}</span>
          </p>
          <p className="text-lg font-bold text-foreground" data-testid="report-total-revenue">
            {formatAED(report.totalRevenue)}
          </p>
        </div>
      </div>
    </div>
  )
}
