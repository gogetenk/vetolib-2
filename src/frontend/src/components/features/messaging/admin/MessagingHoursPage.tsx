"use client"

import { useEffect, useState, useCallback } from "react"
import { useTranslations } from "next-intl"
import { Button } from "@/components/ui/button"
import { DayHoursRow } from "./DayHoursRow"
import { getMessagingHours, updateMessagingHours } from "@/lib/api/messaging"
import type { MessagingHoursDto } from "@/lib/api/messaging-types"
import { toast } from "sonner"
import { PageContainer } from "@/components/ui/page-container"

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
  }, [t])

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
    <PageContainer data-testid="messaging-hours-page">
      <div>
        <h2 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h2>
        <p className="text-[13px] text-muted-foreground mt-1 ms-3">{t("subtitle")}</p>
      </div>

      {isLoading ? (
        <div data-testid="hours-loading" className="text-[13px] text-muted-foreground">
          {t("loading")}
        </div>
      ) : (
        <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden" data-testid="hours-table">
          <table className="w-full">
            <thead className="bg-muted">
              <tr>
                <th className="px-4 py-3 text-start text-[11px] font-bold uppercase tracking-wider text-foreground w-32">
                  {t("col_day")}
                </th>
                <th className="px-4 py-3 text-start text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t("col_status")}
                </th>
                <th className="px-4 py-3 text-start text-[11px] font-bold uppercase tracking-wider text-foreground">
                  {t("col_open")}
                </th>
                <th className="px-4 py-3 text-start text-[11px] font-bold uppercase tracking-wider text-foreground">
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
            className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-11 px-6 shadow-sm"
          >
            {isSaving ? t("saving") : t("save")}
          </Button>
        </div>
      </div>
    </PageContainer>
  )
}
