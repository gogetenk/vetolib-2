'use client'

import { useTranslations } from 'next-intl'
import type { PortalPetDto } from '@/lib/api/messaging-types'

interface PetSelectorProps {
  pets: PortalPetDto[]
  value: string | null
  onChange: (petId: string | null) => void
}

export function PetSelector({ pets, value, onChange }: PetSelectorProps) {
  const t = useTranslations('portal.new_message')

  return (
    <div className="flex flex-col gap-1">
      <label htmlFor="pet-selector" className="text-sm font-medium text-gray-700">
        {t('pet_label')}
      </label>
      <select
        id="pet-selector"
        data-testid="pet-selector"
        value={value ?? ''}
        onChange={(e) => onChange(e.target.value === '' ? null : e.target.value)}
        className="block w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm shadow-sm focus:border-emerald-500 focus:outline-none focus:ring-1 focus:ring-emerald-500"
      >
        <option value="">{t('no_pet')}</option>
        {pets.map((pet) => (
          <option key={pet.id} value={pet.id}>
            {pet.name} — {pet.species} ({pet.breed})
          </option>
        ))}
      </select>
    </div>
  )
}
