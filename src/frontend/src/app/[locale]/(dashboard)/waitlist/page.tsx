import type { Metadata } from "next";
import { WaitlistTable } from "@/components/features/waitlist/WaitlistTable";
import { PageContainer } from "@/components/ui/page-container";
import { getTranslations } from 'next-intl/server'

export const metadata: Metadata = {
  title: "Waitlist",
};

export default async function WaitlistPage() {
  const t = await getTranslations('waitlist')

  return (
    <PageContainer data-testid="waitlist-page">
      <div className="flex items-center justify-between">
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2" data-testid="waitlist-title">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t('title')}
        </h1>
      </div>
      <WaitlistTable />
    </PageContainer>
  );
}
