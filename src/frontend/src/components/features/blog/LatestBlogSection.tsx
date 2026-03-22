import Link from "next/link";
import { ArrowRight } from "lucide-react";
import { getArticlesSorted } from "@/lib/blog";
import { BlogArticleCard } from "./BlogArticleCard";

interface LatestBlogSectionProps {
  locale: string;
}

export function LatestBlogSection({ locale }: LatestBlogSectionProps) {
  const latestArticles = getArticlesSorted().slice(0, 3);

  return (
    <section
      data-testid="section-latest-blog"
      className="bg-stone-50 py-20 sm:py-28"
    >
      <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-2xl font-bold tracking-tight text-stone-900 sm:text-3xl lg:text-4xl">
            Latest from the Blog
          </h2>
          <p className="mt-4 text-lg text-stone-600">
            Expert insights on veterinary clinic management, technology, and
            the UAE market.
          </p>
        </div>

        <div className="mt-12 grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
          {latestArticles.map((article) => (
            <BlogArticleCard
              key={article.slug}
              article={article}
              locale={locale}
            />
          ))}
        </div>

        <div className="mt-10 text-center">
          <Link
            href={`/${locale}/blog`}
            className="inline-flex items-center gap-1.5 text-sm font-semibold text-primary hover:text-primary/90 transition-colors"
            data-testid="landing-blog-view-all"
          >
            View all articles
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      </div>
    </section>
  );
}
