import { cn } from '@/lib/utils'

type LtrTextProps = React.HTMLAttributes<HTMLElement> & {
  children: React.ReactNode
  className?: string
  as?: 'span' | 'p' | 'div'
  'data-testid'?: string
}

/**
 * Wraps content that must always render left-to-right, even in RTL locales.
 * Use for: dates, phone numbers, email addresses, invoice numbers,
 * financial amounts, quantities with units.
 */
export function LtrText({ children, className, as: Tag = 'span', ...rest }: LtrTextProps) {
  return (
    <Tag dir="ltr" className={cn(className)} style={{ unicodeBidi: 'embed' }} {...rest}>
      {children}
    </Tag>
  )
}
