import { InvoiceDetail } from '@/components/features/billing/InvoiceDetail'

interface Props {
  params: Promise<{ id: string }>
}

export default async function InvoiceDetailPage({ params }: Props) {
  const { id } = await params

  return (
    <div className="space-y-6" data-testid="invoice-detail-page">
      <h1 className="text-2xl font-bold" data-testid="invoice-detail-title">
        Invoice Detail
      </h1>
      <InvoiceDetail id={id} />
    </div>
  )
}
