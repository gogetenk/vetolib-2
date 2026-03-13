import { cn } from '@/lib/utils'
import { Dog, Cat, Bird, Rabbit, PawPrint } from 'lucide-react'

const SPECIES_CONFIG: Record<string, { icon: typeof Dog; color: string; bg: string }> = {
  dog: { icon: Dog, color: 'text-amber-700', bg: 'bg-amber-100' },
  cat: { icon: Cat, color: 'text-purple-600', bg: 'bg-purple-100' },
  bird: { icon: Bird, color: 'text-sky-600', bg: 'bg-sky-100' },
  rabbit: { icon: Rabbit, color: 'text-pink-600', bg: 'bg-pink-100' },
}

const DEFAULT_CONFIG = { icon: PawPrint, color: 'text-teal-600', bg: 'bg-teal-100' }

interface SpeciesIconProps {
  species: string
  size?: 'sm' | 'md' | 'lg'
  className?: string
  'data-testid'?: string
}

export function SpeciesIcon({ species, size = 'md', className, ...props }: SpeciesIconProps) {
  const config = SPECIES_CONFIG[species.toLowerCase()] ?? DEFAULT_CONFIG
  const Icon = config.icon
  const sizeClasses = {
    sm: 'h-6 w-6',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  }
  const iconSizes = {
    sm: 'h-3 w-3',
    md: 'h-4 w-4',
    lg: 'h-6 w-6',
  }
  return (
    <span
      className={cn('inline-flex items-center justify-center rounded-full', config.bg, config.color, sizeClasses[size], className)}
      data-testid={props['data-testid']}
    >
      <Icon className={iconSizes[size]} />
    </span>
  )
}
