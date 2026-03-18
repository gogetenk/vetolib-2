import { InvoiceTable } from "@/components/features/billing/InvoiceTable";
import { getTranslations } from 'next-intl/server'

export default async function BillingPage() {
  const t = await getTranslations('billing')

  return (
    <div className="p-6 lg:p-8 space-y-6 animate-in fade-in duration-300" data-testid="billing-page">
      <div className="flex items-center justify-between">
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2" data-testid="billing-title">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {t('title')}
        </h1>
      </div>
      <InvoiceTable />
    </div>
  );
}
