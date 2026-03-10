'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
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
  const [invoice, setInvoice] = useState<InvoiceDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [actionLoading, setActionLoading] = useState(false)

  useEffect(() => {
    async function load() {
      try {
        const data = await getInvoice(id)
        setInvoice(data)
        setError(null)
      } catch {
        setError('Invoice not found')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [id])

  // Auto-retry once after 1.5s if an error occurred (handles MSW initialisation race)
  useEffect(() => {
    if (!error || invoice) return
    const timer = setTimeout(async () => {
      setLoading(true)
      setError(null)
      try {
        const data = await getInvoice(id)
        setInvoice(data)
      } catch {
        setError('Invoice not found')
      } finally {
        setLoading(false)
      }
    }, 1500)
    return () => clearTimeout(timer)
  }, [error, id, invoice])

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
      setError('Failed to send invoice')
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
      setError('Failed to mark as paid')
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
      setError('Failed to cancel invoice')
    } finally {
      setActionLoading(false)
    }
  }

  async function handleDelete() {
    if (!invoice) return
    if (!confirm('Delete this invoice? This action cannot be undone.')) return
    setActionLoading(true)
    try {
      await deleteInvoice(invoice.id)
      router.push('/billing')
    } catch {
      setError('Failed to delete invoice')
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
      setError('Failed to download PDF')
    } finally {
      setActionLoading(false)
    }
  }

  if (loading) {
    return <p className="text-sm text-muted-foreground" data-testid="invoice-detail-loading">Loading...</p>
  }

  if (error && !invoice) {
    return <p className="text-sm text-destructive" data-testid="invoice-detail-error">{error}</p>
  }

  if (!invoice) return null

  return (
    <div className="space-y-6" data-testid="invoice-detail">
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
                Created: {formatDate(invoice.createdAt)}
              </p>
              {invoice.dueDate && (
                <p className="text-sm text-muted-foreground" data-testid="invoice-due-date">
                  Due: {formatDate(invoice.dueDate)}
                </p>
              )}
              {invoice.paidAt && (
                <p className="text-sm text-green-600 font-medium" data-testid="invoice-paid-date">
                  Paid on: {formatDate(invoice.paidAt)}
                </p>
              )}
            </div>
            {/* Clinic placeholder */}
            <div className="text-right text-sm text-muted-foreground">
              <p className="font-semibold text-foreground">Happy Paws Veterinary</p>
              <p>Dubai, UAE</p>
              <p>+971 4 000 0000</p>
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
          <p className="text-sm text-muted-foreground" data-testid="invoice-owner-phone">{invoice.ownerPhone}</p>
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
                  <TableCell className="text-right">{formatAED(item.unitPrice)}</TableCell>
                  <TableCell className="text-right">{formatAED(item.subtotal)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {/* Totals */}
          <div className="mt-4 border-t pt-4 space-y-1 text-sm max-w-xs ml-auto" data-testid="detail-totals">
            <div className="flex justify-between">
              <span>Subtotal HT</span>
              <span data-testid="detail-subtotal">{formatAED(invoice.subtotal)}</span>
            </div>
            <div className="flex justify-between text-muted-foreground">
              <span>VAT (5%)</span>
              <span data-testid="detail-vat">{formatAED(invoice.vatAmount)}</span>
            </div>
            <div className="flex justify-between font-bold text-base border-t pt-1">
              <span>Total AED</span>
              <span data-testid="detail-total">{formatAED(invoice.total)}</span>
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
            ← Back to Billing
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
            <Link href={`/billing/${invoice.id}/edit`}>
              <Button variant="outline" data-testid="edit-invoice-btn">
                Edit
              </Button>
            </Link>
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

      {error && (
        <p className="text-sm text-destructive" data-testid="action-error">{error}</p>
      )}
    </div>
  )
}
