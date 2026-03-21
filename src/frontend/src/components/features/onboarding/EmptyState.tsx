'use client'

import { ReactNode } from 'react'
import Link from 'next/link'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface CtaConfig {
  label: string
  href?: string
  onClick?: () => void
  'data-testid'?: string
}

interface EmptyStateProps {
  icon: ReactNode
  title: string
  description: string
  primaryCta: CtaConfig
  secondaryCta?: CtaConfig
  tip?: string
  'data-testid-prefix': string
  className?: string
}

export function EmptyState({
  icon,
  title,
  description,
  primaryCta,
  secondaryCta,
  tip,
  'data-testid-prefix': prefix,
  className,
}: EmptyStateProps) {
  const primaryButton = (
    <Button
      data-testid={`empty-state-cta-${prefix}`}
      onClick={primaryCta.onClick}
      className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl shadow-sm"
    >
      {primaryCta.label}
    </Button>
  )

  return (
    <div
      data-testid={`empty-state-${prefix}`}
      className={cn(
        'flex flex-col items-center justify-center py-16 px-6 text-center gap-4',
        className
      )}
    >
      <div className="text-muted-foreground/60" aria-hidden="true">
        {icon}
      </div>

      <div className="space-y-2 max-w-sm">
        <h2 className="text-[18px] font-bold tracking-tight text-foreground">{title}</h2>
        <p className="text-muted-foreground text-[13px]">{description}</p>
      </div>

      <div className="flex flex-wrap items-center justify-center gap-3 mt-2">
        {primaryCta.href ? (
          <Link href={primaryCta.href}>
            {primaryButton}
          </Link>
        ) : (
          primaryButton
        )}

        {secondaryCta && (
          secondaryCta.href ? (
            <Link href={secondaryCta.href}>
              <Button
                variant="outline"
                data-testid={secondaryCta['data-testid'] ?? `empty-state-cta-secondary-${prefix}`}
                className="border-border/80 hover:bg-muted rounded-xl font-semibold"
              >
                {secondaryCta.label}
              </Button>
            </Link>
          ) : (
            <Button
              variant="outline"
              onClick={secondaryCta.onClick}
              data-testid={secondaryCta['data-testid'] ?? `empty-state-cta-secondary-${prefix}`}
              className="border-border/80 hover:bg-muted rounded-xl font-semibold"
            >
              {secondaryCta.label}
            </Button>
          )
        )}
      </div>

      {tip && (
        <p
          data-testid={`empty-state-tip-${prefix}`}
          className="text-[13px] text-muted-foreground italic max-w-sm mt-1"
        >
          {tip}
        </p>
      )}
    </div>
  )
}
