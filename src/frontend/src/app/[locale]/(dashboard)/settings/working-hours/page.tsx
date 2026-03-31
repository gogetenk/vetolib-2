"use client"

import { useEffect, useState, useCallback } from "react"
import { toast } from "sonner"
import { useRole } from "@/hooks/use-role"
import { useTranslations } from "next-intl"
import {
  getWorkingHours,
  updateWorkingHours,
} from "@/lib/api/working-hours"
import type { WorkingHoursDayDto, DayOfWeek } from "@/lib/api/working-hours"
import { PageContainer } from "@/components/ui/page-container"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { Switch } from "@/components/ui/switch"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

const DAY_ORDER: DayOfWeek[] = [
  "Sunday",
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
]

function emptyDay(dayOfWeek: DayOfWeek): WorkingHoursDayDto {
  return {
    dayOfWeek,
    isOpen: false,
    openTime: null,
    closeTime: null,
    breakStartTime: null,
    breakEndTime: null,
  }
}

export default function WorkingHoursPage() {
  const t = useTranslations("working_hours")
  const role = useRole()
  const isAdmin = role === "ADMIN"

  const [days, setDays] = useState<WorkingHoursDayDto[]>(
    DAY_ORDER.map(emptyDay)
  )
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [isDirty, setIsDirty] = useState(false)

  const loadWorkingHours = useCallback(async () => {
    try {
      setIsLoading(true)
      const data = await getWorkingHours()
      // Ensure days are in correct order
      const ordered = DAY_ORDER.map(
        (day) =>
          data.days.find((d) => d.dayOfWeek === day) ?? emptyDay(day)
      )
      setDays(ordered)
      setIsDirty(false)
    } catch {
      toast.error(t("errors.load_failed"))
    } finally {
      setIsLoading(false)
    }
  }, [t])

  useEffect(() => {
    loadWorkingHours()
  }, [loadWorkingHours])

  const updateDay = useCallback(
    (index: number, updates: Partial<WorkingHoursDayDto>) => {
      setDays((prev) =>
        prev.map((day, i) => (i === index ? { ...day, ...updates } : day))
      )
      setIsDirty(true)
    },
    []
  )

  const handleToggleOpen = useCallback(
    (index: number, checked: boolean) => {
      if (checked) {
        updateDay(index, {
          isOpen: true,
          openTime: "08:00",
          closeTime: "18:00",
        })
      } else {
        updateDay(index, {
          isOpen: false,
          openTime: null,
          closeTime: null,
          breakStartTime: null,
          breakEndTime: null,
        })
      }
    },
    [updateDay]
  )

  const handleSave = useCallback(async () => {
    setIsSaving(true)
    try {
      await updateWorkingHours({ days })
      setIsDirty(false)
      toast.success(t("save_success"))
    } catch {
      toast.error(t("errors.save_failed"))
    } finally {
      setIsSaving(false)
    }
  }, [days, t])

  if (isLoading) {
    return (
      <PageContainer data-testid="working-hours-page">
        <div className="space-y-4">
          {[...Array(7)].map((_, i) => (
            <Skeleton key={i} className="h-16 w-full rounded-xl" />
          ))}
        </div>
      </PageContainer>
    )
  }

  return (
    <PageContainer data-testid="working-hours-page">
      <div>
        <h1 className="text-[22px] font-bold text-foreground flex items-center gap-2">
          <span className="w-1 h-5 bg-primary rounded-full"></span>
          {t("title")}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ms-3">
          {t("subtitle")}
        </p>
      </div>

      <div className="bg-card border border-border/80 rounded-xl shadow-sm overflow-hidden">
        {/* Header row */}
        <div className="hidden md:grid md:grid-cols-[180px_80px_1fr_1fr_1fr_1fr] gap-4 px-5 py-3 bg-muted/50 border-b border-border/60 text-[12px] font-semibold text-muted-foreground uppercase tracking-wide">
          <span>{t("table.day")}</span>
          <span>{t("table.open")}</span>
          <span>{t("table.open_time")}</span>
          <span>{t("table.close_time")}</span>
          <span>{t("table.break_start")}</span>
          <span>{t("table.break_end")}</span>
        </div>

        {/* Day rows */}
        {days.map((day, index) => (
          <div
            key={day.dayOfWeek}
            className="grid grid-cols-1 md:grid-cols-[180px_80px_1fr_1fr_1fr_1fr] gap-4 px-5 py-4 border-b border-border/40 last:border-b-0 items-center"
            data-testid={`working-hours-row-${day.dayOfWeek.toLowerCase()}`}
          >
            {/* Day name */}
            <span className="text-[14px] font-medium text-foreground">
              {t(`days.${day.dayOfWeek.toLowerCase()}`)}
            </span>

            {/* Open toggle */}
            <div className="flex items-center gap-2">
              <Switch
                checked={day.isOpen}
                onCheckedChange={(checked) =>
                  handleToggleOpen(index, checked)
                }
                disabled={!isAdmin}
                data-testid={`working-hours-toggle-${day.dayOfWeek.toLowerCase()}`}
                aria-label={`${day.dayOfWeek} open`}
              />
              <span className="text-[12px] text-muted-foreground md:hidden">
                {day.isOpen ? t("status.open") : t("status.closed")}
              </span>
            </div>

            {/* Open time */}
            <div>
              <Label className="sr-only">
                {t("table.open_time")}
              </Label>
              <Input
                type="time"
                value={day.openTime ?? ""}
                onChange={(e) =>
                  updateDay(index, { openTime: e.target.value })
                }
                disabled={!isAdmin || !day.isOpen}
                data-testid={`working-hours-open-time-${day.dayOfWeek.toLowerCase()}`}
                className="h-9 text-[13px]"
              />
            </div>

            {/* Close time */}
            <div>
              <Label className="sr-only">
                {t("table.close_time")}
              </Label>
              <Input
                type="time"
                value={day.closeTime ?? ""}
                onChange={(e) =>
                  updateDay(index, { closeTime: e.target.value })
                }
                disabled={!isAdmin || !day.isOpen}
                data-testid={`working-hours-close-time-${day.dayOfWeek.toLowerCase()}`}
                className="h-9 text-[13px]"
              />
            </div>

            {/* Break start */}
            <div>
              <Label className="sr-only">
                {t("table.break_start")}
              </Label>
              <Input
                type="time"
                value={day.breakStartTime ?? ""}
                onChange={(e) =>
                  updateDay(index, {
                    breakStartTime: e.target.value || null,
                  })
                }
                disabled={!isAdmin || !day.isOpen}
                placeholder={t("table.optional")}
                data-testid={`working-hours-break-start-${day.dayOfWeek.toLowerCase()}`}
                className="h-9 text-[13px]"
              />
            </div>

            {/* Break end */}
            <div>
              <Label className="sr-only">
                {t("table.break_end")}
              </Label>
              <Input
                type="time"
                value={day.breakEndTime ?? ""}
                onChange={(e) =>
                  updateDay(index, {
                    breakEndTime: e.target.value || null,
                  })
                }
                disabled={!isAdmin || !day.isOpen}
                placeholder={t("table.optional")}
                data-testid={`working-hours-break-end-${day.dayOfWeek.toLowerCase()}`}
                className="h-9 text-[13px]"
              />
            </div>
          </div>
        ))}
      </div>

      {/* Save button (Admin only) */}
      {isAdmin && (
        <div className="flex justify-end">
          <Button
            onClick={handleSave}
            disabled={isSaving || !isDirty}
            data-testid="working-hours-save-btn"
            className="rounded-xl h-10 px-6 font-semibold"
          >
            {isSaving ? t("saving") : t("save")}
          </Button>
        </div>
      )}
    </PageContainer>
  )
}
