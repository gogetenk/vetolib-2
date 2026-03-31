'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useParams, useRouter } from 'next/navigation'
import { PawPrint, ArrowLeft } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { listPortalPets } from '@/lib/api/portal'
import type { PortalPetDto } from '@/lib/api/messaging-types'
import { ShareRecordsModal } from './ShareRecordsModal'
import { ActiveSharesList } from './ActiveSharesList'

export function PortalAnimalDetail() {
  const t = useTranslations('portal.pets')
  const params = useParams<{ locale: string; clinicSlug: string; animalId: string }>()
  const router = useRouter()
  const [pet, setPet] = useState<PortalPetDto | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    listPortalPets()
      .then(pets => {
        const found = pets.find(p => p.id === params.animalId)
        setPet(found ?? null)
      })
      .catch(() => {
        // Handled by empty state
      })
      .finally(() => setLoading(false))
  }, [params.animalId])

  if (loading) {
    return (
      <div className="space-y-4" data-testid="animal-detail-loading">
        <div className="h-8 w-32 rounded bg-muted animate-pulse" />
        <div className="h-40 rounded-xl bg-muted animate-pulse" />
      </div>
    )
  }

  if (!pet) {
    return (
      <div className="text-center py-12" data-testid="animal-not-found">
        <PawPrint className="w-10 h-10 mx-auto mb-2 text-muted-foreground/50" />
        <p className="text-sm text-muted-foreground">{t('not_found')}</p>
      </div>
    )
  }

  return (
    <div className="space-y-5" data-testid="animal-detail-page">
      {/* Back button */}
      <Button
        variant="ghost"
        size="sm"
        onClick={() => router.push(`/${params.locale}/portal/${params.clinicSlug}/pets`)}
        data-testid="animal-detail-back"
      >
        <ArrowLeft className="h-4 w-4 me-1" />
        {t('back_to_pets')}
      </Button>

      {/* Animal header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
            <PawPrint className="h-6 w-6 text-primary" />
          </div>
          <div>
            <h1 className="text-xl font-bold text-foreground" data-testid="animal-detail-name">
              {pet.name}
            </h1>
            <p className="text-sm text-muted-foreground">
              {pet.species} &middot; {pet.breed} &middot; {t('age', { years: pet.ageYears })}
            </p>
          </div>
        </div>

        {/* Share Records button */}
        <ShareRecordsModal animalId={pet.id} />
      </div>

      {/* Overview tab content */}
      <div className="bg-white rounded-xl border border-border/80 p-4 space-y-4" data-testid="animal-overview">
        <h2 className="text-sm font-semibold text-foreground">{t('overview')}</h2>
        <div className="grid grid-cols-2 gap-3 text-sm">
          <div>
            <p className="text-muted-foreground text-xs">{t('species_label')}</p>
            <p className="font-medium">{pet.species}</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">{t('breed_label')}</p>
            <p className="font-medium">{pet.breed}</p>
          </div>
          <div>
            <p className="text-muted-foreground text-xs">{t('age_label')}</p>
            <p className="font-medium">{t('age', { years: pet.ageYears })}</p>
          </div>
        </div>
      </div>

      {/* Active share links */}
      <ActiveSharesList />
    </div>
  )
}
