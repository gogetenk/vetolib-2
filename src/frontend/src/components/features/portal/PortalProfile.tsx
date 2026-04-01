'use client'

import { useEffect, useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { User, Mail, Phone, PawPrint, ChevronRight, Pencil, Save, X, Loader2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  getPortalProfile,
  updatePortalProfile,
} from '@/lib/api/portal'
import type { PortalOwnerProfileDto } from '@/lib/api/portal'

export function PortalProfile() {
  const t = useTranslations('portal.profile_page')
  const params = useParams<{ locale: string; clinicSlug: string }>()

  const [profile, setProfile] = useState<PortalOwnerProfileDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)

  // Edit mode
  const [editing, setEditing] = useState(false)
  const [saving, setSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [phone, setPhone] = useState('')

  useEffect(() => {
    getPortalProfile()
      .then((data) => {
        setProfile(data)
        setFirstName(data.firstName)
        setLastName(data.lastName)
        setPhone(data.phone)
      })
      .catch(() => {
        setError(true)
      })
      .finally(() => setLoading(false))
  }, [])

  const handleEdit = useCallback(() => {
    if (profile) {
      setFirstName(profile.firstName)
      setLastName(profile.lastName)
      setPhone(profile.phone)
      setSaveError(null)
      setEditing(true)
    }
  }, [profile])

  const handleCancel = useCallback(() => {
    setEditing(false)
    setSaveError(null)
  }, [])

  const handleSave = useCallback(async () => {
    if (!firstName.trim() || !lastName.trim()) {
      setSaveError(t('validation_name_required'))
      return
    }

    setSaving(true)
    setSaveError(null)

    try {
      const updated = await updatePortalProfile({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim(),
      })
      setProfile(updated)
      setEditing(false)
    } catch {
      setSaveError(t('save_error'))
    } finally {
      setSaving(false)
    }
  }, [firstName, lastName, phone, t])

  if (loading) {
    return (
      <div className="space-y-5" data-testid="portal-profile-page">
        <div className="h-8 w-40 rounded bg-muted animate-pulse" />
        <div className="space-y-3" data-testid="portal-profile-loading">
          {[1, 2, 3].map((i) => (
            <div key={i} className="h-14 rounded-xl bg-muted animate-pulse" />
          ))}
        </div>
      </div>
    )
  }

  if (error || !profile) {
    return (
      <div className="space-y-5" data-testid="portal-profile-page">
        <h1
          className="text-[22px] font-bold text-foreground"
          data-testid="portal-profile-title"
        >
          {t('title')}
        </h1>
        <div
          className="text-center py-12 text-muted-foreground"
          data-testid="portal-profile-error"
        >
          <User className="w-10 h-10 mx-auto mb-2 text-muted-foreground/50" />
          <p className="text-[13px]">{t('load_error')}</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6" data-testid="portal-profile-page">
      <div className="flex items-center justify-between">
        <h1
          className="text-[22px] font-bold text-foreground"
          data-testid="portal-profile-title"
        >
          {t('title')}
        </h1>
        {!editing && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleEdit}
            data-testid="portal-profile-edit-btn"
            className="flex items-center gap-1 text-primary"
          >
            <Pencil className="h-4 w-4" />
            {t('edit')}
          </Button>
        )}
      </div>

      {/* Account info section */}
      <section
        className="bg-white rounded-xl border border-border/80 divide-y divide-border/60"
        data-testid="portal-profile-info"
      >
        <h2 className="px-4 py-3 text-[13px] font-semibold text-muted-foreground uppercase tracking-wide">
          {t('account_info')}
        </h2>

        {editing ? (
          <div className="px-4 py-4 space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="profile-first-name" className="text-[13px]">
                {t('first_name')}
              </Label>
              <Input
                id="profile-first-name"
                data-testid="portal-profile-first-name-input"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                disabled={saving}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="profile-last-name" className="text-[13px]">
                {t('last_name')}
              </Label>
              <Input
                id="profile-last-name"
                data-testid="portal-profile-last-name-input"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                disabled={saving}
              />
            </div>
            <div className="space-y-1.5">
              <Label htmlFor="profile-phone" className="text-[13px]">
                {t('phone')}
              </Label>
              <Input
                id="profile-phone"
                data-testid="portal-profile-phone-input"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                disabled={saving}
              />
            </div>

            {saveError && (
              <p
                className="text-sm text-destructive"
                data-testid="portal-profile-save-error"
              >
                {saveError}
              </p>
            )}

            <div className="flex gap-2 pt-1">
              <Button
                size="sm"
                onClick={handleSave}
                disabled={saving}
                data-testid="portal-profile-save-btn"
                className="flex items-center gap-1"
              >
                {saving ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Save className="h-4 w-4" />
                )}
                {saving ? t('saving') : t('save')}
              </Button>
              <Button
                variant="ghost"
                size="sm"
                onClick={handleCancel}
                disabled={saving}
                data-testid="portal-profile-cancel-btn"
                className="flex items-center gap-1"
              >
                <X className="h-4 w-4" />
                {t('cancel')}
              </Button>
            </div>
          </div>
        ) : (
          <>
            {/* Name */}
            <div className="flex items-center gap-3 px-4 py-3">
              <User className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <p className="text-xs text-muted-foreground">{t('name')}</p>
                <p
                  className="text-[14px] font-medium text-foreground"
                  data-testid="portal-profile-name"
                >
                  {profile.firstName} {profile.lastName}
                </p>
              </div>
            </div>

            {/* Email */}
            <div className="flex items-center gap-3 px-4 py-3">
              <Mail className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <p className="text-xs text-muted-foreground">{t('email')}</p>
                <p
                  className="text-[14px] text-foreground"
                  data-testid="portal-profile-email"
                >
                  {profile.email}
                </p>
              </div>
            </div>

            {/* Phone */}
            <div className="flex items-center gap-3 px-4 py-3">
              <Phone className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <p className="text-xs text-muted-foreground">{t('phone')}</p>
                <p
                  className="text-[14px] text-foreground"
                  data-testid="portal-profile-phone"
                >
                  {profile.phone}
                </p>
              </div>
            </div>
          </>
        )}
      </section>

      {/* Linked pets section */}
      <section data-testid="portal-profile-pets-section">
        <h2 className="text-[15px] font-semibold text-foreground mb-3">
          {t('my_pets')}
        </h2>

        {profile.pets.length === 0 ? (
          <div
            className="text-center py-8 text-muted-foreground"
            data-testid="portal-profile-no-pets"
          >
            <PawPrint className="w-8 h-8 mx-auto mb-2 text-muted-foreground/50" />
            <p className="text-[13px]">{t('no_pets')}</p>
          </div>
        ) : (
          <ul className="space-y-2" data-testid="portal-profile-pets-list">
            {profile.pets.map((pet) => (
              <li key={pet.id}>
                <Link
                  href={`/${params.locale}/portal/${params.clinicSlug}/pets/${pet.id}`}
                  data-testid={`portal-profile-pet-${pet.id}`}
                  className="flex items-center gap-3 bg-white rounded-xl border border-border/80 px-4 py-3 hover:border-primary/40 hover:shadow-sm transition-all"
                >
                  <div className="w-9 h-9 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                    <PawPrint className="h-4 w-4 text-primary" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-[14px] font-medium text-foreground">
                      {pet.name}
                    </p>
                    <p className="text-xs text-muted-foreground">
                      {pet.species} &middot; {pet.breed}
                    </p>
                  </div>
                  <ChevronRight className="h-4 w-4 text-muted-foreground flex-shrink-0" />
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
