'use client'

import { useEffect, useRef, useState, useCallback } from 'react'

/**
 * Hook that triggers a CSS class when the element scrolls into view.
 * Uses Intersection Observer for performance.
 *
 * @param options - IntersectionObserver options
 * @returns [ref, isVisible] — attach ref to the element, isVisible becomes true once in viewport
 */
export function useScrollAnimation<T extends HTMLElement = HTMLDivElement>(
  options?: IntersectionObserverInit
): [React.RefObject<T | null>, boolean] {
  const ref = useRef<T | null>(null)
  const [isVisible, setIsVisible] = useState(false)

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
      { threshold: 0.1, ...options }
    )

    observer.observe(element)

    return () => observer.disconnect()
  }, [options])

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
