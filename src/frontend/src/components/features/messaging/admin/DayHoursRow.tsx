"use client"

import { useTranslations } from "next-intl"
import { Input } from "@/components/ui/input"
import type { MessagingHoursDto } from "@/lib/api/messaging-types"

const DAY_NAMES = [
  "sunday",
  "monday",
  "tuesday",
  "wednesday",
  "thursday",
  "friday",
  "saturday",
] as const

interface DayHoursRowProps {
  hours: MessagingHoursDto
  onChange: (updated: MessagingHoursDto) => void
}

export function DayHoursRow({ hours, onChange }: DayHoursRowProps) {
  const t = useTranslations("messaging_admin.hours")
  const dayName = DAY_NAMES[hours.dayOfWeek]

  function handleToggle() {
    onChange({ ...hours, isClosed: !hours.isClosed })
  }

  function handleOpenTime(e: React.ChangeEvent<HTMLInputElement>) {
    onChange({ ...hours, openTime: e.target.value })
  }

  function handleCloseTime(e: React.ChangeEvent<HTMLInputElement>) {
    onChange({ ...hours, closeTime: e.target.value })
  }

  return (
    <tr
      className="border-b border-border/30 last:border-b-0 hover:bg-[#f4f6f9]/50 transition-colors"
      data-testid={`day-row-${dayName}`}
    >
      <td className="px-4 py-3 text-[13px] font-semibold text-[#061e44] w-32">
        {t(`days.${dayName}`)}
      </td>
      <td className="px-4 py-3">
        <label className="flex items-center gap-2 cursor-pointer">
          <input
            type="checkbox"
            checked={!hours.isClosed}
            onChange={handleToggle}
            data-testid={`day-open-toggle-${dayName}`}
            className="h-4 w-4 cursor-pointer accent-primary"
          />
          <span className="text-[13px] text-muted-foreground">
            {hours.isClosed ? t("closed") : t("open")}
          </span>
        </label>
      </td>
      <td className="px-4 py-3">
        <Input
          type="time"
          value={hours.openTime}
          onChange={handleOpenTime}
          disabled={hours.isClosed}
          data-testid={`day-open-time-${dayName}`}
          aria-label={`${t(`days.${dayName}`)} — ${t("col_open")}`}
          className="w-32 rounded-xl border-border/80 text-[13px] disabled:opacity-40"
        />
      </td>
      <td className="px-4 py-3">
        <Input
          type="time"
          value={hours.closeTime}
          onChange={handleCloseTime}
          disabled={hours.isClosed}
          data-testid={`day-close-time-${dayName}`}
          aria-label={`${t(`days.${dayName}`)} — ${t("col_close")}`}
          className="w-32 rounded-xl border-border/80 text-[13px] disabled:opacity-40"
        />
      </td>
    </tr>
  )
}
