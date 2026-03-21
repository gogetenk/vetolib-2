'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { useLocale } from 'next-intl'
import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { toast } from 'sonner'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Skeleton } from '@/components/ui/skeleton'
import { ErrorState } from '@/components/ui/error-state'
import { LtrText } from '@/components/ui/ltr-text'
import { cn } from '@/lib/utils'
import { formatAED, formatDate } from '@/lib/utils'
import {
  getInvoice,
  sendInvoice,
  markAsPaid,
  cancelInvoice,
  deleteInvoice,
  downloadInvoicePdf,
} from '@/lib/api/billing'
import type { InvoiceDto, InvoiceStatus } from '@/lib/api/billing'
import { trackEvent, AnalyticsEvents } from '@/lib/analytics'

const STATUS_BADGE_STYLES: Record<InvoiceStatus, string> = {
  DRAFT: 'bg-amber-50 text-amber-700 border-amber-200',
  SENT: 'bg-primary/10 text-primary border-primary/30',
  PAID: 'bg-emerald-50 text-emerald-700 border-emerald-200',
  CANCELLED: 'bg-muted text-muted-foreground border-border/50',
}

function StatusBadge({ status }: { status: InvoiceStatus }) {
  const style = STATUS_BADGE_STYLES[status] ?? 'bg-muted text-muted-foreground border-border/50'
  return (
    <span
      className={cn('inline-flex items-center rounded-md px-2.5 py-0.5 text-[10px] font-bold uppercase tracking-wider border', style)}
      data-testid="invoice-detail-status"
    >
      {status}
    </span>
  )
}

interface InvoiceDetailProps {
  id: string
}

export function InvoiceDetail({ id }: InvoiceDetailProps) {
  const router = useRouter()
  const locale = useLocale()

  const [invoice, setInvoice] = useState<InvoiceDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [loadError, setLoadError] = useState<string | null>(null)
  const [lastActionError, setLastActionError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)

  const loadInvoice = useCallback(async () => {
    setLoading(true)
    setLoadError(null)
    try {
      const data = await getInvoice(id)
      setInvoice(data)
    } catch {
      setLoadError('Invoice not found')
    } finally {
      setLoading(false)
    }
  }, [id])

  useEffect(() => {
    loadInvoice()
  }, [loadInvoice])

  // Auto-retry once after 1.5s if an error occurred (handles MSW initialisation race)
  useEffect(() => {
    if (!loadError || invoice) return
    const timer = setTimeout(async () => {
      setLoading(true)
      setLoadError(null)
      try {
        const data = await getInvoice(id)
        setInvoice(data)
      } catch {
        setLoadError('Invoice not found')
      } finally {
        setLoading(false)
      }
    }, 1500)
    return () => clearTimeout(timer)
  }, [loadError, id, invoice])

  async function handleSend() {
    if (!invoice) return
    setActionLoading(true)
    const fromStatus = invoice.status
    try {
      const updated = await sendInvoice(invoice.id)
      trackEvent(AnalyticsEvents.INVOICE_STATUS_CHANGED, {
        from_status: fromStatus,
        to_status: updated.status,
      })
      setInvoice(updated)
    } catch {
      const msg = 'Failed to send invoice'
      setLastActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  async function handleMarkPaid() {
    if (!invoice) return
    setActionLoading(true)
    const fromStatus = invoice.status
    try {
      const updated = await markAsPaid(invoice.id)
      trackEvent(AnalyticsEvents.INVOICE_STATUS_CHANGED, {
        from_status: fromStatus,
        to_status: updated.status,
      })
      setInvoice(updated)
    } catch {
      const msg = 'Failed to mark as paid'
      setLastActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  async function handleCancel() {
    if (!invoice) return
    setActionLoading(true)
    const fromStatus = invoice.status
    try {
      const updated = await cancelInvoice(invoice.id)
      trackEvent(AnalyticsEvents.INVOICE_STATUS_CHANGED, {
        from_status: fromStatus,
        to_status: updated.status,
      })
      setInvoice(updated)
    } catch {
      const msg = 'Failed to cancel invoice'
      setLastActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  function handleDelete() {
    if (!invoice) return
    setDeleteDialogOpen(true)
  }

  async function handleDeleteConfirm() {
    if (!invoice) return
    setDeleteDialogOpen(false)
    setActionLoading(true)
    try {
      await deleteInvoice(invoice.id)
      router.push(`/${locale}/billing`)
    } catch {
      const msg = 'Failed to delete invoice'
      setLastActionError(msg)
      toast.error(msg)
      setActionLoading(false)
    }
  }

  async function handleDownloadPdf() {
    if (!invoice) return
    setActionLoading(true)
    try {
      const blob = await downloadInvoicePdf(invoice.id)
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `${invoice.invoiceNumber}.pdf`
      a.click()
      URL.revokeObjectURL(url)
      trackEvent(AnalyticsEvents.INVOICE_PDF_DOWNLOADED)
    } catch {
      const msg = 'Failed to download PDF'
      setLastActionError(msg)
      toast.error(msg)
    } finally {
      setActionLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="space-y-3" data-testid="invoice-detail-loading">
        <Skeleton className="h-32 w-full rounded-xl" style={{ animationDelay: '0ms' }} />
        <Skeleton className="h-24 w-full rounded-xl" style={{ animationDelay: '100ms' }} />
        <Skeleton className="h-48 w-full rounded-xl" style={{ animationDelay: '200ms' }} />
      </div>
    )
  }

  if (loadError && !invoice) {
    return (
      <ErrorState
        data-testid="invoice-detail-error"
        title="Invoice not found"
        description={loadError}
        onRetry={loadInvoice}
      />
    )
  }

  if (!invoice) return null

  return (
    <>
    <div className="space-y-3 animate-in fade-in duration-300" data-testid="invoice-detail">
      {/* Visually hidden alert for test accessibility — action errors are shown via toast */}
      {lastActionError && (
        <span
          role="alert"
          data-testid="action-error"
          className="sr-only"
        >
          {lastActionError}
        </span>
      )}

      {/* Header — single row: invoice number (left), status (center), date (right) */}
      <Card className="bg-white border-border/80 rounded-xl shadow-sm">
        <CardHeader>
          <div className="flex items-center justify-between" data-testid="invoice-detail-header">
            <span className="text-[16px] font-bold text-foreground" data-testid="invoice-detail-number">
              {invoice.invoiceNumber}
            </span>
            <StatusBadge status={invoice.status} />
            <div className="text-[13px] text-muted-foreground text-end" data-testid="invoice-detail-date">
              <LtrText>{formatDate(invoice.createdAt)}</LtrText>
              {invoice.dueDate && (
                <p className="text-muted-foreground/70" data-testid="invoice-due-date">
                  Due: <LtrText>{formatDate(invoice.dueDate)}</LtrText>
                </p>
              )}
              {invoice.paidAt && (
                <p className="text-emerald-600 font-semibold" data-testid="invoice-paid-date">
                  Paid: <LtrText>{formatDate(invoice.paidAt)}</LtrText>
                </p>
              )}
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Client Info + Clinic */}
      <Card className="bg-white border-border/80 rounded-xl shadow-sm">
        <CardHeader className="pb-2">
          <CardTitle className="text-[15px] font-bold text-foreground">Client</CardTitle>
        </CardHeader>
        <CardContent className="flex justify-between">
          <div>
            <p className="font-semibold text-[14px] text-foreground" data-testid="invoice-owner-name">{invoice.ownerName}</p>
            <p className="text-[13px] text-muted-foreground" data-testid="invoice-owner-phone"><LtrText>{invoice.ownerPhone}</LtrText></p>
            <p className="text-[13px] text-muted-foreground">Patient: <span className="font-semibold text-foreground" data-testid="invoice-patient-name">{invoice.patientName}</span></p>
          </div>
          <div className="text-end text-[13px] text-muted-foreground">
            <p className="font-semibold text-foreground">Happy Paws Veterinary</p>
            <p>Dubai, UAE</p>
            <LtrText as="p">+971 4 000 0000</LtrText>
          </div>
        </CardContent>
      </Card>

      {/* Line Items */}
      <Card className="bg-white border-border/80 rounded-xl shadow-sm">
        <CardHeader className="pb-2">
          <CardTitle className="text-[15px] font-bold text-foreground">Items</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="border border-border/80 rounded-xl overflow-hidden">
            <Table data-testid="invoice-items-table">
              <TableHeader>
                <TableRow className="bg-muted">
                  <TableHead className="text-[11px] font-bold uppercase tracking-wider text-foreground">Description</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">Qty</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">Unit Price</TableHead>
                  <TableHead className="text-end text-[11px] font-bold uppercase tracking-wider text-foreground">Subtotal</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {invoice.items.map((item) => (
                  <TableRow key={item.id} data-testid={`detail-item-${item.id}`} className="hover:bg-muted/50 border-border/30 transition-colors">
                    <TableCell className="text-[13px] text-foreground">{item.description}</TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] text-foreground">{item.quantity}</TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] text-muted-foreground"><LtrText>{formatAED(item.unitPrice)}</LtrText></TableCell>
                    <TableCell className="text-end tabular-nums text-[13px] font-semibold text-foreground"><LtrText>{formatAED(item.subtotal)}</LtrText></TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {/* Totals */}
          <div className="mt-4 rounded-xl bg-muted border border-border/50 px-4 py-3 space-y-1.5 text-[13px] max-w-xs ms-auto" data-testid="detail-totals">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Subtotal (excl. VAT)</span>
              <LtrText className="font-semibold text-foreground tabular-nums" data-testid="detail-subtotal">{formatAED(invoice.subtotal)}</LtrText>
            </div>
            <div className="flex justify-between">
              <span className="text-muted-foreground">VAT (5%)</span>
              <LtrText className="text-muted-foreground tabular-nums" data-testid="detail-vat">{formatAED(invoice.vatAmount)}</LtrText>
            </div>
            <div className="flex justify-between font-bold text-[15px] text-foreground border-t border-border/50 pt-1.5">
              <span>Total AED</span>
              <LtrText className="tabular-nums" data-testid="detail-total">{formatAED(invoice.total)}</LtrText>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Notes */}
      {invoice.notes && (
        <Card className="bg-white border-border/80 rounded-xl shadow-sm">
          <CardHeader className="pb-2">
            <CardTitle className="text-[15px] font-bold text-foreground">Notes</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-[13px] text-muted-foreground" data-testid="invoice-notes">{invoice.notes}</p>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex gap-3 flex-wrap" data-testid="invoice-actions">
        <Link href={`/${locale}/billing`}>
          <Button variant="outline" data-testid="back-to-billing-btn" className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted group/back">
            <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" /> Back to Billing
          </Button>
        </Link>

        {invoice.status === 'DRAFT' && (
          <>
            <Button
              data-testid="send-invoice-btn"
              onClick={handleSend}
              disabled={actionLoading}
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-10 px-5 shadow-sm"
            >
              Send
            </Button>
            <Button
              variant="destructive"
              data-testid="delete-invoice-btn"
              onClick={handleDelete}
              disabled={actionLoading}
              className="rounded-xl h-10 px-5 font-semibold"
            >
              Delete
            </Button>
          </>
        )}

        {invoice.status === 'SENT' && (
          <>
            <Button
              data-testid="mark-paid-btn"
              onClick={handleMarkPaid}
              disabled={actionLoading}
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-10 px-5 shadow-sm"
            >
              Mark as Paid
            </Button>
            <Button
              variant="outline"
              data-testid="cancel-invoice-btn"
              onClick={handleCancel}
              disabled={actionLoading}
              className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
            >
              Cancel
            </Button>
          </>
        )}

        {invoice.status === 'PAID' && (
          <Button
            variant="outline"
            data-testid="download-pdf-btn"
            onClick={handleDownloadPdf}
            disabled={actionLoading}
            className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
          >
            Download PDF
          </Button>
        )}
      </div>
    </div>

    <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
      <DialogContent data-testid="delete-confirm-dialog" className="rounded-2xl">
        <DialogHeader>
          <DialogTitle className="text-foreground font-bold">Delete Invoice</DialogTitle>
          <DialogDescription className="text-muted-foreground text-[13px]">
            This action cannot be undone. The invoice will be permanently deleted.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            variant="outline"
            data-testid="delete-confirm-cancel"
            onClick={() => setDeleteDialogOpen(false)}
            className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            data-testid="delete-confirm-ok"
            onClick={handleDeleteConfirm}
            disabled={actionLoading}
            className="rounded-xl h-10 px-5 font-semibold"
          >
            Delete
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    </>
  )
}
