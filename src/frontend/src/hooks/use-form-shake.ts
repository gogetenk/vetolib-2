import { useState, useCallback } from 'react'

/**
 * Hook that encapsulates the form shake animation pattern.
 * Returns a boolean for the CSS class and a trigger function.
 */
export function useFormShake() {
  const [shakeForm, setShakeForm] = useState(false)

  const triggerShake = useCallback(() => {
    setShakeForm(true)
    setTimeout(() => setShakeForm(false), 500)
  }, [])

  return { shakeForm, triggerShake } as const
}
