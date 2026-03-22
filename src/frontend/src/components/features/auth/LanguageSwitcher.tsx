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
    if (segments.length > 1 && (segments[1] === "en" || segments[1] === "ar" || segments[1] === "fr")) {
      segments[1] = targetLocale
    }
    return segments.join("/") || `/${targetLocale}`
  }

  return (
    <div
      data-testid="language-switcher"
      className={`flex items-center gap-2 rounded-full border border-stone-200 bg-white px-3 py-1.5 shadow-sm transition-shadow duration-200 hover:shadow-md ${className ?? ""}`}
    >
      <Link
        href={getLocalePath("en")}
        data-testid="lang-switch-en"
        className={`text-xs font-medium transition-all duration-200 ${
          locale === "en"
            ? "text-primary scale-105"
            : "text-stone-400 hover:text-primary hover:scale-105"
        }`}
      >
        EN
      </Link>
      <span className="text-stone-200">|</span>
      <Link
        href={getLocalePath("fr")}
        data-testid="lang-switch-fr"
        className={`text-xs font-medium transition-all duration-200 ${
          locale === "fr"
            ? "text-primary scale-105"
            : "text-stone-400 hover:text-primary hover:scale-105"
        }`}
      >
        FR
      </Link>
      <span className="text-stone-200">|</span>
      <Link
        href={getLocalePath("ar")}
        data-testid="lang-switch-ar"
        className={`text-xs font-medium transition-all duration-200 ${
          locale === "ar"
            ? "text-primary scale-105"
            : "text-stone-400 hover:text-primary hover:scale-105"
        }`}
      >
        AR
      </Link>
    </div>
  )
}
