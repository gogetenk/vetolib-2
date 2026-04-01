import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import { PrivacyContent } from "./PrivacyContent";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "privacy" });

  return {
    title: t("meta_title"),
    description: t("meta_description"),
  };
}

export default function PrivacyPage() {
  return <PrivacyContent />;
}
