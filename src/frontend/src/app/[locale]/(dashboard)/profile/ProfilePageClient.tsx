"use client"

import { useMemo } from "react"
import { User, Mail, Shield } from "lucide-react"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { PageContainer } from "@/components/ui/page-container"
import { getStoredUser } from "@/lib/api/auth"
import { useRole } from "@/hooks/use-role"
import { useTranslations } from "next-intl"

interface ProfileInfo {
  name: string
  email: string
  clinicName: string
}

export default function ProfilePageClient() {
  const role = useRole()
  const t = useTranslations("profile")
  const profile = useMemo<ProfileInfo | null>(() => {
    const user = getStoredUser()
    if (user) {
      return {
        name: user.name,
        email: user.email,
        clinicName: user.clinicName,
      }
    }
    return null
  }, [])

  return (
    <PageContainer data-testid="profile-page">
      <div>
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">
          {t("subtitle")}
        </p>
      </div>

      <Card className="border-border/80 shadow-sm" data-testid="profile-card">
        <CardHeader>
          <CardTitle className="text-[15px] font-bold text-foreground">{t("account_details")}</CardTitle>
          <CardDescription className="text-[13px] text-muted-foreground">{t("account_description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-5">
          {profile ? (
            <>
              <div className="flex items-center gap-4" data-testid="profile-name">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
                  <User className="h-4 w-4 text-primary" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">{t("name_label")}</p>
                  <p className="text-[14px] font-semibold text-foreground">{profile.name}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-email">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
                  <Mail className="h-4 w-4 text-primary" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">{t("email_label")}</p>
                  <p className="text-[14px] font-semibold text-foreground">{profile.email}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-role">
                <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-primary/10">
                  <Shield className="h-4 w-4 text-primary" />
                </div>
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">{t("role_label")}</p>
                  <p className="text-[14px] font-semibold text-foreground capitalize">{role.toLowerCase()}</p>
                </div>
              </div>

              <div className="flex items-center gap-4" data-testid="profile-clinic">
                <div className="h-9 w-9" />
                <div>
                  <p className="text-[12px] font-semibold text-muted-foreground uppercase tracking-wider">{t("clinic_label")}</p>
                  <p className="text-[14px] font-semibold text-foreground">{profile.clinicName}</p>
                </div>
              </div>
            </>
          ) : (
            <p className="text-[13px] text-muted-foreground" data-testid="profile-not-loaded">
              {t("not_loaded")}
            </p>
          )}
        </CardContent>
      </Card>

      <Card className="border-border/80 shadow-sm" data-testid="profile-security-card">
        <CardHeader>
          <CardTitle className="text-[15px] font-bold text-foreground">{t("security_title")}</CardTitle>
          <CardDescription className="text-[13px] text-muted-foreground">{t("security_description")}</CardDescription>
        </CardHeader>
        <CardContent>
          <Button variant="outline" data-testid="change-password-btn" className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted">
            {t("change_password")}
          </Button>
        </CardContent>
      </Card>
    </PageContainer>
  )
}
