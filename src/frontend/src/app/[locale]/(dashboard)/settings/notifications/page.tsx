"use client"

import { useEffect, useState, useCallback } from "react"
import { toast } from "sonner"
import { useTranslations } from "next-intl"
import { Bell, Syringe, CalendarCheck, Loader2 } from "lucide-react"
import {
  getReminderConfig,
  updateReminderConfig,
  getReminderLogs,
} from "@/lib/api/reminders"
import type { ReminderConfigDto, ReminderLogDto } from "@/lib/api/reminders"
import { PageContainer } from "@/components/ui/page-container"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Switch } from "@/components/ui/switch"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { useAhaMoment } from "@/hooks/use-aha-moment"

export default function NotificationSettingsPage() {
  const t = useTranslations("reminder_settings")
  const { triggerAha } = useAhaMoment()

  const [config, setConfig] = useState<ReminderConfigDto | null>(null)
  const [logs, setLogs] = useState<ReminderLogDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true)
      const [configData, logsData] = await Promise.all([
        getReminderConfig(),
        getReminderLogs(),
      ])
      setConfig(configData)
      setLogs(logsData)
      if (logsData.some((log) => log.status === "sent")) {
        triggerAha("first_whatsapp_reminder")
      }
    } catch {
      toast.error(t("errors.load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [t, triggerAha])

  useEffect(() => {
    loadData()
  }, [loadData])

  const handleSave = useCallback(async () => {
    if (!config) return
    setIsSaving(true)
    try {
      await updateReminderConfig(config)
      toast.success(t("save_success"))
    } catch {
      toast.error(t("errors.save_failed"))
    } finally {
      setIsSaving(false)
    }
  }, [config, t])

  if (isLoading || !config) {
    return (
      <PageContainer data-testid="notification-settings-page">
        <div className="space-y-4">
          {[...Array(4)].map((_, i) => (
            <Skeleton key={i} className="h-24 w-full rounded-xl" />
          ))}
        </div>
      </PageContainer>
    )
  }

  return (
    <PageContainer data-testid="notification-settings-page">
      <div>
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ms-3">
          {t("subtitle")}
        </p>
      </div>

      {/* Appointment Reminders */}
      <Card
        className="border-border/80 shadow-sm"
        data-testid="appointment-reminders-section"
      >
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                <Bell className="h-5 w-5 text-primary" />
              </div>
              <div>
                <CardTitle className="text-[15px] font-bold text-foreground">
                  {t("appointment.title")}
                </CardTitle>
                <CardDescription className="text-[13px] text-muted-foreground">
                  {t("appointment.description")}
                </CardDescription>
              </div>
            </div>
            <Switch
              checked={config.appointmentReminders.enabled}
              onCheckedChange={(checked: boolean) =>
                setConfig({
                  ...config,
                  appointmentReminders: {
                    ...config.appointmentReminders,
                    enabled: checked,
                  },
                })
              }
              data-testid="appointment-reminders-toggle"
            />
          </div>
        </CardHeader>
        {config.appointmentReminders.enabled && (
          <CardContent>
            <div className="flex items-center gap-3">
              <label
                htmlFor="timing-hours"
                className="text-[13px] text-muted-foreground whitespace-nowrap"
              >
                {t("appointment.timing_label")}
              </label>
              <Input
                id="timing-hours"
                type="number"
                min={1}
                max={72}
                value={config.appointmentReminders.timingHours}
                onChange={(e) =>
                  setConfig({
                    ...config,
                    appointmentReminders: {
                      ...config.appointmentReminders,
                      timingHours: Number(e.target.value) || 24,
                    },
                  })
                }
                className="w-20"
                data-testid="appointment-timing-input"
              />
              <span className="text-[13px] text-muted-foreground">
                {t("appointment.hours_before")}
              </span>
            </div>
          </CardContent>
        )}
      </Card>

      {/* Vaccination Reminders */}
      <Card
        className="border-border/80 shadow-sm"
        data-testid="vaccination-reminders-section"
      >
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                <Syringe className="h-5 w-5 text-primary" />
              </div>
              <div>
                <CardTitle className="text-[15px] font-bold text-foreground">
                  {t("vaccination.title")}
                </CardTitle>
                <CardDescription className="text-[13px] text-muted-foreground">
                  {t("vaccination.description")}
                </CardDescription>
              </div>
            </div>
            <Switch
              checked={config.vaccinationReminders.enabled}
              onCheckedChange={(checked: boolean) =>
                setConfig({
                  ...config,
                  vaccinationReminders: {
                    ...config.vaccinationReminders,
                    enabled: checked,
                  },
                })
              }
              data-testid="vaccination-reminders-toggle"
            />
          </div>
        </CardHeader>
      </Card>

      {/* Follow-up Reminders */}
      <Card
        className="border-border/80 shadow-sm"
        data-testid="followup-reminders-section"
      >
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-primary/10">
                <CalendarCheck className="h-5 w-5 text-primary" />
              </div>
              <div>
                <CardTitle className="text-[15px] font-bold text-foreground">
                  {t("followup.title")}
                </CardTitle>
                <CardDescription className="text-[13px] text-muted-foreground">
                  {t("followup.description")}
                </CardDescription>
              </div>
            </div>
            <Switch
              checked={config.followUpReminders.enabled}
              onCheckedChange={(checked: boolean) =>
                setConfig({
                  ...config,
                  followUpReminders: {
                    ...config.followUpReminders,
                    enabled: checked,
                  },
                })
              }
              data-testid="followup-reminders-toggle"
            />
          </div>
        </CardHeader>
        {config.followUpReminders.enabled && (
          <CardContent>
            <div className="flex items-center gap-3">
              <label
                htmlFor="followup-days"
                className="text-[13px] text-muted-foreground whitespace-nowrap"
              >
                {t("followup.days_label")}
              </label>
              <Input
                id="followup-days"
                type="number"
                min={1}
                max={90}
                value={config.followUpReminders.daysAfter}
                onChange={(e) =>
                  setConfig({
                    ...config,
                    followUpReminders: {
                      ...config.followUpReminders,
                      daysAfter: Number(e.target.value) || 7,
                    },
                  })
                }
                className="w-20"
                data-testid="followup-days-input"
              />
              <span className="text-[13px] text-muted-foreground">
                {t("followup.days_after")}
              </span>
            </div>
          </CardContent>
        )}
      </Card>

      {/* Save Button */}
      <div className="flex justify-end">
        <Button
          onClick={handleSave}
          disabled={isSaving}
          className="rounded-xl h-10 px-6 font-semibold"
          data-testid="save-reminder-config-btn"
        >
          {isSaving && <Loader2 className="me-2 h-4 w-4 animate-spin" />}
          {isSaving ? t("saving") : t("save")}
        </Button>
      </div>

      {/* Reminder Logs */}
      <Card
        className="border-border/80 shadow-sm"
        data-testid="reminder-logs-section"
      >
        <CardHeader>
          <CardTitle className="text-[15px] font-bold text-foreground">
            {t("logs.title")}
          </CardTitle>
          <CardDescription className="text-[13px] text-muted-foreground">
            {t("logs.description")}
          </CardDescription>
        </CardHeader>
        <CardContent>
          {logs.length === 0 ? (
            <p
              className="text-[13px] text-muted-foreground text-center py-8"
              data-testid="reminder-logs-empty"
            >
              {t("logs.empty")}
            </p>
          ) : (
            <Table data-testid="reminder-logs-table">
              <TableHeader>
                <TableRow>
                  <TableHead className="text-[12px]">{t("logs.col_date")}</TableHead>
                  <TableHead className="text-[12px]">{t("logs.col_type")}</TableHead>
                  <TableHead className="text-[12px]">{t("logs.col_patient")}</TableHead>
                  <TableHead className="text-[12px]">{t("logs.col_owner")}</TableHead>
                  <TableHead className="text-[12px]">{t("logs.col_status")}</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {logs.map((log) => (
                  <TableRow key={log.id} data-testid={`reminder-log-row-${log.id}`}>
                    <TableCell className="text-[13px] text-muted-foreground">
                      {new Date(log.sentAt).toLocaleDateString(undefined, {
                        year: "numeric",
                        month: "short",
                        day: "numeric",
                        hour: "2-digit",
                        minute: "2-digit",
                      })}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className="text-[11px] capitalize"
                        data-testid={`reminder-log-type-${log.id}`}
                      >
                        {t(`logs.type_${log.type}`)}
                      </Badge>
                    </TableCell>
                    <TableCell className="text-[13px] font-medium text-foreground">
                      {log.patientName}
                    </TableCell>
                    <TableCell className="text-[13px] text-muted-foreground">
                      {log.ownerName}
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant={
                          log.status === "sent"
                            ? "default"
                            : log.status === "failed"
                              ? "destructive"
                              : "secondary"
                        }
                        className="text-[11px] capitalize"
                        data-testid={`reminder-log-status-${log.id}`}
                      >
                        {t(`logs.status_${log.status}`)}
                      </Badge>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>
    </PageContainer>
  )
}
