"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { Settings, Users, MessageSquare, type LucideIcon } from "lucide-react"
import { Card, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"

interface SettingsLink {
  href: string
  icon: LucideIcon
  titleKey: string
  descriptionKey: string
  testId: string
}

const settingsLinks: SettingsLink[] = [
  {
    href: "settings/preferences",
    icon: Settings,
    titleKey: "preferences_title",
    descriptionKey: "preferences_description",
    testId: "settings-link-preferences",
  },
  {
    href: "settings/team",
    icon: Users,
    titleKey: "team_title",
    descriptionKey: "team_description",
    testId: "settings-link-team",
  },
  {
    href: "settings/messaging",
    icon: MessageSquare,
    titleKey: "messaging_title",
    descriptionKey: "messaging_description",
    testId: "settings-link-messaging",
  },
]

export default function SettingsPageClient() {
  const pathname = usePathname()
  const t = useTranslations("settings_page")
  // Extract locale prefix from current path (e.g. "/en/settings" -> "/en")
  const localePrefix = pathname.replace(/\/settings$/, "")

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="settings-index-page">
      <div>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {t("title")}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">
          {t("subtitle")}
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {settingsLinks.map((link) => (
          <Link
            key={link.href}
            href={`${localePrefix}/${link.href}`}
            className="block group"
            data-testid={link.testId}
          >
            <Card className="h-full bg-white border-border/80 rounded-xl shadow-sm transition-all duration-200 hover:shadow-md hover:border-[#303ef5]/30 cursor-pointer">
              <CardHeader>
                <div className="flex items-start gap-4">
                  <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-[#eef2fd] group-hover:bg-[#303ef5]/15 transition-colors">
                    <link.icon className="h-5 w-5 text-[#303ef5]" />
                  </div>
                  <div>
                    <CardTitle className="text-[15px] font-bold text-[#061e44]">{t(link.titleKey)}</CardTitle>
                    <CardDescription className="mt-1 text-[13px] text-muted-foreground">
                      {t(link.descriptionKey)}
                    </CardDescription>
                  </div>
                </div>
              </CardHeader>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  )
}
