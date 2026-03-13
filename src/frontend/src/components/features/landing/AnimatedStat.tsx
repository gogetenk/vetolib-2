'use client'

import { useEffect, useState, useRef } from 'react'
import { useScrollAnimation } from '@/hooks/use-scroll-animation'

interface AnimatedStatProps {
  /** The final text value, e.g. "250+", "99.9%", "AED 1.2M" */
  value: string
  /** Delay before starting the animation (ms) */
  delay?: number
  className?: string
  'data-testid'?: string
}

/**
 * Extracts the numeric portion of a stat string and animates it counting up.
 * Non-numeric prefixes/suffixes are preserved.
 */
export function AnimatedStat({
  value,
  delay = 0,
  className,
  ...props
}: AnimatedStatProps) {
  const [ref, isVisible] = useScrollAnimation<HTMLSpanElement>()
  const [displayValue, setDisplayValue] = useState(value)
  const hasAnimated = useRef(false)

  useEffect(() => {
    if (!isVisible || hasAnimated.current) return
    hasAnimated.current = true

    // Parse the numeric part from the value
    const match = value.match(/^([^\d]*)([\d,.]+)(.*)$/)
    if (!match) {
      setDisplayValue(value)
      return
    }

    const [, prefix, numStr, suffix] = match
    const cleanNum = numStr.replace(/,/g, '')
    const target = parseFloat(cleanNum)
    const hasDecimals = cleanNum.includes('.')
    const decimalPlaces = hasDecimals ? (cleanNum.split('.')[1]?.length ?? 0) : 0
    const hasCommas = numStr.includes(',')

    const duration = 1500
    const startTime = Date.now() + delay
    const fps = 30
    const interval = 1000 / fps

    function formatNumber(n: number): string {
      const formatted = hasDecimals ? n.toFixed(decimalPlaces) : Math.round(n).toString()
      if (hasCommas) {
        const parts = formatted.split('.')
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ',')
        return parts.join('.')
      }
      return formatted
    }

    const timer = setInterval(() => {
      const elapsed = Date.now() - startTime
      if (elapsed < 0) return

      const progress = Math.min(elapsed / duration, 1)
      // Ease-out cubic
      const eased = 1 - Math.pow(1 - progress, 3)
      const current = target * eased

      setDisplayValue(`${prefix}${formatNumber(current)}${suffix}`)

      if (progress >= 1) {
        clearInterval(timer)
        setDisplayValue(value) // Ensure exact final value
      }
    }, interval)

    return () => clearInterval(timer)
  }, [isVisible, value, delay])

  return (
    <span ref={ref} className={className} data-testid={props['data-testid']}>
      {displayValue}
    </span>
  )
}
