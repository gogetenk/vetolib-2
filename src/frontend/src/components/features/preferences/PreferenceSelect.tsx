"use client"

import { Badge } from "@/components/ui/badge"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { cn } from "@/lib/utils"
import type { PreferenceItemDto, PreferenceSource } from "@/lib/api/preferences"
import { useTranslations } from "next-intl"

interface PreferenceSelectProps {
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

export function PreferenceSelect({ item, isAdminUser, onUpdate }: PreferenceSelectProps) {
  const t = useTranslations('preferences')
  const isReadOnly = (item.adminOnly && !isAdminUser) || item.disabled
  const options = item.options ?? []

  return (
    <div className="flex items-start justify-between gap-4 py-3">
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 flex-wrap">
          <span className={cn("text-sm font-medium", isReadOnly && "text-muted-foreground")}>
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
          {item.adminOnly && !isAdminUser && (
            <span className="text-xs text-muted-foreground italic">
              {t('admin_only_hint')}
            </span>
          )}
        </div>
        {item.description && (
          <p className="text-xs text-muted-foreground mt-0.5">{item.description}</p>
        )}
      </div>

      <div className="shrink-0">
        <Select
          value={item.value}
          onValueChange={(val) => val !== null && onUpdate(item.key, val)}
          disabled={isReadOnly}
        >
          <SelectTrigger
            data-testid={`pref-select-${item.key}`}
            className="w-32 h-8 text-xs"
          >
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {options.map((opt) => (
              <SelectItem key={opt.value} value={opt.value}>
                {opt.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  )
}
