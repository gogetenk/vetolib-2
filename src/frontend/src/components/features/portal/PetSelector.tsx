'use client'

import { useTranslations } from 'next-intl'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { PortalPetDto } from '@/lib/api/messaging-types'

interface PetSelectorProps {
  pets: PortalPetDto[]
  value: string | null
  onChange: (petId: string | null) => void
}

export function PetSelector({ pets, value, onChange }: PetSelectorProps) {
  const t = useTranslations('portal.new_message')

  const selectedPet = pets.find((p) => p.id === value)

  return (
    <div className="flex flex-col gap-1">
      <label className="text-sm font-medium text-foreground">
        {t('pet_label')}
      </label>
      <div data-testid="pet-selector">
        <Select
          value={value ?? ''}
          onValueChange={(v) => onChange((v as string) === '' ? null : (v as string))}
        >
          <SelectTrigger className="w-full rounded-md border-border/80 text-sm shadow-sm">
            <SelectValue>
              {selectedPet ? `${selectedPet.name} — ${selectedPet.species} (${selectedPet.breed})` : t('no_pet')}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="">{t('no_pet')}</SelectItem>
            {pets.map((pet) => (
              <SelectItem key={pet.id} value={pet.id}>
                {pet.name} — {pet.species} ({pet.breed})
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  )
}
