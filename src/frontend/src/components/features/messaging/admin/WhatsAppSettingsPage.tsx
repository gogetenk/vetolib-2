"use client"

import { useCallback, useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import {
  getWhatsAppConfig,
  updateWhatsAppConfig,
  testWhatsAppConnection,
} from "@/lib/api/messaging"
import type { WhatsAppConfigDto } from "@/lib/api/messaging-types"

export function WhatsAppSettingsPage() {
  const t = useTranslations("messaging_admin.whatsapp")

  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [testing, setTesting] = useState(false)

  const [enabled, setEnabled] = useState(false)
  const [businessAccountId, setBusinessAccountId] = useState("")
  const [phoneNumberId, setPhoneNumberId] = useState("")
  const [accessToken, setAccessToken] = useState("")
  const [optInCount, setOptInCount] = useState(0)

  const loadConfig = useCallback(async () => {
    try {
      setLoading(true)
      const config: WhatsAppConfigDto = await getWhatsAppConfig()
      setEnabled(config.enabled)
      setBusinessAccountId(config.businessAccountId)
      setPhoneNumberId(config.phoneNumberId)
      setAccessToken(config.accessToken)
      setOptInCount(config.optInCount)
    } catch {
      toast.error(t("load_failed"))
    } finally {
      setLoading(false)
    }
  }, [t])

  useEffect(() => {
    loadConfig()
  }, [loadConfig])

  const handleSave = async () => {
    setSaving(true)
    try {
      const updated = await updateWhatsAppConfig({
        enabled,
        businessAccountId,
        phoneNumberId,
        accessToken,
      })
      setEnabled(updated.enabled)
      setOptInCount(updated.optInCount)
      toast.success(t("save_success"))
    } catch {
      toast.error(t("save_failed"))
    } finally {
      setSaving(false)
    }
  }

  const handleTestConnection = async () => {
    setTesting(true)
    try {
      const result = await testWhatsAppConnection({
        enabled,
        businessAccountId,
        phoneNumberId,
        accessToken,
      })
      if (result.success) {
        toast.success(result.message)
      } else {
        toast.error(result.message)
      }
    } catch {
      toast.error(t("test_failed"))
    } finally {
      setTesting(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-12" data-testid="whatsapp-loading">
        <p className="text-sm text-muted-foreground">{t("loading")}</p>
      </div>
    )
  }

  return (
    <div className="space-y-6" data-testid="whatsapp-settings-page">
      <div>
        <h2 className="text-xl font-semibold tracking-tight">{t("title")}</h2>
        <p className="text-sm text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      {/* Enable/Disable Toggle */}
      <Card data-testid="whatsapp-toggle-card">
        <CardHeader>
          <CardTitle className="text-base">{t("enable_title")}</CardTitle>
          <CardDescription>{t("enable_description")}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-3">
            <Switch
              checked={enabled}
              onCheckedChange={setEnabled}
              data-testid="whatsapp-enabled-toggle"
              aria-label={t("enable_title")}
            />
            <Label className="text-sm">
              {enabled ? t("status_enabled") : t("status_disabled")}
            </Label>
          </div>
        </CardContent>
      </Card>

      {/* WABA Credentials Form */}
      <Card data-testid="whatsapp-credentials-card">
        <CardHeader>
          <CardTitle className="text-base">{t("credentials_title")}</CardTitle>
          <CardDescription>{t("credentials_description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="businessAccountId">{t("business_account_id")}</Label>
            <Input
              id="businessAccountId"
              value={businessAccountId}
              onChange={(e) => setBusinessAccountId(e.target.value)}
              placeholder={t("business_account_id_placeholder")}
              data-testid="whatsapp-business-account-id"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="phoneNumberId">{t("phone_number_id")}</Label>
            <Input
              id="phoneNumberId"
              value={phoneNumberId}
              onChange={(e) => setPhoneNumberId(e.target.value)}
              placeholder={t("phone_number_id_placeholder")}
              data-testid="whatsapp-phone-number-id"
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="accessToken">{t("access_token")}</Label>
            <Input
              id="accessToken"
              type="password"
              value={accessToken}
              onChange={(e) => setAccessToken(e.target.value)}
              placeholder={t("access_token_placeholder")}
              data-testid="whatsapp-access-token"
            />
          </div>

          <div className="flex gap-3 pt-2">
            <Button
              onClick={handleSave}
              disabled={saving}
              data-testid="whatsapp-save-button"
            >
              {saving ? t("saving") : t("save")}
            </Button>
            <Button
              variant="outline"
              onClick={handleTestConnection}
              disabled={testing || !businessAccountId || !phoneNumberId || !accessToken}
              data-testid="whatsapp-test-button"
            >
              {testing ? t("testing") : t("test_connection")}
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Opt-in Status (read-only) */}
      <Card data-testid="whatsapp-optin-card">
        <CardHeader>
          <CardTitle className="text-base">{t("optin_title")}</CardTitle>
          <CardDescription>{t("optin_description")}</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-baseline gap-2">
            <span className="text-3xl font-bold" data-testid="whatsapp-optin-count">
              {optInCount}
            </span>
            <span className="text-sm text-muted-foreground">{t("optin_label")}</span>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
