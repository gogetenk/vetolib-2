import Link from 'next/link'
import { InvoiceForm } from '@/components/features/billing/InvoiceForm'
import { getTranslations } from 'next-intl/server'

export default async function NewInvoicePage() {
  const t = await getTranslations('billing')

  return (
    <div className="space-y-6" data-testid="new-invoice-page">
      <div className="flex items-center gap-3">
        <Link href="/billing" className="text-sm text-muted-foreground hover:text-foreground">
          ← {t('title')}
        </Link>
        <h1 className="text-2xl font-bold" data-testid="new-invoice-title">
          {t('form.title')}
        </h1>
      </div>
      <InvoiceForm />
    </div>
  )
}
