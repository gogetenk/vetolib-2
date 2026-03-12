"use client"

import { useEffect, useState, useCallback } from "react"
import { useTranslations } from "next-intl"
import { Button } from "@/components/ui/button"
import { DayHoursRow } from "./DayHoursRow"
import { getMessagingHours, updateMessagingHours } from "@/lib/api/messaging"
import type { MessagingHoursDto } from "@/lib/api/messaging-types"
import { toast } from "sonner"

// UAE default hours
const UAE_DEFAULT_HOURS: MessagingHoursDto[] = [
  { dayOfWeek: 0, openTime: "08:00", closeTime: "20:00", isClosed: false }, // Sunday
  { dayOfWeek: 1, openTime: "08:00", closeTime: "20:00", isClosed: false }, // Monday
  { dayOfWeek: 2, openTime: "08:00", closeTime: "20:00", isClosed: false }, // Tuesday
  { dayOfWeek: 3, openTime: "08:00", closeTime: "20:00", isClosed: false }, // Wednesday
  { dayOfWeek: 4, openTime: "08:00", closeTime: "20:00", isClosed: false }, // Thursday
  { dayOfWeek: 5, openTime: "08:00", closeTime: "12:00", isClosed: false }, // Friday
  { dayOfWeek: 6, openTime: "09:00", closeTime: "17:00", isClosed: true },  // Saturday
]

export function MessagingHoursPage() {
  const t = useTranslations("messaging_admin.hours")
  const [hours, setHours] = useState<MessagingHoursDto[]>(UAE_DEFAULT_HOURS)
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)

  const load = useCallback(async () => {
    try {
      setIsLoading(true)
      const data = await getMessagingHours()
      if (data && data.length > 0) {
        // Sort by dayOfWeek
        setHours([...data].sort((a, b) => a.dayOfWeek - b.dayOfWeek))
      }
    } catch {
      toast.error(t("load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    load()
  }, [load])

  function handleDayChange(updated: MessagingHoursDto) {
    setHours((prev) =>
      prev.map((h) => (h.dayOfWeek === updated.dayOfWeek ? updated : h))
    )
  }

  async function handleSave() {
    try {
      setIsSaving(true)
      await updateMessagingHours(hours)
      toast.success(t("save_success"))
    } catch {
      toast.error(t("save_failed"))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="space-y-6" data-testid="messaging-hours-page">
      <div>
        <h2 className="text-xl font-semibold">{t("title")}</h2>
        <p className="text-sm text-muted-foreground mt-1">{t("subtitle")}</p>
      </div>

      {isLoading ? (
        <div data-testid="hours-loading" className="text-sm text-muted-foreground">
          {t("loading")}
        </div>
      ) : (
        <div className="rounded-md border overflow-hidden" data-testid="hours-table">
          <table className="w-full">
            <thead className="bg-muted/40">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground w-32">
                  {t("col_day")}
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  {t("col_status")}
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  {t("col_open")}
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium text-muted-foreground">
                  {t("col_close")}
                </th>
              </tr>
            </thead>
            <tbody>
              {hours.map((h) => (
                <DayHoursRow
                  key={h.dayOfWeek}
                  hours={h}
                  onChange={handleDayChange}
                />
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="flex flex-col gap-2">
        <p className="text-xs text-muted-foreground italic">
          {t("outside_hours_note")}
        </p>
        <div>
          <Button
            data-testid="save-hours-btn"
            onClick={handleSave}
            disabled={isSaving || isLoading}
          >
            {isSaving ? t("saving") : t("save")}
          </Button>
        </div>
      </div>
    </div>
  )
}
