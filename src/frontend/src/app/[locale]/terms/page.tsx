import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import { TermsContent } from "./TermsContent";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "terms" });

  return {
    title: t("meta_title"),
    description: t("meta_description"),
  };
}

export default function TermsPage() {
  return <TermsContent />;
}
