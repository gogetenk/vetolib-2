'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter, useParams } from 'next/navigation'
import { PawPrint, ChevronRight, Dog, Cat } from 'lucide-react'
import { listMyAnimals } from '@/lib/api/portal'
import type { PortalAnimalDto } from '@/lib/api/portal'
import { ApiError } from '@/lib/api/client'

function SpeciesIcon({ species }: { species: string }) {
  if (species.toLowerCase() === 'dog') {
    return <Dog className="h-6 w-6 text-primary" />
  }
  if (species.toLowerCase() === 'cat') {
    return <Cat className="h-6 w-6 text-primary" />
  }
  return <PawPrint className="h-6 w-6 text-primary" />
}

export default function MyPetsPage() {
  const t = useTranslations('portal.my_pets')
  const params = useParams<{ locale: string; clinicSlug: string }>()
  const router = useRouter()
  const [animals, setAnimals] = useState<PortalAnimalDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    async function load() {
      try {
        const data = await listMyAnimals()
        if (!cancelled) {
          setAnimals(data)
          setIsLoading(false)
        }
      } catch (err) {
        if (!cancelled) {
          if (err instanceof ApiError && err.status === 401) {
            setError('expired')
          } else {
            setError('generic')
          }
          setIsLoading(false)
        }
      }
    }
    load()
    return () => { cancelled = true }
  }, [])

  if (isLoading) {
    return (
      <div className="text-center py-12 text-muted-foreground" data-testid="my-pets-loading">
        {t('loading')}
      </div>
    )
  }

  if (error === 'expired') {
    return (
      <div className="text-center py-12 text-destructive" data-testid="my-pets-expired">
        {t('link_expired')}
      </div>
    )
  }

  if (error) {
    return (
      <div className="text-center py-12 text-destructive" data-testid="my-pets-error">
        {t('error_generic')}
      </div>
    )
  }

  function formatDate(dateStr: string | null): string {
    if (!dateStr) return t('no_visits')
    return new Date(dateStr).toLocaleDateString('en-AE', {
      timeZone: 'Asia/Dubai',
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    })
  }

  return (
    <div data-testid="my-pets-page">
      <h1 className="text-xl font-bold text-foreground mb-4" data-testid="my-pets-title">
        {t('title')}
      </h1>

      {animals.length === 0 ? (
        <div className="text-center py-12 text-muted-foreground" data-testid="my-pets-empty">
          {t('no_animals')}
        </div>
      ) : (
        <div className="space-y-3" data-testid="my-pets-list">
          {animals.map((animal) => (
            <button
              key={animal.id}
              onClick={() =>
                router.push(
                  `/${params.locale}/portal/${params.clinicSlug}/animals/${animal.id}`
                )
              }
              data-testid={`animal-card-${animal.id}`}
              className="w-full flex items-center gap-3 rounded-xl border border-border/80 bg-white p-4 hover:bg-muted/50 transition-colors text-start"
            >
              <div className="flex-shrink-0 w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
                <SpeciesIcon species={animal.species} />
              </div>
              <div className="flex-1 min-w-0">
                <p className="font-semibold text-foreground truncate" data-testid={`animal-name-${animal.id}`}>
                  {animal.name}
                </p>
                <p className="text-sm text-muted-foreground truncate">
                  {animal.breed} &middot; {animal.species}
                </p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {t('last_visit')}: {formatDate(animal.lastVisitDate)}
                </p>
              </div>
              <ChevronRight className="h-5 w-5 text-muted-foreground flex-shrink-0" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
