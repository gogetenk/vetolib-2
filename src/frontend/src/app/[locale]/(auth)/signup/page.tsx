import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import { SignupForm } from "@/components/features/auth/SignupForm";

interface Props {
  params: Promise<{ locale: string }>;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "auth.signup" });

  return {
    title: t("meta_title"),
    description: t("meta_description"),
  };
}

export default function SignupPage() {
  return <SignupForm />;
}
