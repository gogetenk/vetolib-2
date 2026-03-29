'use client'

import { Dog, Cat, Bird, Rabbit, PawPrint } from 'lucide-react'
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
      return <PawPrint {...iconProps} />
    case 'Camel':
      return <PawPrint {...iconProps} />
    case 'Falcon':
      return <Bird {...iconProps} />
    case 'Reptile':
      return <PawPrint {...iconProps} />
    default:
      return <PawPrint {...iconProps} />
  }
}

export interface SpeciesColorSet {
  border: string
  bg: string
  text: string
}

export function getSpeciesColor(species: Species): SpeciesColorSet {
  switch (species) {
    case 'Dog':
      return { border: 'border-amber-500', bg: 'bg-amber-50', text: 'text-amber-600' }
    case 'Cat':
      return { border: 'border-purple-500', bg: 'bg-purple-50', text: 'text-purple-600' }
    case 'Bird':
      return { border: 'border-sky-500', bg: 'bg-sky-50', text: 'text-sky-600' }
    case 'Rabbit':
      return { border: 'border-pink-500', bg: 'bg-pink-50', text: 'text-pink-600' }
    case 'Horse':
      return { border: 'border-teal-500', bg: 'bg-teal-50', text: 'text-teal-600' }
    case 'Camel':
      return { border: 'border-orange-500', bg: 'bg-orange-50', text: 'text-orange-600' }
    case 'Falcon':
      return { border: 'border-indigo-500', bg: 'bg-indigo-50', text: 'text-indigo-600' }
    case 'Reptile':
      return { border: 'border-emerald-500', bg: 'bg-emerald-50', text: 'text-emerald-600' }
    default:
      return { border: 'border-stone-400', bg: 'bg-stone-50', text: 'text-stone-500' }
  }
}

export const SPECIES_LABELS: Record<Species, string> = {
  Dog: 'Dog',
  Cat: 'Cat',
  Bird: 'Bird',
  Rabbit: 'Rabbit',
  Horse: 'Horse',
  Camel: 'Camel',
  Exotic: 'Exotic',
  Falcon: 'Falcon',
  Reptile: 'Reptile',
}

export const ALL_SPECIES: Species[] = ['Dog', 'Cat', 'Bird', 'Rabbit', 'Horse', 'Camel', 'Exotic', 'Falcon', 'Reptile']
