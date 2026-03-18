import type { Metadata } from "next";
import { Manrope } from "next/font/google";
import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { notFound } from "next/navigation";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Toaster } from "@/components/ui/sonner";
import { MSWProvider } from "@/components/MSWProvider";
import { routing } from "@/i18n/routing";

const manrope = Manrope({
  variable: "--font-manrope",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Vetolib — Veterinary Clinic Management",
  description:
    "Manage appointments, patients, medical records, and billing for your veterinary clinic.",
};

interface Props {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}

export default async function LocaleLayout({ children, params }: Props) {
  const { locale } = await params;

  // Validate locale
  if (!routing.locales.includes(locale as 'en' | 'ar')) {
    notFound();
  }

  const messages = await getMessages();

  return (
    <html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'} className={`${manrope.variable} font-sans`}>
      <body
        className="antialiased"
      >
        <NextIntlClientProvider messages={messages}>
          <TooltipProvider>
            <MSWProvider>
              {children}
            </MSWProvider>
            <Toaster />
          </TooltipProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
