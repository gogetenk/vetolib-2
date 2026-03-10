import { InvoiceTable } from "@/components/features/billing/InvoiceTable";
import { getTranslations } from 'next-intl/server'

export default async function BillingPage() {
  const t = await getTranslations('billing')

  return (
    <div className="space-y-6" data-testid="billing-page">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold" data-testid="billing-title">
          {t('title')}
        </h1>
      </div>
      <InvoiceTable />
    </div>
  );
}
