"use client";

import { useMemo } from "react";

interface TocItem {
  id: string;
  text: string;
  level: number;
}

interface BlogTableOfContentsProps {
  content: string;
}

export function BlogTableOfContents({ content }: BlogTableOfContentsProps) {
  const headings = useMemo(() => {
    const items: TocItem[] = [];
    const regex = /<h([23])\s+id="([^"]+)"[^>]*>([^<]+)<\/h[23]>/g;
    let match;
    while ((match = regex.exec(content)) !== null) {
      items.push({
        level: parseInt(match[1], 10),
        id: match[2],
        text: match[3],
      });
    }
    return items;
  }, [content]);

  if (headings.length === 0) return null;

  return (
    <nav
      className="rounded-xl border border-stone-100 bg-stone-50 p-5"
      data-testid="article-toc"
      aria-label="Table of contents"
    >
      <h3 className="mb-3 text-sm font-semibold text-stone-900">
        Table of Contents
      </h3>
      <ul className="space-y-2">
        {headings.map((heading) => (
          <li
            key={heading.id}
            className={heading.level === 3 ? "ml-4" : ""}
          >
            <a
              href={`#${heading.id}`}
              className="block text-sm text-stone-500 hover:text-emerald-700 transition-colors leading-snug"
              data-testid={`toc-link-${heading.id}`}
            >
              {heading.text}
            </a>
          </li>
        ))}
      </ul>
    </nav>
  );
}
