"use client"

import { useState } from "react"
import { ChevronDownIcon } from "lucide-react"
import { cn } from "@/lib/utils"
import { PreferenceToggle } from "./PreferenceToggle"
import { PreferenceSelect } from "./PreferenceSelect"
import { PreferenceTimePicker } from "./PreferenceTimePicker"
import type { PreferenceCategoryDto } from "@/lib/api/preferences"

interface PreferenceCategorySectionProps {
  category: PreferenceCategoryDto
  isAdminUser: boolean
  onToggle: (key: string, value: string) => void
  onUpdate: (key: string, value: string) => void
  defaultOpen?: boolean
}

export function PreferenceCategorySection({
  category,
  isAdminUser,
  onToggle,
  onUpdate,
  defaultOpen = true,
}: PreferenceCategorySectionProps) {
  const [isOpen, setIsOpen] = useState(defaultOpen)

  return (
    <div
      className="border rounded-lg overflow-hidden"
      data-testid={`pref-section-${category.key}`}
    >
      {/* Header */}
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="w-full flex items-center justify-between px-4 py-3 bg-muted/30 hover:bg-muted/50 transition-colors text-left"
        data-testid={`pref-section-toggle-${category.key}`}
        aria-expanded={isOpen}
      >
        <div>
          <h3 className="text-sm font-semibold">{category.label}</h3>
          <p className="text-xs text-muted-foreground mt-0.5">{category.description}</p>
        </div>
        <ChevronDownIcon
          className={cn(
            "size-4 text-muted-foreground shrink-0 transition-transform duration-200",
            isOpen && "rotate-180"
          )}
        />
      </button>

      {/* Content */}
      {isOpen && (
        <div className="px-4 divide-y divide-border">
          {category.items.map((item) => {
            if (item.valueType === 'boolean') {
              return (
                <PreferenceToggle
                  key={item.key}
                  item={item}
                  isAdminUser={isAdminUser}
                  onToggle={onToggle}
                />
              )
            }
            if (item.valueType === 'time') {
              return (
                <PreferenceTimePicker
                  key={item.key}
                  item={item}
                  isAdminUser={isAdminUser}
                  onUpdate={onUpdate}
                />
              )
            }
            // string with options -> select
            return (
              <PreferenceSelect
                key={item.key}
                item={item}
                isAdminUser={isAdminUser}
                onUpdate={onUpdate}
              />
            )
          })}
        </div>
      )}
    </div>
  )
}
