import { InvoiceDetail } from '@/components/features/billing/InvoiceDetail'
import { getTranslations } from 'next-intl/server'

interface Props {
  params: Promise<{ id: string }>
}

export default async function InvoiceDetailPage({ params }: Props) {
  const { id } = await params
  const t = await getTranslations('billing')

  return (
    <div className="space-y-6 animate-in fade-in duration-300" data-testid="invoice-detail-page">
      <h1 className="text-2xl font-bold" data-testid="invoice-detail-title">
        {t('invoices')}
      </h1>
      <InvoiceDetail id={id} />
    </div>
  )
}
