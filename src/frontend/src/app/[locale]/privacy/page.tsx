"use client";

import Link from "next/link";
import { useTranslations, useLocale } from "next-intl";

const SECTION_KEYS = [
  "introduction",
  "data_controller",
  "data_collected",
  "legal_basis",
  "data_use",
  "data_sharing",
  "data_residency",
  "data_security",
  "data_retention",
  "your_rights",
  "cookies",
  "children",
  "changes",
  "contact",
] as const;

export default function PrivacyPage() {
  const t = useTranslations("privacy");
  const locale = useLocale();

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-4xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-xl font-bold tracking-tight text-primary"
            data-testid="privacy-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href={`/${locale}`}
              className="text-sm font-medium text-stone-600 transition-colors hover:text-primary"
              data-testid="privacy-nav-home"
            >
              {t("nav_home")}
            </Link>
            <span
              className="text-sm font-medium text-primary"
              data-testid="privacy-nav-current"
            >
              {t("nav_privacy")}
            </span>
          </div>
        </nav>
      </header>

      {/* Content */}
      <main className="mx-auto max-w-4xl px-4 py-12 sm:px-6 lg:px-8">
        <h1
          className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl"
          data-testid="privacy-title"
        >
          {t("title")}
        </h1>
        <p className="mt-2 text-sm text-stone-500" data-testid="privacy-last-updated">
          {t("last_updated")}
        </p>

        <div className="mt-10 space-y-10">
          {SECTION_KEYS.map((key) => (
            <section key={key} data-testid={`privacy-section-${key}`}>
              <h2 className="text-xl font-semibold text-stone-900">
                {t(`sections.${key}.title`)}
              </h2>
              <p className="mt-3 leading-relaxed text-stone-700">
                {t(`sections.${key}.content`)}
              </p>
            </section>
          ))}
        </div>

        {/* Footer links */}
        <div className="mt-16 border-t border-stone-200 pt-8">
          <div className="flex flex-wrap gap-6 text-sm text-stone-500">
            <Link
              href={`/${locale}/terms`}
              className="transition-colors hover:text-primary"
              data-testid="privacy-terms-link"
            >
              Terms of Service
            </Link>
            <Link
              href={`/${locale}`}
              className="transition-colors hover:text-primary"
              data-testid="privacy-home-link"
            >
              Back to Home
            </Link>
          </div>
        </div>
      </main>
    </div>
  );
}
