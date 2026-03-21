import Link from "next/link";
import { CalendarDays, Clock } from "lucide-react";
import type { BlogArticle } from "@/lib/blog";

interface BlogArticleCardProps {
  article: BlogArticle;
  locale: string;
}

export function BlogArticleCard({ article, locale }: BlogArticleCardProps) {
  return (
    <Link
      href={`/${locale}/blog/${article.slug}`}
      className="group flex flex-col overflow-hidden rounded-xl border border-stone-100 bg-white shadow-sm transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
      data-testid={`blog-card-${article.slug}`}
    >
      {/* Image placeholder */}
      <div className="aspect-[16/9] w-full bg-gradient-to-br from-emerald-100 to-teal-50 flex items-center justify-center">
        <span className="text-sm font-medium text-emerald-700/40">
          {article.title.slice(0, 30)}...
        </span>
      </div>

      <div className="flex flex-1 flex-col p-5">
        {/* Tags */}
        <div className="flex flex-wrap gap-1.5 mb-3" data-testid={`blog-card-tags-${article.slug}`}>
          {article.tags.slice(0, 2).map((tag) => (
            <span
              key={tag}
              className="rounded-full bg-emerald-50 px-2.5 py-0.5 text-xs font-medium text-emerald-700"
            >
              {tag}
            </span>
          ))}
        </div>

        {/* Title */}
        <h2
          className="text-lg font-semibold text-stone-900 group-hover:text-emerald-700 transition-colors line-clamp-2"
          data-testid={`blog-card-title-${article.slug}`}
        >
          {article.title}
        </h2>

        {/* Excerpt */}
        <p
          className="mt-2 flex-1 text-sm leading-relaxed text-stone-600 line-clamp-3"
          data-testid={`blog-card-excerpt-${article.slug}`}
        >
          {article.excerpt}
        </p>

        {/* Meta */}
        <div
          className="mt-4 flex items-center gap-3 text-xs text-stone-400"
          data-testid={`blog-card-meta-${article.slug}`}
        >
          <span className="flex items-center gap-1">
            <CalendarDays className="h-3.5 w-3.5" />
            {new Date(article.date).toLocaleDateString("en-US", {
              month: "short",
              day: "numeric",
              year: "numeric",
            })}
          </span>
          <span className="flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" />
            {article.readingTime}
          </span>
        </div>
      </div>
    </Link>
  );
}
