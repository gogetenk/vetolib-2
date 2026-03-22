import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { CalendarDays, Clock, ArrowLeft } from "lucide-react";
import {
  getArticleBySlug,
  getArticlesSorted,
  getRelatedArticles,
} from "@/lib/blog";
import { BlogArticleCard } from "@/components/features/blog/BlogArticleCard";
import { BlogTableOfContents } from "@/components/features/blog/BlogTableOfContents";

interface Props {
  params: Promise<{ locale: string; slug: string }>;
}

export async function generateStaticParams() {
  const articles = getArticlesSorted();
  return articles.map((article) => ({ slug: article.slug }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug } = await params;
  const article = getArticleBySlug(slug);
  if (!article) return {};

  const canonicalUrl = `https://vetara.com/en/blog/${article.slug}`;

  return {
    title: article.metaTitle,
    description: article.metaDescription,
    alternates: {
      canonical: canonicalUrl,
    },
    openGraph: {
      title: article.metaTitle,
      description: article.metaDescription,
      type: "article",
      publishedTime: article.date,
      authors: [article.author.name],
      tags: article.tags,
      images: [
        {
          url: article.featuredImage,
          width: 1200,
          height: 630,
          alt: article.title,
        },
      ],
    },
    twitter: {
      card: "summary_large_image",
      title: article.metaTitle,
      description: article.metaDescription,
      images: [article.featuredImage],
    },
  };
}

export default async function BlogArticlePage({ params }: Props) {
  const { locale, slug } = await params;
  const article = getArticleBySlug(slug);

  if (!article) {
    notFound();
  }

  const relatedArticles = getRelatedArticles(article);
  const canonicalUrl = `https://vetara.com/en/blog/${article.slug}`;

  const jsonLd = {
    "@context": "https://schema.org",
    "@type": "BlogPosting",
    headline: article.title,
    description: article.metaDescription,
    image: `https://vetara.com${article.featuredImage}`,
    datePublished: article.date,
    author: {
      "@type": "Person",
      name: article.author.name,
      jobTitle: article.author.role,
    },
    publisher: {
      "@type": "Organization",
      name: "Vetara",
      logo: {
        "@type": "ImageObject",
        url: "https://vetara.com/logo.png",
      },
    },
    mainEntityOfPage: {
      "@type": "WebPage",
      "@id": canonicalUrl,
    },
    keywords: article.tags.join(", "),
  };

  return (
    <div className="min-h-screen bg-white">
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }}
      />

      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href={`/${locale}`}
            className="text-xl font-bold tracking-tight text-emerald-700"
            data-testid="article-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href={`/${locale}`}
              className="text-sm font-medium text-stone-600 hover:text-emerald-700 transition-colors"
              data-testid="article-nav-home"
            >
              Home
            </Link>
            <Link
              href={`/${locale}/blog`}
              className="text-sm font-medium text-stone-600 hover:text-emerald-700 transition-colors"
              data-testid="article-nav-blog"
            >
              Blog
            </Link>
          </div>
        </nav>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
        <Link
          href={`/${locale}/blog`}
          className="mb-8 inline-flex items-center gap-1.5 text-sm font-medium text-emerald-700 hover:text-emerald-800 transition-colors"
          data-testid="article-back-link"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Blog
        </Link>

        <div className="lg:grid lg:grid-cols-[1fr_280px] lg:gap-12">
          <article className="mx-auto max-w-[720px] lg:mx-0" data-testid="article-content">
            <header className="mb-8">
              <div className="flex flex-wrap gap-2 mb-4" data-testid="article-tags">
                {article.tags.map((tag) => (
                  <Link
                    key={tag}
                    href={`/${locale}/blog?tag=${encodeURIComponent(tag)}`}
                    className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-medium text-emerald-700 hover:bg-emerald-100 transition-colors"
                    data-testid={`article-tag-${tag.toLowerCase().replace(/\s+/g, "-")}`}
                  >
                    {tag}
                  </Link>
                ))}
              </div>
              <h1
                className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl"
                data-testid="article-title"
              >
                {article.title}
              </h1>
              <div
                className="mt-4 flex flex-wrap items-center gap-4 text-sm text-stone-500"
                data-testid="article-meta"
              >
                <span className="flex items-center gap-1.5">
                  <CalendarDays className="h-4 w-4" />
                  {new Date(article.date).toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "long",
                    day: "numeric",
                  })}
                </span>
                <span className="flex items-center gap-1.5">
                  <Clock className="h-4 w-4" />
                  {article.readingTime}
                </span>
              </div>
            </header>

            <div
              className="mb-10 aspect-[16/9] w-full rounded-xl bg-gradient-to-br from-emerald-100 to-teal-50 flex items-center justify-center"
              data-testid="article-featured-image"
            >
              <span className="text-lg font-medium text-emerald-700/50">
                Featured Image
              </span>
            </div>

            <div
              className="prose prose-stone prose-lg max-w-none prose-headings:scroll-mt-20 prose-headings:font-bold prose-h2:text-2xl prose-h2:mt-10 prose-h2:mb-4 prose-h3:text-xl prose-h3:mt-8 prose-h3:mb-3 prose-p:leading-relaxed prose-a:text-emerald-700 prose-a:no-underline hover:prose-a:underline prose-strong:text-stone-900 prose-table:text-sm prose-th:bg-stone-50 prose-th:p-3 prose-td:p-3"
              dangerouslySetInnerHTML={{ __html: article.content }}
              data-testid="article-body"
            />

            <div
              className="mt-12 flex items-center gap-4 rounded-xl border border-stone-100 bg-stone-50 p-6"
              data-testid="article-author"
            >
              <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-emerald-100 text-lg font-bold text-emerald-700">
                {article.author.name
                  .split(" ")
                  .map((n) => n[0])
                  .join("")}
              </div>
              <div>
                <p className="font-semibold text-stone-900">
                  {article.author.name}
                </p>
                <p className="text-sm text-stone-500">{article.author.role}</p>
              </div>
            </div>
          </article>

          <aside className="hidden lg:block" data-testid="article-sidebar">
            <div className="sticky top-24">
              <BlogTableOfContents content={article.content} />
            </div>
          </aside>
        </div>

        {relatedArticles.length > 0 && (
          <section
            className="mt-16 border-t border-stone-100 pt-12"
            data-testid="article-related"
          >
            <h2 className="text-2xl font-bold text-stone-900">
              Related Articles
            </h2>
            <div className="mt-8 grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
              {relatedArticles.map((related) => (
                <BlogArticleCard
                  key={related.slug}
                  article={related}
                  locale={locale}
                />
              ))}
            </div>
          </section>
        )}
      </main>
    </div>
  );
}
