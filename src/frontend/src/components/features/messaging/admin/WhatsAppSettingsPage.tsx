"use client"

import { useCallback, useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
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

  const [hasAccessToken, setHasAccessToken] = useState(false)
  const [wabaId, setWabaId] = useState("")
  const [phoneNumberId, setPhoneNumberId] = useState("")
  const [accessToken, setAccessToken] = useState("")

  const loadConfig = useCallback(async () => {
    try {
      setLoading(true)
      const config: WhatsAppConfigDto = await getWhatsAppConfig()
      setHasAccessToken(config.hasAccessToken)
      setWabaId(config.wabaId)
      setPhoneNumberId(config.phoneNumberId)
      // Never populate the access token from the backend (it's not returned)
      setAccessToken("")
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
        wabaId,
        phoneNumberId,
        accessToken,
      })
      setHasAccessToken(updated.hasAccessToken)
      setWabaId(updated.wabaId)
      setPhoneNumberId(updated.phoneNumberId)
      setAccessToken("")
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
        recipientPhone: "+971501234567",
        templateName: "hello_world",
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

      {/* Connection Status */}
      {hasAccessToken && (
        <Card data-testid="whatsapp-status-card">
          <CardContent className="pt-6">
            <p className="text-sm text-green-600 font-medium" data-testid="whatsapp-connected-status">
              {t("status_connected")}
            </p>
          </CardContent>
        </Card>
      )}

      {/* WABA Credentials Form */}
      <Card data-testid="whatsapp-credentials-card">
        <CardHeader>
          <CardTitle className="text-base">{t("credentials_title")}</CardTitle>
          <CardDescription>{t("credentials_description")}</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="wabaId">{t("business_account_id")}</Label>
            <Input
              id="wabaId"
              value={wabaId}
              onChange={(e) => setWabaId(e.target.value)}
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
              placeholder={hasAccessToken ? t("access_token_placeholder_existing") : t("access_token_placeholder")}
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
              disabled={testing || !wabaId || !phoneNumberId}
              data-testid="whatsapp-test-button"
            >
              {testing ? t("testing") : t("test_connection")}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
