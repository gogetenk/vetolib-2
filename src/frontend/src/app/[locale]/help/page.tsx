"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { Search, ChevronDown, BookOpen, Layers, HelpCircle, Wrench, ArrowLeft, ExternalLink } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useTranslations } from "next-intl";

/* ------------------------------------------------------------------ */
/*  Article data – all 33 articles from the Help Center content       */
/* ------------------------------------------------------------------ */

interface Article {
  id: string;
  section: string;
  title: string;
  content: string;
}

const SECTION_KEYS = ["getting-started", "features", "faq", "troubleshooting"] as const;

const SECTION_ICONS: Record<string, typeof BookOpen> = {
  "getting-started": BookOpen,
  "features": Layers,
  "faq": HelpCircle,
  "troubleshooting": Wrench,
};

const ARTICLE_IDS: { id: string; section: string }[] = [
  /* Getting Started */
  { id: "1-1", section: "getting-started" },
  { id: "1-2", section: "getting-started" },
  { id: "1-3", section: "getting-started" },
  { id: "1-4", section: "getting-started" },
  { id: "1-5", section: "getting-started" },
  /* Features Guide */
  { id: "2-1", section: "features" },
  { id: "2-2", section: "features" },
  { id: "2-3", section: "features" },
  { id: "2-4", section: "features" },
  { id: "2-5", section: "features" },
  { id: "2-6", section: "features" },
  { id: "2-7", section: "features" },
  { id: "2-8", section: "features" },
  /* FAQ */
  { id: "3-1", section: "faq" },
  { id: "3-2", section: "faq" },
  { id: "3-3", section: "faq" },
  { id: "3-4", section: "faq" },
  { id: "3-5", section: "faq" },
  { id: "3-6", section: "faq" },
  { id: "3-7", section: "faq" },
  { id: "3-8", section: "faq" },
  { id: "3-9", section: "faq" },
  { id: "3-10", section: "faq" },
  { id: "3-11", section: "faq" },
  { id: "3-12", section: "faq" },
  { id: "3-13", section: "faq" },
  { id: "3-14", section: "faq" },
  { id: "3-15", section: "faq" },
  /* Troubleshooting */
  { id: "4-1", section: "troubleshooting" },
  { id: "4-2", section: "troubleshooting" },
  { id: "4-3", section: "troubleshooting" },
  { id: "4-4", section: "troubleshooting" },
  { id: "4-5", section: "troubleshooting" },
];

/* ------------------------------------------------------------------ */
/*  Simple markdown-like renderer for bold text                       */
/* ------------------------------------------------------------------ */

function renderContent(text: string) {
  // Split into paragraphs
  const paragraphs = text.split("\n\n");

  return paragraphs.map((para, i) => {
    const trimmed = para.trim();
    if (!trimmed) return null;

    // Check if it's a numbered list
    if (/^\d+\.\s/.test(trimmed)) {
      const items = trimmed.split(/\n/).filter((l) => l.trim());
      return (
        <ol key={i} className="list-decimal space-y-1.5 pl-6 text-stone-700">
          {items.map((item, j) => (
            <li key={j} className="leading-relaxed">
              <span
                dangerouslySetInnerHTML={{
                  __html: item
                    .replace(/^\d+\.\s*/, "")
                    .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
                }}
              />
            </li>
          ))}
        </ol>
      );
    }

    // Check if it's a bulleted list
    if (/^[-*]\s/.test(trimmed) || /^\s+[-*]\s/.test(trimmed)) {
      const items = trimmed.split(/\n/).filter((l) => l.trim());
      return (
        <ul key={i} className="list-disc space-y-1.5 pl-6 text-stone-700">
          {items.map((item, j) => (
            <li key={j} className="leading-relaxed">
              <span
                dangerouslySetInnerHTML={{
                  __html: item
                    .replace(/^\s*[-*]\s*/, "")
                    .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
                }}
              />
            </li>
          ))}
        </ul>
      );
    }

    // Bold header lines (e.g., "**Steps:**")
    if (/^\*\*.*\*\*$/.test(trimmed) || /^\*\*.*:\*\*$/.test(trimmed)) {
      return (
        <h4
          key={i}
          className="mt-2 text-sm font-semibold text-stone-900"
          dangerouslySetInnerHTML={{
            __html: trimmed.replace(/\*\*(.*?)\*\*/g, "$1"),
          }}
        />
      );
    }

    // Regular paragraph
    return (
      <p
        key={i}
        className="leading-relaxed text-stone-700"
        dangerouslySetInnerHTML={{
          __html: trimmed.replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
        }}
      />
    );
  });
}

/* ------------------------------------------------------------------ */
/*  Help Center Page Component                                        */
/* ------------------------------------------------------------------ */

export default function HelpCenterPage() {
  const t = useTranslations("help");
  const [searchQuery, setSearchQuery] = useState("");
  const [activeSection, setActiveSection] = useState<string | null>(null);
  const [activeArticleId, setActiveArticleId] = useState<string | null>(null);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  // Build sections array with translated labels
  const SECTIONS = useMemo(() => SECTION_KEYS.map((key) => ({
    key,
    label: t(`sections.${key.replace("-", "_")}`),
    icon: SECTION_ICONS[key],
  })), [t]);

  // Build articles from translations
  const ARTICLES: Article[] = useMemo(() => ARTICLE_IDS.map((meta) => ({
    id: meta.id,
    section: meta.section,
    title: t(`articles.a${meta.id.replace("-", "_")}.title`),
    content: t(`articles.a${meta.id.replace("-", "_")}.content`),
  })), [t]);

  // Filter articles based on search
  const filteredArticles = useMemo(() => {
    if (!searchQuery.trim()) return ARTICLES;
    const q = searchQuery.toLowerCase();
    return ARTICLES.filter(
      (a) =>
        a.title.toLowerCase().includes(q) ||
        a.content.toLowerCase().includes(q)
    );
  }, [searchQuery, ARTICLES]);

  // Group filtered articles by section
  const groupedArticles = useMemo(() => {
    const groups: Record<string, Article[]> = {};
    for (const article of filteredArticles) {
      if (!groups[article.section]) groups[article.section] = [];
      groups[article.section].push(article);
    }
    return groups;
  }, [filteredArticles]);

  // Currently selected article
  const activeArticle = activeArticleId
    ? ARTICLES.find((a) => a.id === activeArticleId) ?? null
    : null;

  // Visible sections (either filtered or all)
  const visibleSections = SECTIONS.filter(
    (s) => groupedArticles[s.key] && groupedArticles[s.key].length > 0
  );

  function handleArticleClick(articleId: string) {
    setActiveArticleId(articleId);
    setMobileNavOpen(false);
  }

  function handleSectionClick(sectionKey: string) {
    setActiveSection(activeSection === sectionKey ? null : sectionKey);
    setActiveArticleId(null);
  }

  function handleBackToList() {
    setActiveArticleId(null);
  }

  /* -- Sidebar content (shared between desktop and mobile) -- */
  const sidebarContent = (
    <nav className="space-y-1" data-testid="help-sidebar-nav">
      {visibleSections.map((section) => {
        const Icon = section.icon;
        const isExpanded =
          activeSection === section.key ||
          !!activeArticle?.section ||
          searchQuery.trim().length > 0;
        const sectionArticles = groupedArticles[section.key] || [];

        return (
          <div key={section.key}>
            <button
              onClick={() => handleSectionClick(section.key)}
              className={`flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                activeSection === section.key ||
                activeArticle?.section === section.key
                  ? "bg-primary/5 text-primary"
                  : "text-stone-600 hover:bg-stone-50 hover:text-stone-900"
              }`}
              data-testid={`help-section-${section.key}`}
            >
              <Icon className="h-4 w-4 shrink-0" />
              <span className="flex-1 text-left">{section.label}</span>
              <ChevronDown
                className={`h-3.5 w-3.5 shrink-0 transition-transform ${
                  isExpanded && (activeSection === section.key || searchQuery)
                    ? "rotate-180"
                    : ""
                }`}
              />
            </button>
            {(activeSection === section.key || searchQuery.trim().length > 0) &&
              sectionArticles.length > 0 && (
                <div className="ml-4 mt-1 space-y-0.5 border-l border-stone-200 pl-3">
                  {sectionArticles.map((article) => (
                    <button
                      key={article.id}
                      onClick={() => handleArticleClick(article.id)}
                      className={`block w-full rounded-md px-2.5 py-1.5 text-left text-sm transition-colors ${
                        activeArticleId === article.id
                          ? "bg-primary/5 font-medium text-primary"
                          : "text-stone-500 hover:bg-stone-50 hover:text-stone-700"
                      }`}
                      data-testid={`help-article-${article.id}`}
                    >
                      {article.title}
                    </button>
                  ))}
                </div>
              )}
          </div>
        );
      })}

      {visibleSections.length === 0 && searchQuery.trim().length > 0 && (
        <p className="px-3 py-4 text-sm text-muted-foreground" data-testid="help-no-results">
          {t("no_results_for", { query: searchQuery })}
        </p>
      )}
    </nav>
  );

  return (
    <div className="min-h-screen bg-white">
      {/* -- Header -- */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href="/"
            className="text-xl font-bold tracking-tight text-primary"
            data-testid="help-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href="/"
              className="text-sm font-medium text-stone-600 transition-colors hover:text-primary"
              data-testid="help-nav-home"
            >
              {t("nav_home")}
            </Link>
            <span
              className="text-sm font-medium text-primary"
              data-testid="help-nav-help"
            >
              {t("nav_help_center")}
            </span>
          </div>
        </nav>
      </header>

      {/* -- Hero -- */}
      <section className="border-b border-stone-100 bg-gradient-to-br from-primary/5 via-white to-teal-50 py-12 sm:py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center">
            <h1
              className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl"
              data-testid="help-title"
            >
              {t("title")}
            </h1>
            <p className="mt-3 text-lg text-stone-600">
              {t("subtitle")}
            </p>
            <div className="relative mx-auto mt-6 max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                type="search"
                placeholder={t("search_placeholder")}
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value);
                  setActiveArticleId(null);
                }}
                className="pl-10"
                data-testid="help-search"
              />
            </div>
          </div>
        </div>
      </section>

      {/* -- Main Content -- */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
        <div className="flex gap-8">
          {/* -- Sidebar (desktop) -- */}
          <aside
            className="hidden w-64 shrink-0 lg:block"
            data-testid="help-sidebar"
          >
            <div className="sticky top-24">{sidebarContent}</div>
          </aside>

          {/* -- Mobile navigation dropdown -- */}
          <div className="mb-4 w-full lg:hidden">
            <Button
              variant="outline"
              className="flex w-full items-center justify-between"
              onClick={() => setMobileNavOpen(!mobileNavOpen)}
              data-testid="help-mobile-nav-toggle"
            >
              <span className="text-sm font-medium">
                {activeArticle
                  ? activeArticle.title
                  : t("browse_articles")}
              </span>
              <ChevronDown
                className={`h-4 w-4 transition-transform ${
                  mobileNavOpen ? "rotate-180" : ""
                }`}
              />
            </Button>
            {mobileNavOpen && (
              <div className="mt-2 rounded-lg border border-stone-200 bg-white p-3 shadow-lg">
                {sidebarContent}
              </div>
            )}
          </div>

          {/* -- Content Area -- */}
          <main className="min-w-0 flex-1" data-testid="help-content">
            {activeArticle ? (
              /* -- Single article view -- */
              <article data-testid="help-article-detail">
                <button
                  onClick={handleBackToList}
                  className="mb-4 flex items-center gap-1.5 text-sm font-medium text-primary transition-colors hover:text-primary/90"
                  data-testid="help-back-btn"
                >
                  <ArrowLeft className="h-3.5 w-3.5" />
                  {t("back_to_articles")}
                </button>
                <div className="mb-2">
                  <span className="inline-block rounded-full bg-primary/5 px-2.5 py-0.5 text-xs font-medium text-primary">
                    {SECTIONS.find((s) => s.key === activeArticle.section)
                      ?.label ?? activeArticle.section}
                  </span>
                </div>
                <h2 className="text-2xl font-bold text-stone-900 sm:text-3xl">
                  {activeArticle.title}
                </h2>
                <div className="mt-6 space-y-4 text-sm sm:text-base">
                  {renderContent(activeArticle.content)}
                </div>
              </article>
            ) : (
              /* -- Article list view -- */
              <div data-testid="help-article-list">
                {visibleSections.map((section) => {
                  const Icon = section.icon;
                  const sectionArticles =
                    groupedArticles[section.key] || [];
                  return (
                    <div key={section.key} className="mb-10">
                      <div className="mb-4 flex items-center gap-2">
                        <Icon className="h-5 w-5 text-primary" />
                        <h2 className="text-lg font-semibold text-stone-900">
                          {section.label}
                        </h2>
                        <span className="rounded-full bg-stone-100 px-2 py-0.5 text-xs font-medium text-stone-500">
                          {sectionArticles.length}
                        </span>
                      </div>
                      <div className="grid gap-3 sm:grid-cols-2">
                        {sectionArticles.map((article) => (
                          <button
                            key={article.id}
                            onClick={() => handleArticleClick(article.id)}
                            className="group flex items-start gap-3 rounded-xl border border-stone-100 bg-white p-4 text-left transition-all hover:border-primary/20 hover:shadow-sm"
                            data-testid={`help-card-${article.id}`}
                          >
                            <div className="flex-1">
                              <h3 className="text-sm font-medium text-stone-900 group-hover:text-primary">
                                {article.title}
                              </h3>
                              <p className="mt-1 line-clamp-2 text-xs text-stone-500">
                                {article.content.slice(0, 120)}...
                              </p>
                            </div>
                            <ExternalLink className="mt-0.5 h-3.5 w-3.5 shrink-0 text-stone-300 group-hover:text-primary" />
                          </button>
                        ))}
                      </div>
                    </div>
                  );
                })}

                {visibleSections.length === 0 && (
                  <div className="py-16 text-center" data-testid="help-empty-state">
                    <HelpCircle className="mx-auto h-12 w-12 text-stone-300" />
                    <h3 className="mt-4 text-lg font-medium text-stone-700">
                      {t("no_articles_found")}
                    </h3>
                    <p className="mt-2 text-sm text-stone-500">
                      {t("try_different_search")}
                    </p>
                  </div>
                )}
              </div>
            )}

            {/* -- Contact support banner -- */}
            <div className="mt-12 rounded-xl border border-stone-200 bg-stone-50 p-6 text-center" data-testid="help-contact-banner">
              <h3 className="text-base font-semibold text-stone-900">
                {t("still_need_help")}
              </h3>
              <p className="mt-1 text-sm text-stone-600">
                {t("support_hours")}
              </p>
              <div className="mt-4 flex flex-col items-center gap-3 sm:flex-row sm:justify-center">
                <a
                  href="mailto:support@vetara.ae"
                  className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-primary/90"
                  data-testid="help-contact-email"
                >
                  {t("email_support")}
                </a>
                <a
                  href="https://wa.me/"
                  className="inline-flex items-center gap-1.5 rounded-lg border border-stone-200 bg-white px-4 py-2 text-sm font-medium text-stone-700 transition-colors hover:bg-stone-50"
                  data-testid="help-contact-whatsapp"
                >
                  {t("whatsapp_support")}
                </a>
              </div>
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
