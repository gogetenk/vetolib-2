'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useParams } from 'next/navigation'
import Link from 'next/link'
import { PawPrint, ChevronRight } from 'lucide-react'
import { listPortalPets } from '@/lib/api/portal'
import type { PortalPetDto } from '@/lib/api/messaging-types'

export function PortalPetsList() {
  const t = useTranslations('portal.pets')
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const [pets, setPets] = useState<PortalPetDto[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    listPortalPets()
      .then(setPets)
      .catch(() => {
        // Handled by empty state
      })
      .finally(() => setLoading(false))
  }, [])

  return (
    <div className="space-y-5" data-testid="portal-pets-page">
      <h1 className="text-[22px] font-bold text-foreground" data-testid="portal-pets-title">
        {t('title')}
      </h1>

      {loading ? (
        <div className="space-y-3" data-testid="portal-pets-loading">
          {[1, 2].map(i => (
            <div key={i} className="h-20 rounded-xl bg-muted animate-pulse" />
          ))}
        </div>
      ) : pets.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground" data-testid="portal-pets-empty">
          <PawPrint className="w-10 h-10 mx-auto mb-2 text-muted-foreground/50" />
          <p className="text-[13px]">{t('no_pets')}</p>
        </div>
      ) : (
        <ul className="space-y-2" data-testid="portal-pets-list">
          {pets.map(pet => (
            <li key={pet.id}>
              <Link
                href={`/${params.locale}/portal/${params.clinicSlug}/pets/${pet.id}`}
                data-testid={`pet-item-${pet.id}`}
                className="flex items-center gap-3 bg-white rounded-xl border border-border/80 px-4 py-3 hover:border-primary/40 hover:shadow-sm transition-all"
              >
                <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                  <PawPrint className="h-5 w-5 text-primary" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-[14px] font-medium text-foreground" data-testid={`pet-name-${pet.id}`}>
                    {pet.name}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {pet.species} &middot; {pet.breed} &middot; {t('age', { years: pet.ageYears })}
                  </p>
                </div>
                <ChevronRight className="h-4 w-4 text-muted-foreground flex-shrink-0" />
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
