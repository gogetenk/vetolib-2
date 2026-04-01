import type { Metadata } from "next";
import Link from "next/link";
import { InvoiceTable } from "@/components/features/billing/InvoiceTable";
import { PageContainer } from "@/components/ui/page-container";
import { getTranslations, getLocale } from 'next-intl/server'

export const metadata: Metadata = {
  title: "Billing",
};

export default async function BillingPage() {
  const t = await getTranslations('billing')
  const tReporting = await getTranslations('billing.reporting')
  const locale = await getLocale()

  return (
    <PageContainer data-testid="billing-page">
      <div className="flex items-center justify-between">
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="billing-title">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t('title')}
        </h1>
        <Link href={`/${locale}/billing/reporting`} data-testid="ereporting-link">
          <span className="text-sm font-medium text-primary hover:underline">
            {tReporting('e_reporting')}
          </span>
        </Link>
      </div>
      <InvoiceTable />
    </PageContainer>
  );
}
