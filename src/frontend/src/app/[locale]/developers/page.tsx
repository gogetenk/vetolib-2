import type { Metadata } from "next";
import { getTranslations } from "next-intl/server";
import Link from "next/link";
import {
  Code2,
  Shield,
  FileJson,
  ExternalLink,
  ArrowRight,
  Heart,
  Lock,
  Globe,
} from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props {
  params: Promise<{ locale: string }>;
}

const JSON_LD_API = {
  "@context": "https://schema.org",
  "@type": "WebAPI",
  name: "Vetara FHIR API",
  description:
    "Open Veterinary Health API built on FHIR R4. Export patient data, medical records, and more.",
  url: "https://vetara.com/en/developers",
  provider: {
    "@type": "Organization",
    name: "Vetara",
    url: "https://vetara.com",
  },
  documentation: "https://vetara.com/scalar/v1",
  termsOfService: "https://vetara.com/en/terms",
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "developers" });

  return {
    title: t("meta.title"),
    description: t("meta.description"),
    alternates: {
      canonical: `/${locale}/developers`,
      languages: {
        en: "/en/developers",
        ar: "/ar/developers",
        fr: "/fr/developers",
      },
    },
    openGraph: {
      title: t("meta.title"),
      description: t("meta.description"),
      type: "website",
    },
  };
}

export default async function DevelopersPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: "developers" });

  const endpoints = [
    {
      method: "GET",
      path: "/api/v1/fhir/export",
      description: t("endpoints.export_desc"),
      available: true,
    },
    {
      method: "POST",
      path: "/api/v1/fhir/import",
      description: t("endpoints.import_desc"),
      available: false,
    },
    {
      method: "GET",
      path: "/api/v1/portal/{token}/pets",
      description: t("endpoints.shared_records_desc"),
      available: true,
    },
  ];

  return (
    <div className="min-h-screen bg-white">
      {/* JSON-LD */}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(JSON_LD_API) }}
      />

      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-xl font-bold tracking-tight text-primary"
            data-testid="developers-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href={`/${locale}`}
              className="text-sm font-medium text-stone-600 transition-colors hover:text-primary"
              data-testid="developers-nav-home"
            >
              {t("nav.home")}
            </Link>
            <span
              className="text-sm font-medium text-primary"
              data-testid="developers-nav-current"
            >
              {t("nav.developers")}
            </span>
          </div>
        </nav>
      </header>

      <main>
        {/* Hero */}
        <section
          data-testid="developers-hero"
          className="relative overflow-hidden bg-gradient-to-br from-stone-900 via-stone-800 to-stone-900 py-20 sm:py-28"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-3xl text-center">
              <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-emerald-500/30 bg-emerald-500/10 px-4 py-1.5">
                <Code2
                  className="h-4 w-4 text-emerald-400"
                  aria-hidden="true"
                />
                <span className="text-sm font-medium text-emerald-400">
                  {t("hero.badge")}
                </span>
              </div>
              <h1
                className="text-3xl font-extrabold leading-tight tracking-tight text-white sm:text-4xl lg:text-5xl"
                data-testid="developers-hero-title"
              >
                {t("hero.title")}
              </h1>
              <p className="mt-6 text-lg leading-relaxed text-stone-300">
                {t("hero.subtitle")}
              </p>
              <div className="mt-8 flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
                <a
                  href="/scalar/v1"
                  target="_blank"
                  rel="noopener noreferrer"
                  data-testid="developers-cta-docs"
                >
                  <Button
                    size="lg"
                    className="bg-emerald-500 text-white hover:bg-emerald-600"
                  >
                    {t("hero.cta_docs")}
                    <ExternalLink className="ml-2 h-4 w-4" aria-hidden="true" />
                  </Button>
                </a>
                <a
                  href="#get-access"
                  data-testid="developers-cta-access"
                >
                  <Button
                    size="lg"
                    variant="outline"
                    className="border-stone-600 text-stone-200 hover:bg-stone-800 hover:text-white"
                  >
                    {t("hero.cta_access")}
                    <ArrowRight
                      className="ml-2 h-4 w-4"
                      aria-hidden="true"
                    />
                  </Button>
                </a>
              </div>
            </div>
          </div>
        </section>

        {/* FHIR R4 Section */}
        <section
          data-testid="developers-fhir"
          className="border-b border-stone-100 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="grid items-center gap-12 lg:grid-cols-2">
              <div>
                <div className="mb-4 inline-flex items-center gap-2 rounded-full bg-orange-50 px-3 py-1">
                  <Heart
                    className="h-4 w-4 text-orange-500"
                    aria-hidden="true"
                  />
                  <span className="text-sm font-medium text-orange-700">
                    FHIR R4
                  </span>
                </div>
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                  {t("fhir.title")}
                </h2>
                <p className="mt-4 text-base leading-relaxed text-stone-600">
                  {t("fhir.description")}
                </p>
                <ul className="mt-6 space-y-3">
                  {[0, 1, 2].map((i) => (
                    <li key={i} className="flex items-start gap-3">
                      <FileJson
                        className="mt-0.5 h-5 w-5 shrink-0 text-primary"
                        aria-hidden="true"
                      />
                      <span className="text-sm text-stone-700">
                        {t(`fhir.benefits.${i}`)}
                      </span>
                    </li>
                  ))}
                </ul>
                <a
                  href="https://www.hl7.org/fhir/"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="mt-6 inline-flex items-center gap-1 text-sm font-medium text-primary hover:underline"
                  data-testid="developers-fhir-learn-more"
                >
                  {t("fhir.learn_more")}
                  <ExternalLink className="h-3.5 w-3.5" aria-hidden="true" />
                </a>
              </div>
              <div className="rounded-2xl border border-stone-200 bg-stone-50 p-6">
                <p className="mb-3 text-xs font-semibold uppercase tracking-wider text-stone-500">
                  {t("fhir.example_title")}
                </p>
                <pre
                  className="overflow-x-auto rounded-lg bg-stone-900 p-4 text-sm leading-relaxed text-emerald-400"
                  data-testid="developers-fhir-example"
                >
                  <code>{`{
  "resourceType": "Patient",
  "id": "pet-12345",
  "name": [{
    "text": "Luna",
    "family": "Al Maktoum"
  }],
  "gender": "female",
  "birthDate": "2022-03-15",
  "extension": [{
    "url": "http://vetara.com/fhir/species",
    "valueString": "Canine"
  }]
}`}</code>
                </pre>
              </div>
            </div>
          </div>
        </section>

        {/* Endpoints Section */}
        <section
          data-testid="developers-endpoints"
          className="bg-stone-50 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                {t("endpoints.title")}
              </h2>
              <p className="mt-4 text-base text-stone-600">
                {t("endpoints.subtitle")}
              </p>
            </div>
            <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {endpoints.map((ep) => (
                <div
                  key={ep.path}
                  className="relative rounded-xl border border-stone-200 bg-white p-6 shadow-sm"
                  data-testid={`developers-endpoint-${ep.method.toLowerCase()}-${ep.path.split("/").pop()}`}
                >
                  <div className="mb-3 flex items-center gap-2">
                    <span
                      className={`rounded px-2 py-0.5 text-xs font-bold ${
                        ep.method === "GET"
                          ? "bg-emerald-100 text-emerald-700"
                          : "bg-blue-100 text-blue-700"
                      }`}
                    >
                      {ep.method}
                    </span>
                    <code className="text-xs text-stone-500">{ep.path}</code>
                  </div>
                  <p className="text-sm text-stone-600">{ep.description}</p>
                  {!ep.available && (
                    <span className="mt-3 inline-block rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-medium text-amber-700">
                      {t("endpoints.coming_soon")}
                    </span>
                  )}
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Authentication Section */}
        <section
          data-testid="developers-auth"
          className="border-b border-stone-100 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="grid items-start gap-12 lg:grid-cols-2">
              <div>
                <div className="mb-4 inline-flex items-center gap-2 rounded-full bg-blue-50 px-3 py-1">
                  <Lock
                    className="h-4 w-4 text-blue-500"
                    aria-hidden="true"
                  />
                  <span className="text-sm font-medium text-blue-700">
                    {t("auth.badge")}
                  </span>
                </div>
                <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                  {t("auth.title")}
                </h2>
                <p className="mt-4 text-base leading-relaxed text-stone-600">
                  {t("auth.description")}
                </p>
                <div className="mt-6 space-y-4">
                  <div className="flex items-start gap-3">
                    <Shield
                      className="mt-0.5 h-5 w-5 shrink-0 text-emerald-500"
                      aria-hidden="true"
                    />
                    <div>
                      <p className="text-sm font-semibold text-stone-900">
                        {t("auth.jwt_title")}
                      </p>
                      <p className="text-sm text-stone-600">
                        {t("auth.jwt_desc")}
                      </p>
                    </div>
                  </div>
                  <div className="flex items-start gap-3">
                    <Globe
                      className="mt-0.5 h-5 w-5 shrink-0 text-stone-400"
                      aria-hidden="true"
                    />
                    <div>
                      <p className="text-sm font-semibold text-stone-900">
                        {t("auth.oauth_title")}
                      </p>
                      <p className="text-sm text-stone-600">
                        {t("auth.oauth_desc")}
                      </p>
                      <span className="mt-1 inline-block rounded-full bg-amber-50 px-2.5 py-0.5 text-xs font-medium text-amber-700">
                        {t("endpoints.coming_soon")}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
              <div className="rounded-2xl border border-stone-200 bg-stone-50 p-6">
                <p className="mb-3 text-xs font-semibold uppercase tracking-wider text-stone-500">
                  {t("auth.example_title")}
                </p>
                <pre
                  className="overflow-x-auto rounded-lg bg-stone-900 p-4 text-sm leading-relaxed text-emerald-400"
                  data-testid="developers-auth-example"
                >
                  <code>{`# Authenticate
curl -X POST https://api.vetara.com/api/v1/auth/login \\
  -H "Content-Type: application/json" \\
  -d '{"email": "clinic@example.com", "password": "..."}'

# Use the token
curl https://api.vetara.com/api/v1/fhir/export \\
  -H "Authorization: Bearer <your-token>" \\
  -H "Accept: application/fhir+json"`}</code>
                </pre>
              </div>
            </div>
          </div>
        </section>

        {/* Code Example Section */}
        <section
          data-testid="developers-code-example"
          className="bg-stone-50 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                {t("code.title")}
              </h2>
              <p className="mt-4 text-base text-stone-600">
                {t("code.subtitle")}
              </p>
            </div>
            <div className="mx-auto mt-12 max-w-3xl">
              <div className="rounded-2xl border border-stone-200 bg-stone-900 shadow-lg">
                <div className="flex items-center gap-2 border-b border-stone-700 px-4 py-3">
                  <div className="h-3 w-3 rounded-full bg-red-500" />
                  <div className="h-3 w-3 rounded-full bg-yellow-500" />
                  <div className="h-3 w-3 rounded-full bg-green-500" />
                  <span className="ml-2 text-xs text-stone-400">
                    export-fhir.sh
                  </span>
                </div>
                <pre
                  className="overflow-x-auto p-6 text-sm leading-relaxed text-emerald-400"
                  data-testid="developers-curl-example"
                >
                  <code>{`#!/bin/bash
# Export all patient records as FHIR R4 Bundle
# Requires: clinic JWT token

TOKEN="your-jwt-token-here"
API_URL="https://api.vetara.com"

# Export patients as FHIR Bundle
curl -s "$API_URL/api/v1/fhir/export" \\
  -H "Authorization: Bearer $TOKEN" \\
  -H "Accept: application/fhir+json" \\
  | jq '.entry[] | .resource.name[0].text'

# Response: FHIR R4 Bundle with Patient resources
# {
#   "resourceType": "Bundle",
#   "type": "searchset",
#   "entry": [
#     { "resource": { "resourceType": "Patient", ... } }
#   ]
# }`}</code>
                </pre>
              </div>
            </div>
          </div>
        </section>

        {/* API Docs CTA */}
        <section
          data-testid="developers-scalar"
          className="border-b border-stone-100 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl">
                {t("scalar.title")}
              </h2>
              <p className="mt-4 text-base text-stone-600">
                {t("scalar.description")}
              </p>
              <a
                href="/scalar/v1"
                target="_blank"
                rel="noopener noreferrer"
                className="mt-8 inline-block"
                data-testid="developers-scalar-link"
              >
                <Button
                  size="lg"
                  className="bg-primary text-white hover:bg-primary/90"
                >
                  {t("scalar.cta")}
                  <ExternalLink className="ml-2 h-4 w-4" aria-hidden="true" />
                </Button>
              </a>
            </div>
          </div>
        </section>

        {/* Get API Access */}
        <section
          id="get-access"
          data-testid="developers-get-access"
          className="bg-gradient-to-br from-stone-900 via-stone-800 to-stone-900 py-20 sm:py-24"
        >
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="text-2xl font-bold tracking-tight text-white sm:text-3xl">
                {t("access.title")}
              </h2>
              <p className="mt-4 text-base text-stone-300">
                {t("access.description")}
              </p>
              <div className="mt-8 flex flex-col items-center gap-4 sm:flex-row sm:justify-center">
                <a
                  href="mailto:api@vetara.ae?subject=API Access Request"
                  data-testid="developers-access-email"
                >
                  <Button
                    size="lg"
                    className="bg-emerald-500 text-white hover:bg-emerald-600"
                  >
                    {t("access.cta")}
                    <ArrowRight
                      className="ml-2 h-4 w-4"
                      aria-hidden="true"
                    />
                  </Button>
                </a>
              </div>
              <p className="mt-6 text-sm text-stone-400">
                {t("access.note")}
              </p>
            </div>
          </div>
        </section>
      </main>

      {/* Minimal Footer */}
      <footer
        className="border-t border-stone-100 bg-stone-50"
        data-testid="developers-footer"
      >
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-6 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-sm font-bold text-primary"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4 text-xs text-stone-500">
            <Link
              href={`/${locale}/terms`}
              className="hover:text-primary"
            >
              {t("footer.terms")}
            </Link>
            <Link
              href={`/${locale}/privacy`}
              className="hover:text-primary"
            >
              {t("footer.privacy")}
            </Link>
            <span>&copy; 2026 Vetara</span>
          </div>
        </div>
      </footer>
    </div>
  );
}
