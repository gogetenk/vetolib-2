import type { Metadata } from "next";
import { ReportingTable } from "@/components/features/billing/ReportingTable";
import { PageContainer } from "@/components/ui/page-container";
import { getTranslations } from 'next-intl/server'

export const metadata: Metadata = {
  title: "E-Reporting",
};

export default async function EReportingPage() {
  const t = await getTranslations('billing.reporting')

  return (
    <PageContainer data-testid="ereporting-page">
      <div className="flex items-center justify-between">
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="ereporting-title">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t('page_title')}
        </h1>
      </div>
      <ReportingTable />
    </PageContainer>
  );
}
