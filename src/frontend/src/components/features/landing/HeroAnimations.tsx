'use client'

import { useEffect, useState } from 'react'
import { cn } from '@/lib/utils'

interface HeroAnimationsProps {
  children: React.ReactNode
  /** Stagger index: 0 = first, 1 = second, etc. Each has 150ms more delay */
  index: number
  className?: string
}

/**
 * Wraps hero elements with staggered fade-in + slide-up animation on page load.
 */
export function HeroStagger({ children, index, className }: HeroAnimationsProps) {
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    // Small RAF delay to ensure the initial hidden state is painted first
    const raf = requestAnimationFrame(() => {
      setMounted(true)
    })
    return () => cancelAnimationFrame(raf)
  }, [])

  return (
    <div
      className={cn(
        'transition-all ease-out',
        mounted
          ? 'translate-y-0 opacity-100'
          : 'translate-y-6 opacity-0',
        className
      )}
      style={{
        transitionDuration: '800ms',
        transitionDelay: `${150 + index * 150}ms`,
      }}
    >
      {children}
    </div>
  )
}

/**
 * Animated dashboard image — fades in and slides up with a slight scale.
 */
export function HeroDashboardReveal({
  children,
  className,
}: {
  children: React.ReactNode
  className?: string
}) {
  const [mounted, setMounted] = useState(false)

  useEffect(() => {
    const raf = requestAnimationFrame(() => {
      setMounted(true)
    })
    return () => cancelAnimationFrame(raf)
  }, [])

  return (
    <div
      className={cn(
        'transition-all ease-out',
        mounted
          ? 'translate-y-0 opacity-100 scale-100'
          : 'translate-y-10 opacity-0 scale-[0.97]',
        className
      )}
      style={{
        transitionDuration: '1000ms',
        transitionDelay: '600ms',
      }}
    >
      {children}
    </div>
  )
}
