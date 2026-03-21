import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { PageContainer } from '@/components/ui/page-container'
import { InvoiceDetail } from '@/components/features/billing/InvoiceDetail'
import { getTranslations } from 'next-intl/server'

interface Props {
  params: Promise<{ id: string }>
}

export default async function InvoiceDetailPage({ params }: Props) {
  const { id } = await params
  const t = await getTranslations('billing')

  return (
    <PageContainer variant="default" data-testid="invoice-detail-page">
      <Link href="../billing">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-billing-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>

      <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="invoice-detail-title">
        <span className="w-1 h-5 bg-primary rounded-full"></span>
        {t('invoices')}
      </h1>

      <InvoiceDetail id={id} />
    </PageContainer>
  )
}
