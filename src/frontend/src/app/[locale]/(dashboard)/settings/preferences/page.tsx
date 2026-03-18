"use client"

import { useEffect, useState, useCallback } from "react"
import { toast } from "sonner"
import { useRole } from "@/hooks/use-role"
import { useTranslations } from "next-intl"
import {
  getPreferences,
  updatePreference,
  revokeConsent,
} from "@/lib/api/preferences"
import type { PreferenceCategoryDto } from "@/lib/api/preferences"
import { PreferenceCategorySection } from "@/components/features/preferences/PreferenceCategorySection"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"

export default function PreferencesPage() {
  const t = useTranslations('preferences')
  const role = useRole()
  const isAdmin = role === 'ADMIN'

  const [categories, setCategories] = useState<PreferenceCategoryDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [isRevoking, setIsRevoking] = useState(false)

  const loadPreferences = useCallback(async () => {
    try {
      setIsLoading(true)
      const data = await getPreferences()
      setCategories(data)
    } catch {
      toast.error(t('errors.load_failed'))
    } finally {
      setIsLoading(false)
    }
  }, [t])

  useEffect(() => {
    loadPreferences()
  }, [loadPreferences])

  const handleToggle = useCallback(async (key: string, value: string) => {
    // Optimistic update
    setCategories((prev) =>
      prev.map((cat) => ({
        ...cat,
        items: cat.items.map((item) =>
          item.key === key
            ? { ...item, value, source: 'user_override' as const }
            : item
        ),
      }))
    )

    try {
      await updatePreference(key, value)
    } catch (err: unknown) {
      // Rollback on error
      await loadPreferences()
      if (
        err &&
        typeof err === 'object' &&
        'message' in err &&
        typeof (err as { message: string }).message === 'string'
      ) {
        toast.error((err as { message: string }).message)
      } else {
        toast.error(t('errors.update_failed'))
      }
    }
  }, [loadPreferences, t])

  const handleUpdate = useCallback(async (key: string, value: string) => {
    // Optimistic update
    setCategories((prev) =>
      prev.map((cat) => ({
        ...cat,
        items: cat.items.map((item) =>
          item.key === key
            ? { ...item, value, source: 'user_override' as const }
            : item
        ),
      }))
    )

    try {
      await updatePreference(key, value)
    } catch {
      await loadPreferences()
      toast.error(t('errors.update_failed'))
    }
  }, [loadPreferences, t])

  const handleRevokeAnalyticsConsent = useCallback(async () => {
    setIsRevoking(true)
    try {
      await revokeConsent('analytics')
      await loadPreferences()
      toast.success(t('consent.revoked'))
    } catch {
      toast.error(t('errors.revoke_failed'))
    } finally {
      setIsRevoking(false)
    }
  }, [loadPreferences, t])

  if (isLoading) {
    return (
      <div className="space-y-4" data-testid="preferences-page">
        {[...Array(4)].map((_, i) => (
          <Skeleton key={i} className="h-16 w-full rounded-xl" />
        ))}
      </div>
    )
  }

  return (
    <div className="p-6 lg:p-8 space-y-6" data-testid="preferences-page">
      <div>
        <h1 className="text-[22px] font-bold text-[#061e44] flex items-center gap-2">
          <span className="w-1 h-5 bg-[#303ef5] rounded-full"></span>
          {t('title')}
        </h1>
        <p className="text-[13px] text-muted-foreground mt-1 ml-3">{t('subtitle')}</p>
      </div>

      {/* Category sections */}
      <div className="space-y-4">
        {categories.map((category, index) => (
          <div
            key={category.key}
            className="animate-stagger-fade-in"
            style={{ animationDelay: `${index * 80}ms`, opacity: 0 }}
          >
            <PreferenceCategorySection
              category={category}
              isAdminUser={isAdmin}
              onToggle={handleToggle}
              onUpdate={handleUpdate}
              defaultOpen={true}
            />
          </div>
        ))}
      </div>

      {/* Consent management (only visible to Admins) */}
      {isAdmin && (
        <div className="bg-white border border-border/80 rounded-xl shadow-sm p-5 space-y-3 transition-all duration-200 ease-in-out" data-testid="consent-management-section">
          <h3 className="text-[13px] font-bold text-[#061e44]">{t('consent.title')}</h3>
          <p className="text-[12px] text-muted-foreground">{t('consent.description')}</p>
          <Button
            variant="destructive"
            size="sm"
            onClick={handleRevokeAnalyticsConsent}
            disabled={isRevoking}
            data-testid="revoke-analytics-consent-btn"
            className="rounded-xl h-10 px-5 font-semibold"
          >
            {isRevoking ? t('consent.revoking') : t('consent.revoke_analytics')}
          </Button>
        </div>
      )}
    </div>
  )
}
