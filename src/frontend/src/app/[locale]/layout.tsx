import type { Metadata } from "next";
import { Manrope } from "next/font/google";
import { NextIntlClientProvider } from "next-intl";
import { getMessages } from "next-intl/server";
import { headers } from "next/headers";
import { notFound } from "next/navigation";
import { TooltipProvider } from "@/components/ui/tooltip";
import { Toaster } from "@/components/ui/sonner";
import { MSWProvider } from "@/components/MSWProvider";
import { GoogleAnalytics } from "@/components/GoogleAnalytics";
import { ThemeProvider } from "@/components/ThemeProvider";
import { CookieConsent } from "@/components/CookieConsent";
import { routing } from "@/i18n/routing";

const manrope = Manrope({
  variable: "--font-manrope",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: {
    template: "%s — Vetara",
    default: "Vetara — Veterinary Clinic Management",
  },
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

  // Build hreflang path by stripping the current locale prefix
  const headersList = await headers();
  const pathname = headersList.get("x-pathname") ?? "";
  // Strip /{locale} prefix to get the sub-path (e.g. "/en/blog/foo" -> "/blog/foo")
  const localePrefix = `/${locale}`;
  const subPath = pathname.startsWith(localePrefix)
    ? pathname.slice(localePrefix.length)
    : pathname.replace(/^\/[a-z]{2}(?=\/|$)/, "");

  return (
    <html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'} className={`${manrope.variable} font-sans`} suppressHydrationWarning>
      <head>
        <link rel="alternate" hrefLang="en" href={`/en${subPath}`} />
        <link rel="alternate" hrefLang="ar" href={`/ar${subPath}`} />
        <link rel="alternate" hrefLang="fr" href={`/fr${subPath}`} />
        <link rel="alternate" hrefLang="x-default" href={`/en${subPath}`} />
        <link rel="manifest" href="/manifest.json" />
        <meta name="theme-color" content="#16a34a" />
        <meta name="apple-mobile-web-app-capable" content="yes" />
        <meta name="apple-mobile-web-app-status-bar-style" content="default" />
        <meta name="apple-mobile-web-app-title" content="Vetara" />
        <link rel="apple-touch-icon" href="/icons/icon-192.svg" />
      </head>
      <GoogleAnalytics />
      <body
        className="antialiased"
      >
        <ThemeProvider attribute="class" defaultTheme="system" enableSystem disableTransitionOnChange>
          <NextIntlClientProvider messages={messages}>
            <TooltipProvider>
              <MSWProvider>
                {children}
              </MSWProvider>
              <Toaster />
              <CookieConsent />
            </TooltipProvider>
          </NextIntlClientProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
