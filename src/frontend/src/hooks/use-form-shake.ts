import { useState, useCallback, useRef, useEffect } from 'react'

/**
 * Hook that encapsulates the form shake animation pattern.
 * Returns a boolean for the CSS class and a trigger function.
 */
export function useFormShake() {
  const [shakeForm, setShakeForm] = useState(false)
  const timerRef = useRef<NodeJS.Timeout | null>(null)

  const triggerShake = useCallback(() => {
    if (timerRef.current) {
      clearTimeout(timerRef.current)
    }
    setShakeForm(true)
    timerRef.current = setTimeout(() => {
      setShakeForm(false)
      timerRef.current = null
    }, 500)
  }, [])

  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current)
      }
    }
  }, [])

  return { shakeForm, triggerShake } as const
}
