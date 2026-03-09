import Link from 'next/link'
import { InvoiceForm } from '@/components/features/billing/InvoiceForm'

export default function NewInvoicePage() {
  return (
    <div className="space-y-6" data-testid="new-invoice-page">
      <div className="flex items-center gap-3">
        <Link href="/billing" className="text-sm text-muted-foreground hover:text-foreground">
          ← Billing
        </Link>
        <h1 className="text-2xl font-bold" data-testid="new-invoice-title">
          New Invoice
        </h1>
      </div>
      <InvoiceForm />
    </div>
  )
}
