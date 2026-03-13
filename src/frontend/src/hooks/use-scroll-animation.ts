'use client'

import { useEffect, useRef, useState, useCallback } from 'react'
import type { RefObject } from 'react'

/**
 * Hook that triggers a CSS class when the element scrolls into view.
 * Uses Intersection Observer for performance.
 *
 * @param options - IntersectionObserver options
 * @returns [ref, isVisible] — attach ref to the element, isVisible becomes true once in viewport
 */
export function useScrollAnimation<T extends HTMLElement = HTMLDivElement>(
  options?: IntersectionObserverInit
): [RefObject<T | null>, boolean] {
  const ref = useRef<T | null>(null)
  const [isVisible, setIsVisible] = useState(false)

  // Stabilize options to avoid recreating the observer on every render when
  // callers pass an inline object literal.
  const threshold = options?.threshold
  const rootMargin = options?.rootMargin
  const root = options?.root

  useEffect(() => {
    const element = ref.current
    if (!element) return

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true)
          observer.unobserve(element)
        }
      },
      {
        threshold: threshold ?? 0.1,
        ...(rootMargin !== undefined && { rootMargin }),
        ...(root !== undefined && { root }),
      }
    )

    observer.observe(element)

    return () => observer.disconnect()
  }, [threshold, rootMargin, root])

  return [ref, isVisible]
}

/**
 * Hook for staggered children animations.
 * Returns a function to get delay style for each child index.
 */
export function useStaggerDelay(baseDelayMs: number = 100) {
  const getDelay = useCallback(
    (index: number) => ({
      transitionDelay: `${index * baseDelayMs}ms`,
      animationDelay: `${index * baseDelayMs}ms`,
    }),
    [baseDelayMs]
  )

  return getDelay
}
