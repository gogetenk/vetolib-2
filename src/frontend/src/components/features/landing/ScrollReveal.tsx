'use client'

import { useScrollAnimation } from '@/hooks/use-scroll-animation'
import { cn } from '@/lib/utils'

interface ScrollRevealProps {
  children: React.ReactNode
  className?: string
  /** Animation direction: fade-up (default), fade-in, fade-left, fade-right */
  direction?: 'fade-up' | 'fade-in' | 'fade-left' | 'fade-right'
  /** Delay in ms before animation starts */
  delay?: number
  /** Duration in ms */
  duration?: number
  /** data-testid pass-through */
  'data-testid'?: string
}

const DIRECTION_CLASSES = {
  'fade-up': {
    hidden: 'translate-y-8 opacity-0',
    visible: 'translate-y-0 opacity-100',
  },
  'fade-in': {
    hidden: 'opacity-0',
    visible: 'opacity-100',
  },
  'fade-left': {
    hidden: 'translate-x-8 opacity-0',
    visible: 'translate-x-0 opacity-100',
  },
  'fade-right': {
    hidden: '-translate-x-8 opacity-0',
    visible: 'translate-x-0 opacity-100',
  },
} as const

export function ScrollReveal({
  children,
  className,
  direction = 'fade-up',
  delay = 0,
  duration = 700,
  ...props
}: ScrollRevealProps) {
  const [ref, isVisible] = useScrollAnimation<HTMLDivElement>()
  const { hidden, visible } = DIRECTION_CLASSES[direction]

  return (
    <div
      ref={ref}
      data-testid={props['data-testid']}
      className={cn(
        'transition-all ease-out',
        isVisible ? visible : hidden,
        className
      )}
      style={{
        transitionDuration: `${duration}ms`,
        transitionDelay: `${delay}ms`,
      }}
    >
      {children}
    </div>
  )
}
