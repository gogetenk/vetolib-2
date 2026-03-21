"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { cn } from "@/lib/utils"

export default function MessagingSettingsLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const t = useTranslations("messaging_admin.nav")
  const pathname = usePathname()

  const tabs = [
    {
      href: "templates",
      label: t("templates"),
      testId: "nav-messaging-templates",
    },
    {
      href: "hours",
      label: t("hours"),
      testId: "nav-messaging-hours",
    },
    {
      href: "stats",
      label: t("stats"),
      testId: "nav-messaging-stats",
    },
    {
      href: "whatsapp",
      label: t("whatsapp"),
      testId: "nav-messaging-whatsapp",
    },
  ]

  return (
    <div className="space-y-6" data-testid="messaging-settings-layout">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">{t("title")}</h1>
        <p className="text-sm text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      {/* Sub-navigation tabs */}
      <nav
        className="flex gap-1 border-b"
        data-testid="messaging-settings-subnav"
        aria-label="Messaging settings navigation"
      >
        {tabs.map((tab) => {
          const isActive = pathname.endsWith(tab.href)
          return (
            <Link
              key={tab.href}
              href={tab.href}
              data-testid={tab.testId}
              className={cn(
                "px-4 py-2 text-sm font-medium border-b-2 -mb-px transition-colors",
                isActive
                  ? "border-primary text-primary"
                  : "border-transparent text-muted-foreground hover:text-foreground hover:border-border"
              )}
            >
              {tab.label}
            </Link>
          )
        })}
      </nav>

      <div>{children}</div>
    </div>
  )
}
