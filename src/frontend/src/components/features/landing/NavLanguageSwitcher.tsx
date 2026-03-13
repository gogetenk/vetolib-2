import Link from "next/link";

interface Props {
  locale: string;
}

export function NavLanguageSwitcher({ locale }: Props) {
  return (
    <div
      data-testid="nav-language-switcher"
      className="relative flex items-center gap-1 rounded-full border border-gray-200 bg-white p-1"
    >
      {/* Sliding background indicator */}
      <div
        className={`absolute top-1 h-[calc(100%-8px)] w-[calc(50%-4px)] rounded-full bg-emerald-50 transition-all duration-300 ease-out ${
          locale === "ar" ? "translate-x-[calc(100%+2px)]" : "translate-x-0"
        }`}
        aria-hidden="true"
      />
      <Link
        href="/en"
        data-testid="nav-lang-en"
        className={`relative z-10 rounded-full px-2.5 py-1 text-xs font-medium transition-colors duration-300 ${
          locale === "en"
            ? "text-emerald-700"
            : "text-gray-400 hover:text-emerald-700"
        }`}
      >
        EN
      </Link>
      <Link
        href="/ar"
        data-testid="nav-lang-ar"
        className={`relative z-10 rounded-full px-2.5 py-1 text-xs font-medium transition-colors duration-300 ${
          locale === "ar"
            ? "text-emerald-700"
            : "text-gray-400 hover:text-emerald-700"
        }`}
      >
        AR
      </Link>
    </div>
  );
}
