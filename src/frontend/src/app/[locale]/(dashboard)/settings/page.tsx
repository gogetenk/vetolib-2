"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { Settings, Users, MessageSquare } from "lucide-react"
import { Card, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"

const settingsLinks = [
  {
    href: "settings/preferences",
    icon: Settings,
    title: "Clinic Preferences",
    description: "Configure clinic hours, default consultation duration, and notification settings.",
    testId: "settings-link-preferences",
  },
  {
    href: "settings/team",
    icon: Users,
    title: "Team Management",
    description: "Invite team members, manage roles, and control access permissions.",
    testId: "settings-link-team",
  },
  {
    href: "settings/messaging",
    icon: MessageSquare,
    title: "Messaging Settings",
    description: "Set up automated reminders, templates, and messaging hours.",
    testId: "settings-link-messaging",
  },
]

export default function SettingsIndexPage() {
  const pathname = usePathname()
  // Extract locale prefix from current path (e.g. "/en/settings" -> "/en")
  const localePrefix = pathname.replace(/\/settings$/, "")

  return (
    <div className="space-y-6" data-testid="settings-index-page">
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Manage your clinic configuration and preferences.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {settingsLinks.map((link) => (
          <Link
            key={link.href}
            href={`${localePrefix}/${link.href}`}
            className="block"
            data-testid={link.testId}
          >
            <Card className="h-full transition-colors hover:bg-muted/50 cursor-pointer">
              <CardHeader>
                <div className="flex items-center gap-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/10">
                    <link.icon className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <CardTitle className="text-base">{link.title}</CardTitle>
                    <CardDescription className="mt-1 text-sm">
                      {link.description}
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
