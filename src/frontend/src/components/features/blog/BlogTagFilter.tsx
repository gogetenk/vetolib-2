"use client";

import Link from "next/link";

interface BlogTagFilterProps {
  tags: string[];
  activeTag?: string;
  locale: string;
}

export function BlogTagFilter({ tags, activeTag, locale }: BlogTagFilterProps) {
  return (
    <div
      className="flex flex-wrap items-center gap-2"
      data-testid="blog-tag-filter"
    >
      <Link
        href={`/${locale}/blog`}
        className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
          !activeTag
            ? "bg-emerald-700 text-white"
            : "bg-stone-100 text-stone-600 hover:bg-stone-200"
        }`}
        data-testid="blog-tag-all"
      >
        All
      </Link>
      {tags.map((tag) => (
        <Link
          key={tag}
          href={`/${locale}/blog?tag=${encodeURIComponent(tag)}`}
          className={`rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
            activeTag === tag
              ? "bg-emerald-700 text-white"
              : "bg-stone-100 text-stone-600 hover:bg-stone-200"
          }`}
          data-testid={`blog-tag-${tag.toLowerCase().replace(/\s+/g, "-")}`}
        >
          {tag}
        </Link>
      ))}
    </div>
  );
}
