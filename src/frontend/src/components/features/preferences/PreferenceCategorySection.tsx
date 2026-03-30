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
      className="bg-white border border-border/80 rounded-xl shadow-sm overflow-hidden transition-all duration-200 ease-in-out"
      data-testid={`pref-section-${category.key}`}
    >
      {/* Header */}
      <button
        type="button"
        onClick={() => setIsOpen((prev) => !prev)}
        className="w-full flex items-center justify-between px-4 py-3 bg-muted hover:bg-muted/80 transition-colors text-start"
        data-testid={`pref-section-toggle-${category.key}`}
        aria-expanded={isOpen}
      >
        <div>
          <h3 className="text-[14px] font-bold text-foreground">{category.label}</h3>
          <p className="text-[12px] text-muted-foreground mt-0.5">{category.description}</p>
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
        <div className="px-4 divide-y divide-border/30">
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
