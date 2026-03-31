'use client'

import { useEffect, useState, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { ArrowLeft, Bell, BellOff, CalendarPlus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  listMyAnimals,
  getAnimalRecords,
  getAnimalVaccinations,
  getAnimalPrescriptions,
  getAnimalWeightHistory,
  getAnimalVaccinationReminders,
  getNotificationPreferences,
  updateNotificationPreferences,
} from '@/lib/api/portal'
import type {
  PortalAnimalDto,
  PortalMedicalRecordDto,
  PortalVaccinationDto,
  PortalPrescriptionDto,
  PortalWeightEntryDto,
  VaccinationReminderDto,
  NotificationPreferencesDto,
} from '@/lib/api/portal'
import { ApiError } from '@/lib/api/client'
import { cn } from '@/lib/utils'
import { WeightChart } from '@/components/features/patients/WeightChart'

type TabKey = 'overview' | 'records' | 'vaccinations' | 'prescriptions' | 'weight'

function formatDate(dateStr: string | null | undefined): string {
  if (!dateStr) return '—'
  return new Date(dateStr).toLocaleDateString('en-AE', {
    timeZone: 'Asia/Dubai',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export default function AnimalDetailPage() {
  const t = useTranslations('portal.animal_detail')
  const params = useParams<{ locale: string; clinicSlug: string; id: string }>()
  const router = useRouter()
  const animalId = params.id

  const [animal, setAnimal] = useState<PortalAnimalDto | null>(null)
  const [activeTab, setActiveTab] = useState<TabKey>('overview')
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Tab data states
  const [records, setRecords] = useState<PortalMedicalRecordDto[]>([])
  const [vaccinations, setVaccinations] = useState<PortalVaccinationDto[]>([])
  const [prescriptions, setPrescriptions] = useState<PortalPrescriptionDto[]>([])
  const [weightHistory, setWeightHistory] = useState<PortalWeightEntryDto[]>([])
  const [vaccinationReminders, setVaccinationReminders] = useState<VaccinationReminderDto[]>([])
  const [notifPrefs, setNotifPrefs] = useState<NotificationPreferencesDto | null>(null)
  const [notifSaving, setNotifSaving] = useState(false)
  const [tabLoading, setTabLoading] = useState(false)
  const [loadedTabs, setLoadedTabs] = useState<Set<TabKey>>(new Set())

  // Load animal info + vaccination reminders + notification prefs
  useEffect(() => {
    let cancelled = false
    async function load() {
      try {
        const [animals, reminders, prefs] = await Promise.all([
          listMyAnimals(),
          getAnimalVaccinationReminders(animalId),
          getNotificationPreferences(),
        ])
        if (cancelled) return
        const found = animals.find((a) => a.id === animalId)
        if (found) {
          setAnimal(found)
          setVaccinationReminders(reminders)
          setNotifPrefs(prefs)
        } else {
          setError('not_found')
        }
        setIsLoading(false)
      } catch (err) {
        if (cancelled) return
        if (err instanceof ApiError && err.status === 401) {
          setError('expired')
        } else {
          setError('generic')
        }
        setIsLoading(false)
      }
    }
    load()
    return () => { cancelled = true }
  }, [animalId])

  // Load tab data on tab change
  const loadTabData = useCallback(async (tab: TabKey) => {
    if (loadedTabs.has(tab) || tab === 'overview') return
    setTabLoading(true)
    try {
      switch (tab) {
        case 'records': {
          const data = await getAnimalRecords(animalId)
          setRecords(data)
          break
        }
        case 'vaccinations': {
          const data = await getAnimalVaccinations(animalId)
          setVaccinations(data)
          break
        }
        case 'prescriptions': {
          const data = await getAnimalPrescriptions(animalId)
          setPrescriptions(data)
          break
        }
        case 'weight': {
          const data = await getAnimalWeightHistory(animalId)
          setWeightHistory(data)
          break
        }
      }
      setLoadedTabs((prev) => new Set(prev).add(tab))
    } catch {
      // Silently fail — the tab will show empty state
    } finally {
      setTabLoading(false)
    }
  }, [animalId, loadedTabs])

  useEffect(() => {
    if (animal) {
      loadTabData(activeTab)
    }
  }, [activeTab, animal, loadTabData])

  const handleToggleNotification = useCallback(
    async (channel: 'whatsapp' | 'email') => {
      if (!notifPrefs || notifSaving) return
      setNotifSaving(true)
      try {
        const updated = await updateNotificationPreferences({
          whatsappEnabled:
            channel === 'whatsapp' ? !notifPrefs.whatsappEnabled : notifPrefs.whatsappEnabled,
          emailEnabled:
            channel === 'email' ? !notifPrefs.emailEnabled : notifPrefs.emailEnabled,
        })
        setNotifPrefs(updated)
      } catch {
        // silently fail
      } finally {
        setNotifSaving(false)
      }
    },
    [notifPrefs, notifSaving]
  )

  if (isLoading) {
    return (
      <div className="text-center py-12 text-muted-foreground" data-testid="animal-detail-loading">
        {t('loading')}
      </div>
    )
  }

  if (error === 'not_found') {
    return (
      <div className="text-center py-12 text-destructive" data-testid="animal-detail-not-found">
        {t('not_found')}
      </div>
    )
  }

  if (error || !animal) {
    return (
      <div className="text-center py-12 text-destructive" data-testid="animal-detail-error">
        {t('error_generic')}
      </div>
    )
  }

  const tabs: { key: TabKey; label: string }[] = [
    { key: 'overview', label: t('tabs.overview') },
    { key: 'records', label: t('tabs.records') },
    { key: 'vaccinations', label: t('tabs.vaccinations') },
    { key: 'prescriptions', label: t('tabs.prescriptions') },
    { key: 'weight', label: t('tabs.weight') },
  ]

  return (
    <div data-testid="animal-detail-page">
      {/* Back button */}
      <Button
        variant="ghost"
        size="sm"
        onClick={() =>
          router.push(`/${params.locale}/portal/${params.clinicSlug}/pets`)
        }
        data-testid="animal-detail-back"
        className="mb-4 -ms-2 text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('back')}
      </Button>

      {/* Animal header */}
      <div className="mb-6" data-testid="animal-detail-header">
        <h1 className="text-xl font-bold text-foreground" data-testid="animal-detail-name">
          {animal.name}
        </h1>
        <p className="text-sm text-muted-foreground">
          {animal.breed} &middot; {animal.species}
          {animal.dateOfBirth && (
            <> &middot; {t('born')} {formatDate(animal.dateOfBirth)}</>
          )}
        </p>
      </div>

      {/* Tab bar */}
      <div
        className="flex gap-1 overflow-x-auto border-b border-border/80 mb-4 -mx-1 px-1"
        data-testid="animal-detail-tabs"
        role="tablist"
      >
        {tabs.map((tab) => (
          <button
            key={tab.key}
            role="tab"
            aria-selected={activeTab === tab.key}
            onClick={() => setActiveTab(tab.key)}
            data-testid={`animal-tab-${tab.key}`}
            className={cn(
              'whitespace-nowrap px-3 py-2 text-sm font-medium border-b-2 transition-colors',
              activeTab === tab.key
                ? 'border-primary text-primary'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            )}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* Tab content */}
      <div data-testid="animal-tab-content">
        {tabLoading ? (
          <div className="text-center py-8 text-muted-foreground" data-testid="tab-loading">
            {t('tab_loading')}
          </div>
        ) : (
          <>
            {activeTab === 'overview' && (
              <OverviewTab
                animal={animal}
                t={t}
                vaccinationReminders={vaccinationReminders}
                notifPrefs={notifPrefs}
                notifSaving={notifSaving}
                onToggleNotification={handleToggleNotification}
                locale={params.locale}
                clinicSlug={params.clinicSlug}
              />
            )}
            {activeTab === 'records' && <RecordsTab records={records} t={t} />}
            {activeTab === 'vaccinations' && <VaccinationsTab vaccinations={vaccinations} t={t} />}
            {activeTab === 'prescriptions' && <PrescriptionsTab prescriptions={prescriptions} t={t} />}
            {activeTab === 'weight' && <WeightTab weightHistory={weightHistory} t={t} />}
          </>
        )}
      </div>
    </div>
  )
}

// ─── Tab Components ──────────────────────────────────────────────────────────

interface TranslationFn {
  (key: string, values?: Record<string, string | number>): string
}

function OverviewTab({
  animal,
  t,
  vaccinationReminders,
  notifPrefs,
  notifSaving,
  onToggleNotification,
  locale,
  clinicSlug,
}: {
  animal: PortalAnimalDto
  t: TranslationFn
  vaccinationReminders: VaccinationReminderDto[]
  notifPrefs: NotificationPreferencesDto | null
  notifSaving: boolean
  onToggleNotification: (channel: 'whatsapp' | 'email') => void
  locale: string
  clinicSlug: string
}) {
  return (
    <div className="space-y-6" data-testid="tab-overview">
      {/* Basic info */}
      <div className="space-y-3">
        <InfoRow label={t('overview.name')} value={animal.name} />
        <InfoRow label={t('overview.species')} value={animal.species} />
        <InfoRow label={t('overview.breed')} value={animal.breed} />
        <InfoRow label={t('overview.date_of_birth')} value={formatDate(animal.dateOfBirth)} />
        <InfoRow label={t('overview.last_visit')} value={formatDate(animal.lastVisitDate)} />
      </div>

      {/* Vaccination Reminders Card */}
      <div
        className="rounded-xl border border-border/80 bg-white p-4"
        data-testid="vaccination-reminders-card"
      >
        <h3 className="text-sm font-semibold text-foreground mb-3" data-testid="vaccination-reminders-title">
          {t('reminders.title')}
        </h3>

        {vaccinationReminders.length === 0 ? (
          <p
            className="text-sm text-muted-foreground"
            data-testid="vaccination-reminders-empty"
          >
            {t('reminders.empty')}
          </p>
        ) : (
          <div className="space-y-3">
            {vaccinationReminders.map((reminder) => (
              <div
                key={reminder.id}
                className="flex items-start justify-between gap-2"
                data-testid={`vaccination-reminder-${reminder.id}`}
              >
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium text-foreground truncate">
                    {reminder.vaccineName}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {t('reminders.due')}: {formatDate(reminder.dueDate)}
                  </p>
                  <span
                    className={cn(
                      'inline-block mt-1 text-xs font-medium px-2 py-0.5 rounded-full',
                      reminder.status === 'Overdue'
                        ? 'bg-destructive/10 text-destructive'
                        : reminder.status === 'Completed'
                          ? 'bg-green-100 text-green-700'
                          : 'bg-amber-100 text-amber-700'
                    )}
                    data-testid={`vaccination-reminder-status-${reminder.id}`}
                  >
                    {t(`reminders.status_${reminder.status.toLowerCase()}`)}
                  </span>
                </div>
                <a
                  href={`/${locale}/portal/${clinicSlug}/book/new?animalId=${animal.id}&reason=${encodeURIComponent(reminder.vaccineName)}`}
                  className="flex-shrink-0"
                  data-testid={`vaccination-reminder-book-${reminder.id}`}
                >
                  <Button variant="outline" size="sm">
                    <CalendarPlus className="h-3.5 w-3.5 me-1" />
                    {t('reminders.book_appointment')}
                  </Button>
                </a>
              </div>
            ))}
          </div>
        )}

        {/* Notification Preferences */}
        {notifPrefs && (
          <div
            className="mt-4 pt-4 border-t border-border/40"
            data-testid="notification-preferences"
          >
            <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wide mb-2">
              {t('reminders.notification_prefs')}
            </h4>
            <div className="space-y-2">
              <button
                type="button"
                onClick={() => onToggleNotification('whatsapp')}
                disabled={notifSaving}
                className="flex items-center justify-between w-full text-sm py-1"
                data-testid="notification-toggle-whatsapp"
              >
                <span className="text-foreground">{t('reminders.whatsapp')}</span>
                {notifPrefs.whatsappEnabled ? (
                  <Bell className="h-4 w-4 text-primary" />
                ) : (
                  <BellOff className="h-4 w-4 text-muted-foreground" />
                )}
              </button>
              <button
                type="button"
                onClick={() => onToggleNotification('email')}
                disabled={notifSaving}
                className="flex items-center justify-between w-full text-sm py-1"
                data-testid="notification-toggle-email"
              >
                <span className="text-foreground">{t('reminders.email')}</span>
                {notifPrefs.emailEnabled ? (
                  <Bell className="h-4 w-4 text-primary" />
                ) : (
                  <BellOff className="h-4 w-4 text-muted-foreground" />
                )}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex justify-between items-center py-2 border-b border-border/40">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium text-foreground">{value}</span>
    </div>
  )
}

function RecordsTab({ records, t }: { records: PortalMedicalRecordDto[]; t: TranslationFn }) {
  if (records.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground" data-testid="tab-records-empty">
        {t('records.empty')}
      </div>
    )
  }
  return (
    <div className="space-y-3" data-testid="tab-records">
      {records.map((rec) => (
        <div
          key={rec.id}
          className="rounded-xl border border-border/80 bg-white p-4"
          data-testid={`record-${rec.id}`}
        >
          <div className="flex justify-between items-start mb-2">
            <p className="font-semibold text-foreground text-sm">{rec.reason}</p>
            <span className="text-xs text-muted-foreground flex-shrink-0 ms-2">
              {formatDate(rec.visitDate)}
            </span>
          </div>
          <p className="text-sm text-muted-foreground mb-1">
            <span className="font-medium">{t('records.diagnosis')}:</span> {rec.diagnosis}
          </p>
          <p className="text-sm text-muted-foreground mb-1">
            <span className="font-medium">{t('records.treatment')}:</span> {rec.treatment}
          </p>
          <p className="text-xs text-muted-foreground">{rec.vetName}</p>
        </div>
      ))}
    </div>
  )
}

function VaccinationsTab({ vaccinations, t }: { vaccinations: PortalVaccinationDto[]; t: TranslationFn }) {
  if (vaccinations.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground" data-testid="tab-vaccinations-empty">
        {t('vaccinations.empty')}
      </div>
    )
  }
  return (
    <div className="space-y-3" data-testid="tab-vaccinations">
      {vaccinations.map((vax) => (
        <div
          key={vax.id}
          className="rounded-xl border border-border/80 bg-white p-4"
          data-testid={`vaccination-${vax.id}`}
        >
          <p className="font-semibold text-foreground text-sm mb-1">{vax.name}</p>
          <p className="text-sm text-muted-foreground">
            {t('vaccinations.administered')}: {formatDate(vax.administeredAt)}
          </p>
          {vax.nextDueAt && (
            <p className="text-sm text-muted-foreground">
              {t('vaccinations.next_due')}: {formatDate(vax.nextDueAt)}
            </p>
          )}
          <p className="text-xs text-muted-foreground mt-1">{vax.vetName}</p>
        </div>
      ))}
    </div>
  )
}

function PrescriptionsTab({ prescriptions, t }: { prescriptions: PortalPrescriptionDto[]; t: TranslationFn }) {
  if (prescriptions.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground" data-testid="tab-prescriptions-empty">
        {t('prescriptions.empty')}
      </div>
    )
  }
  return (
    <div className="space-y-3" data-testid="tab-prescriptions">
      {prescriptions.map((rx) => (
        <div
          key={rx.id}
          className="rounded-xl border border-border/80 bg-white p-4"
          data-testid={`prescription-${rx.id}`}
        >
          <p className="font-semibold text-foreground text-sm mb-1">{rx.drugName}</p>
          <p className="text-sm text-muted-foreground">
            {t('prescriptions.dosage')}: {rx.dosage}
          </p>
          <p className="text-sm text-muted-foreground">
            {t('prescriptions.frequency')}: {rx.frequency}
          </p>
          <p className="text-sm text-muted-foreground">
            {formatDate(rx.startDate)}
            {rx.endDate && <> — {formatDate(rx.endDate)}</>}
          </p>
          <p className="text-xs text-muted-foreground mt-1">{rx.prescribedBy}</p>
        </div>
      ))}
    </div>
  )
}

function WeightTab({ weightHistory, t }: { weightHistory: PortalWeightEntryDto[]; t: TranslationFn }) {
  if (weightHistory.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground" data-testid="tab-weight-empty">
        {t('weight.empty')}
      </div>
    )
  }

  // Map to WeightCurvePointDto shape expected by WeightChart
  const chartData = weightHistory.map((w) => ({
    date: w.date,
    weightKg: w.weightKg,
  }))

  const latest = weightHistory[weightHistory.length - 1]

  return (
    <div data-testid="tab-weight">
      <div className="mb-4 text-center">
        <p className="text-2xl font-bold text-foreground" data-testid="weight-latest">
          {latest.weightKg} kg
        </p>
        <p className="text-xs text-muted-foreground">
          {t('weight.latest')} &middot; {formatDate(latest.date)}
        </p>
      </div>
      <div data-testid="weight-chart">
        <WeightChart data={chartData} />
      </div>
    </div>
  )
}
