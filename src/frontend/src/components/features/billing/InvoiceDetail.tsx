'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { toast } from 'sonner'
import { Badge } from '@/components/ui/badge'
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
import { useDirection } from '@/hooks/use-direction'

const STATUS_LABEL: Record<InvoiceStatus, string> = {
  DRAFT: 'Draft',
  SENT: 'Sent',
  PAID: 'Paid',
  CANCELLED: 'Cancelled',
}

const STATUS_VARIANT: Record<InvoiceStatus, 'secondary' | 'default' | 'outline' | 'destructive'> = {
  DRAFT: 'secondary',
  SENT: 'default',
  PAID: 'outline',
  CANCELLED: 'destructive',
}

interface InvoiceDetailProps {
  id: string
}

export function InvoiceDetail({ id }: InvoiceDetailProps) {
  const router = useRouter()
  const dir = useDirection()
  const backArrow = dir === 'rtl' ? '\u2192' : '\u2190'
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
      router.push('/billing')
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
      <div className="space-y-6" data-testid="invoice-detail-loading">
        <Skeleton className="h-32 w-full rounded-lg" />
        <Skeleton className="h-24 w-full rounded-lg" />
        <Skeleton className="h-48 w-full rounded-lg" />
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
    <div className="space-y-6" data-testid="invoice-detail">
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

      {/* Header */}
      <Card>
        <CardHeader>
          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-3 mb-1">
                <CardTitle data-testid="invoice-detail-number">{invoice.invoiceNumber}</CardTitle>
                <Badge variant={STATUS_VARIANT[invoice.status]} data-testid="invoice-detail-status">
                  {STATUS_LABEL[invoice.status]}
                </Badge>
              </div>
              <p className="text-sm text-muted-foreground" data-testid="invoice-detail-date">
                Created: <LtrText>{formatDate(invoice.createdAt)}</LtrText>
              </p>
              {invoice.dueDate && (
                <p className="text-sm text-muted-foreground" data-testid="invoice-due-date">
                  Due: <LtrText>{formatDate(invoice.dueDate)}</LtrText>
                </p>
              )}
              {invoice.paidAt && (
                <p className="text-sm text-green-600 font-medium" data-testid="invoice-paid-date">
                  Paid on: <LtrText>{formatDate(invoice.paidAt)}</LtrText>
                </p>
              )}
            </div>
            {/* Clinic placeholder */}
            <div className="text-right text-sm text-muted-foreground">
              <p className="font-semibold text-foreground">Happy Paws Veterinary</p>
              <p>Dubai, UAE</p>
              <LtrText as="p">+971 4 000 0000</LtrText>
            </div>
          </div>
        </CardHeader>
      </Card>

      {/* Client Info */}
      <Card>
        <CardHeader>
          <CardTitle>Client</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="font-medium" data-testid="invoice-owner-name">{invoice.ownerName}</p>
          <p className="text-sm text-muted-foreground" data-testid="invoice-owner-phone"><LtrText>{invoice.ownerPhone}</LtrText></p>
          <p className="text-sm text-muted-foreground">Patient: <span className="text-foreground" data-testid="invoice-patient-name">{invoice.patientName}</span></p>
        </CardContent>
      </Card>

      {/* Line Items */}
      <Card>
        <CardHeader>
          <CardTitle>Items</CardTitle>
        </CardHeader>
        <CardContent>
          <Table data-testid="invoice-items-table">
            <TableHeader>
              <TableRow>
                <TableHead>Description</TableHead>
                <TableHead className="text-right">Qty</TableHead>
                <TableHead className="text-right">Unit Price</TableHead>
                <TableHead className="text-right">Subtotal</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {invoice.items.map((item) => (
                <TableRow key={item.id} data-testid={`detail-item-${item.id}`}>
                  <TableCell>{item.description}</TableCell>
                  <TableCell className="text-right">{item.quantity}</TableCell>
                  <TableCell className="text-right"><LtrText>{formatAED(item.unitPrice)}</LtrText></TableCell>
                  <TableCell className="text-right"><LtrText>{formatAED(item.subtotal)}</LtrText></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {/* Totals */}
          <div className="mt-4 border-t pt-4 space-y-1 text-sm max-w-xs ml-auto" data-testid="detail-totals">
            <div className="flex justify-between">
              <span>Subtotal (excl. VAT)</span>
              <LtrText data-testid="detail-subtotal">{formatAED(invoice.subtotal)}</LtrText>
            </div>
            <div className="flex justify-between text-muted-foreground">
              <span>VAT (5%)</span>
              <LtrText data-testid="detail-vat">{formatAED(invoice.vatAmount)}</LtrText>
            </div>
            <div className="flex justify-between font-bold text-base border-t pt-1">
              <span>Total AED</span>
              <LtrText data-testid="detail-total">{formatAED(invoice.total)}</LtrText>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Notes */}
      {invoice.notes && (
        <Card>
          <CardHeader>
            <CardTitle>Notes</CardTitle>
          </CardHeader>
          <CardContent>
            <p className="text-sm" data-testid="invoice-notes">{invoice.notes}</p>
          </CardContent>
        </Card>
      )}

      {/* Actions */}
      <div className="flex gap-3 flex-wrap" data-testid="invoice-actions">
        <Link href="/billing">
          <Button variant="outline" data-testid="back-to-billing-btn">
            {backArrow} Back to Billing
          </Button>
        </Link>

        {invoice.status === 'DRAFT' && (
          <>
            <Button
              data-testid="send-invoice-btn"
              onClick={handleSend}
              disabled={actionLoading}
            >
              Send
            </Button>
            <Button
              variant="destructive"
              data-testid="delete-invoice-btn"
              onClick={handleDelete}
              disabled={actionLoading}
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
            >
              Mark as Paid
            </Button>
            <Button
              variant="outline"
              data-testid="cancel-invoice-btn"
              onClick={handleCancel}
              disabled={actionLoading}
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
          >
            Download PDF
          </Button>
        )}
      </div>
    </div>

    <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
      <DialogContent data-testid="delete-confirm-dialog">
        <DialogHeader>
          <DialogTitle>Delete Invoice</DialogTitle>
          <DialogDescription>
            This action cannot be undone. The invoice will be permanently deleted.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button
            variant="outline"
            data-testid="delete-confirm-cancel"
            onClick={() => setDeleteDialogOpen(false)}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            data-testid="delete-confirm-ok"
            onClick={handleDeleteConfirm}
            disabled={actionLoading}
          >
            Delete
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
    </>
  )
}
