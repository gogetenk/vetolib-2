import Link from "next/link";

interface Props {
  locale: string;
}

export function NavLanguageSwitcher({ locale }: Props) {
  return (
    <div
      data-testid="nav-language-switcher"
      className="flex items-center gap-2 rounded-full border border-gray-200 bg-white px-3 py-1.5"
    >
      <Link
        href="/en"
        data-testid="nav-lang-en"
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
        href="/ar"
        data-testid="nav-lang-ar"
        className={`text-xs font-medium transition-colors ${
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
