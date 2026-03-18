import Link from 'next/link'
import { ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { InvoiceForm } from '@/components/features/billing/InvoiceForm'
import { getTranslations } from 'next-intl/server'

export default async function NewInvoicePage() {
  const t = await getTranslations('billing')

  return (
    <div className="p-6 lg:p-8 space-y-6 max-w-5xl mx-auto" data-testid="new-invoice-page">
      <Link href="../billing">
        <Button
          variant="ghost"
          size="sm"
          data-testid="back-to-billing-btn"
          className="-ms-2 group/back text-muted-foreground hover:text-[#061e44]"
        >
          <ArrowLeft className="h-4 w-4 me-1 transition-transform duration-200 ease-in-out group-hover/back:-translate-x-0.5 rtl:group-hover/back:translate-x-0.5" />
          {t('title')}
        </Button>
      </Link>

      <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="new-invoice-title">
        <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
        {t('form.title')}
      </h1>

      <InvoiceForm />
    </div>
  )
}
