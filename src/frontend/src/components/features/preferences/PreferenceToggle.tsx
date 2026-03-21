"use client"

import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { cn } from "@/lib/utils"
import type { PreferenceItemDto, PreferenceSource } from "@/lib/api/preferences"
import { useTranslations } from "next-intl"

interface PreferenceToggleProps {
  item: PreferenceItemDto
  isAdminUser: boolean
  onToggle: (key: string, value: string) => void
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

function sourceBadgeLabel(source: PreferenceSource, t: ReturnType<typeof useTranslations>) {
  switch (source) {
    case 'user_override':
      return t('source.user_override')
    case 'clinic_default':
      return t('source.clinic_default')
    case 'system_default':
    default:
      return t('source.system_default')
  }
}

export function PreferenceToggle({ item, isAdminUser, onToggle }: PreferenceToggleProps) {
  const t = useTranslations('preferences')
  const isChecked = item.value === 'true'
  const isReadOnly = item.adminOnly && !isAdminUser
  const isDisabled = item.disabled || isReadOnly

  const switchElement = (
    <Switch
      checked={isChecked}
      onCheckedChange={(checked) => onToggle(item.key, checked ? 'true' : 'false')}
      disabled={isDisabled}
      data-testid={`pref-toggle-${item.key}`}
      aria-label={item.label}
    />
  )

  return (
    <TooltipProvider>
      <div className="flex items-start justify-between gap-4 py-3">
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2 flex-wrap">
            <span className={cn("text-[13px] font-semibold text-foreground", isDisabled && "text-muted-foreground")}>
              {item.label}
            </span>
            <Badge
              variant={sourceBadgeVariant(item.source)}
              className="text-xs px-1.5 py-0 h-4"
              data-testid={`pref-source-badge-${item.key}`}
            >
              {sourceBadgeLabel(item.source, t)}
            </Badge>
            {isReadOnly && (
              <span className="text-xs text-muted-foreground italic">
                {t('admin_only_hint')}
              </span>
            )}
          </div>
          {item.description && (
            <p className="text-[12px] text-muted-foreground mt-0.5">{item.description}</p>
          )}
        </div>

        <div className="shrink-0 flex items-center">
          {item.disabled && item.disabledReason ? (
            <Tooltip>
              <TooltipTrigger
                data-testid={item.key === 'ai.drug_interaction_alerts' ? 'pref-drug-interaction-tooltip' : undefined}
              >
                {switchElement}
              </TooltipTrigger>
              <TooltipContent>
                <p>{item.disabledReason}</p>
              </TooltipContent>
            </Tooltip>
          ) : (
            switchElement
          )}
        </div>
      </div>
    </TooltipProvider>
  )
}
