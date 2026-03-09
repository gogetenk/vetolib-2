'use client'

import { Dog, Cat, Bird, Rabbit, Beef, PawPrint } from 'lucide-react'
import type { Species } from '@/lib/api/patients'

interface SpeciesIconProps {
  species: Species
  className?: string
  'aria-label'?: string
}

export function SpeciesIcon({ species, className = 'h-5 w-5', ...props }: SpeciesIconProps) {
  const ariaLabel = props['aria-label'] ?? species
  const iconProps = { className, 'aria-label': ariaLabel }
  switch (species) {
    case 'Dog':
      return <Dog {...iconProps} />
    case 'Cat':
      return <Cat {...iconProps} />
    case 'Bird':
      return <Bird {...iconProps} />
    case 'Rabbit':
      return <Rabbit {...iconProps} />
    case 'Horse':
      return <Beef {...iconProps} />
    case 'Camel':
      return <PawPrint {...iconProps} />
    default:
      return <PawPrint {...iconProps} />
  }
}

export const SPECIES_LABELS: Record<Species, string> = {
  Dog: '🐕 Dog',
  Cat: '🐈 Cat',
  Bird: '🐦 Bird',
  Rabbit: '🐇 Rabbit',
  Horse: '🐴 Horse',
  Camel: '🐪 Camel',
  Exotic: '⭐ Exotic',
}

export const ALL_SPECIES: Species[] = ['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Camel', 'Exotic']
