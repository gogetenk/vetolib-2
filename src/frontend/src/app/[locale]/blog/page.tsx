import type { Metadata } from "next";
import Link from "next/link";
import { getArticlesSorted, getAllTags } from "@/lib/blog";
import { BlogTagFilter } from "@/components/features/blog/BlogTagFilter";
import { BlogArticleCard } from "@/components/features/blog/BlogArticleCard";

export const metadata: Metadata = {
  title: "Blog",
  description:
    "Insights, guides, and best practices for veterinary clinic management in the UAE. Covering AI triage, WhatsApp integration, Arabic software, and more.",
  openGraph: {
    title: "Blog -- Vetara",
    description:
      "Insights, guides, and best practices for veterinary clinic management in the UAE.",
    type: "website",
  },
};

interface Props {
  params: Promise<{ locale: string }>;
  searchParams: Promise<{ tag?: string }>;
}

export default async function BlogListPage({ params, searchParams }: Props) {
  const { locale } = await params;
  const { tag } = await searchParams;
  const articles = getArticlesSorted();
  const allTags = getAllTags();

  const filteredArticles = tag
    ? articles.filter((a) => a.tags.includes(tag))
    : articles;

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-xl font-bold tracking-tight text-emerald-700"
            data-testid="blog-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href={`/${locale}`}
              className="text-sm font-medium text-stone-600 hover:text-emerald-700 transition-colors"
              data-testid="blog-nav-home"
            >
              Home
            </Link>
            <Link
              href={`/${locale}/blog`}
              className="text-sm font-medium text-emerald-700"
              data-testid="blog-nav-blog"
            >
              Blog
            </Link>
          </div>
        </nav>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
        <div className="mx-auto max-w-2xl text-center" data-testid="blog-header">
          <h1 className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl">
            Vetara Blog
          </h1>
          <p className="mt-4 text-lg text-stone-600">
            Insights, guides, and best practices for veterinary clinic
            management in the UAE.
          </p>
        </div>

        <div className="mt-10" data-testid="blog-tag-filter-section">
          <BlogTagFilter tags={allTags} activeTag={tag} locale={locale} />
        </div>

        <div
          className="mt-10 grid gap-8 sm:grid-cols-2 lg:grid-cols-3"
          data-testid="blog-articles-grid"
        >
          {filteredArticles.map((article) => (
            <BlogArticleCard
              key={article.slug}
              article={article}
              locale={locale}
            />
          ))}
        </div>

        {filteredArticles.length === 0 && (
          <p
            className="mt-12 text-center text-stone-500"
            data-testid="blog-no-articles"
          >
            No articles found for this tag.
          </p>
        )}
      </main>
    </div>
  );
}
