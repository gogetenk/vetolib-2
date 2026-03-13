import Link from "next/link";
import { ScrollReveal } from "./ScrollReveal";

interface FooterColumn {
  title: string;
  links: { label: string; href: string }[];
}

interface FooterMessages {
  copyright: string;
  tagline: string;
  lang_en: string;
  lang_ar: string;
  contact_hello: string;
  contact_support: string;
  columns: {
    product: {
      title: string;
      features: string;
      pricing: string;
      integrations: string;
      changelog: string;
    };
    company: {
      title: string;
      about: string;
      contact: string;
      careers: string;
    };
    resources: {
      title: string;
      help_center: string;
      api_docs: string;
      status: string;
    };
    legal: {
      title: string;
      privacy: string;
      terms: string;
      dpa: string;
    };
  };
}

interface Props {
  locale: string;
  messages: FooterMessages;
}

export function Footer({ locale, messages: m }: Props) {
  const columns: FooterColumn[] = [
    {
      title: m.columns.product.title,
      links: [
        { label: m.columns.product.features, href: "#features" },
        { label: m.columns.product.pricing, href: "#pricing" },
        { label: m.columns.product.integrations, href: "#" },
        { label: m.columns.product.changelog, href: "#" },
      ],
    },
    {
      title: m.columns.company.title,
      links: [
        { label: m.columns.company.about, href: "#" },
        {
          label: m.columns.company.contact,
          href: `mailto:${m.contact_hello}`,
        },
        { label: m.columns.company.careers, href: "#" },
      ],
    },
    {
      title: m.columns.resources.title,
      links: [
        { label: m.columns.resources.help_center, href: "#" },
        { label: m.columns.resources.api_docs, href: "#" },
        { label: m.columns.resources.status, href: "#" },
      ],
    },
    {
      title: m.columns.legal.title,
      links: [
        { label: m.columns.legal.privacy, href: "#" },
        { label: m.columns.legal.terms, href: "#" },
        { label: m.columns.legal.dpa, href: "#" },
      ],
    },
  ];

  return (
    <footer data-testid="section-footer" className="border-t border-gray-100 bg-gray-50">
      <ScrollReveal direction="fade-up">
      <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 lg:px-8 lg:py-16">
        {/* 4-column grid */}
        <div className="grid grid-cols-2 gap-8 sm:grid-cols-4">
          {columns.map((col) => (
            <div key={col.title}>
              <h3 className="text-xs font-semibold uppercase tracking-wider text-gray-500">
                {col.title}
              </h3>
              <ul className="mt-4 space-y-2">
                {col.links.map((link) => (
                  <li key={link.label}>
                    <a
                      href={link.href}
                      className="text-sm text-gray-600 transition-colors hover:text-emerald-700"
                    >
                      {link.label}
                    </a>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Contact + bottom bar */}
        <div className="mt-10 border-t border-gray-200 pt-8">
          <div className="flex flex-col items-start justify-between gap-6 sm:flex-row sm:items-center">
            {/* Brand + copyright */}
            <div>
              <p className="text-sm font-bold text-emerald-700">Vetolib</p>
              <p className="mt-1 text-xs text-gray-400">
                &copy; 2026 Vetolib. {m.tagline}
              </p>
            </div>

            {/* Contact emails */}
            <div
              data-testid="footer-contact"
              className="flex flex-col gap-1 text-xs text-gray-500"
            >
              <a
                href={`mailto:${m.contact_hello}`}
                className="hover:text-emerald-700"
              >
                {m.contact_hello}
              </a>
              <a
                href={`mailto:${m.contact_support}`}
                className="hover:text-emerald-700"
              >
                {m.contact_support}
              </a>
            </div>

            {/* Language switcher */}
            <div
              data-testid="footer-lang-switcher"
              className="flex items-center gap-2 rounded-full border border-gray-200 bg-white px-3 py-1.5"
            >
              <Link
                href={`/en`}
                data-testid="footer-lang-en"
                className={`text-xs font-medium transition-colors ${
                  locale === "en"
                    ? "text-emerald-700"
                    : "text-gray-400 hover:text-emerald-700"
                }`}
              >
                EN
              </Link>
              <span className="text-gray-200">|</span>
              <Link
                href={`/ar`}
                data-testid="footer-lang-ar"
                className={`text-xs font-medium transition-colors ${
                  locale === "ar"
                    ? "text-emerald-700"
                    : "text-gray-400 hover:text-emerald-700"
                }`}
              >
                AR
              </Link>
            </div>
          </div>
        </div>
      </div>
      </ScrollReveal>
    </footer>
  );
}
