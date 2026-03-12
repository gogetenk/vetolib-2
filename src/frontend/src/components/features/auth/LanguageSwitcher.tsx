"use client"

import Link from "next/link"
import { useLocale } from "next-intl"
import { usePathname } from "next/navigation"

interface LanguageSwitcherProps {
  className?: string
}

export function LanguageSwitcher({ className }: LanguageSwitcherProps) {
  const locale = useLocale()
  const pathname = usePathname()

  // Replace the current locale segment in the pathname
  function getLocalePath(targetLocale: string) {
    const segments = pathname.split("/")
    if (segments.length > 1 && (segments[1] === "en" || segments[1] === "ar")) {
      segments[1] = targetLocale
    }
    return segments.join("/") || `/${targetLocale}`
  }

  return (
    <div
      data-testid="language-switcher"
      className={`flex items-center gap-2 rounded-full border border-gray-200 bg-white px-3 py-1.5 ${className ?? ""}`}
    >
      <Link
        href={getLocalePath("en")}
        data-testid="lang-switch-en"
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
        href={getLocalePath("ar")}
        data-testid="lang-switch-ar"
        className={`text-xs font-medium transition-colors ${
          locale === "ar"
            ? "text-emerald-700"
            : "text-gray-400 hover:text-emerald-700"
        }`}
      >
        AR
      </Link>
    </div>
  )
}
