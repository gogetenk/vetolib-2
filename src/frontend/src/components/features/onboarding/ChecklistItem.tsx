'use client'

import { Check, ChevronRight, Circle } from 'lucide-react'
import { useRouter } from 'next/navigation'
import type { OnboardingStepDto } from '@/lib/api/types'

interface ChecklistItemProps {
  step: OnboardingStepDto & { href: string }
  onComplete: (stepId: string) => Promise<void>
}

export function ChecklistItem({ step, onComplete }: ChecklistItemProps) {
  const router = useRouter()

  async function handleClick() {
    if (!step.completed) {
      try {
        await onComplete(step.id)
      } catch {
        // Fail silently — navigation happens regardless
      }
    }
    router.push(step.href)
  }

  return (
    <button
      type="button"
      data-testid={`checklist-item-${step.id}`}
      onClick={handleClick}
      className={[
        'group flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-start transition-colors',
        step.completed
          ? 'cursor-default text-muted-foreground hover:bg-transparent'
          : 'hover:bg-accent',
      ].join(' ')}
      aria-disabled={step.completed}
    >
      {/* Status icon */}
      <span
        data-testid={`checklist-item-check-${step.id}`}
        className={[
          'flex h-5 w-5 shrink-0 items-center justify-center rounded-full border-2 transition-all duration-300',
          step.completed
            ? 'scale-110 border-green-500 bg-green-500 opacity-100'
            : 'border-gray-300 bg-white opacity-100 group-hover:border-primary',
        ].join(' ')}
        aria-hidden="true"
      >
        {step.completed ? (
          <Check className="h-3 w-3 text-white" strokeWidth={3} />
        ) : (
          <Circle className="h-2 w-2 text-transparent" />
        )}
      </span>

      {/* Label */}
      <span
        className={[
          'flex-1 text-sm font-medium transition-all',
          step.completed ? 'line-through opacity-60' : '',
        ].join(' ')}
      >
        {step.title}
      </span>

      {/* Navigation arrow — only for pending steps */}
      {!step.completed && (
        <ChevronRight
          className="h-4 w-4 shrink-0 text-muted-foreground transition-transform group-hover:translate-x-0.5"
          aria-hidden="true"
        />
      )}
    </button>
  )
}
