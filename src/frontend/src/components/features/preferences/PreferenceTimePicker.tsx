"use client"

import { Badge } from "@/components/ui/badge"
import { Input } from "@/components/ui/input"
import { cn } from "@/lib/utils"
import type { PreferenceItemDto, PreferenceSource } from "@/lib/api/preferences"
import { useTranslations } from "next-intl"

interface PreferenceTimePickerProps {
  item: PreferenceItemDto
  isAdminUser: boolean
  onUpdate: (key: string, value: string) => void
}

function sourceBadgeVariant(source: PreferenceSource) {
  switch (source) {
    case 'user_override':
      return 'default'
    case 'clinic_default':
      return 'secondary'
    case 'system_default':
    default:
      return 'outline'
  }
}

export function PreferenceTimePicker({ item, isAdminUser, onUpdate }: PreferenceTimePickerProps) {
  const t = useTranslations('preferences')
  const isReadOnly = (item.adminOnly && !isAdminUser) || item.disabled

  return (
    <div className="flex items-start justify-between gap-4 py-3">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className={cn("text-[13px] font-semibold text-[#061e44]", isReadOnly && "text-muted-foreground")}>
            {item.label}
          </span>
          <Badge
            variant={sourceBadgeVariant(item.source)}
            className="text-xs px-1.5 py-0 h-4"
            data-testid={`pref-source-badge-${item.key}`}
          >
            {item.source === 'user_override'
              ? t('source.user_override')
              : item.source === 'clinic_default'
              ? t('source.clinic_default')
              : t('source.system_default')}
          </Badge>
        </div>
        {item.description && (
          <p className="text-[12px] text-muted-foreground mt-0.5">{item.description}</p>
        )}
      </div>

      <div className="shrink-0">
        <Input
          type="time"
          value={item.value}
          disabled={isReadOnly}
          onChange={(e) => onUpdate(item.key, e.target.value)}
          data-testid={`pref-time-${item.key}`}
          className="w-28 h-8 text-[12px] rounded-xl border-border/80"
        />
      </div>
    </div>
  )
}
