'use client'

import { useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { zodResolver } from '@hookform/resolvers/zod'
import { useTranslations } from 'next-intl'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import type { StockItemDto, MovementType } from '@/lib/api/stock'

function buildSchema(maxOut: number) {
  return z
    .object({
      type: z.enum(['IN', 'OUT', 'ADJUSTMENT'] as [MovementType, ...MovementType[]]),
      quantity: z.coerce.number().int().min(1, 'Quantity must be >= 1'),
      reason: z.string().optional(),
    })
    .superRefine((data, ctx) => {
      if (data.type === 'OUT' && data.quantity > maxOut) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: `Cannot remove more than ${maxOut} (current stock)`,
          path: ['quantity'],
        })
      }
    })
}

type MovementFormValues = z.infer<ReturnType<typeof buildSchema>>

interface StockMovementFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  item: StockItemDto | null
  onSubmit: (data: MovementFormValues) => Promise<void>
}

export function StockMovementForm({ open, onOpenChange, item, onSubmit }: StockMovementFormProps) {
  const t = useTranslations('stock')

  const currentQuantity = item?.quantity ?? 0
  const schema = useMemo(() => buildSchema(currentQuantity), [currentQuantity])

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<MovementFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(schema) as any,
    defaultValues: { type: 'IN' },
  })

  // eslint-disable-next-line react-hooks/incompatible-library
  const selectedType = watch('type')

  useEffect(() => {
    if (open) reset({ type: 'IN' })
  }, [open, reset])

  if (!item) return null

  const movementTypes: MovementType[] = ['IN', 'OUT', 'ADJUSTMENT']

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="rounded-2xl" data-testid="stock-movement-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-foreground">
            {t('movement.title')} — {item.name}
          </DialogTitle>
        </DialogHeader>

        <div className="rounded-xl bg-muted border border-border/50 px-4 py-3 flex items-center justify-between" data-testid="movement-current-qty">
          <span className="text-[13px] text-muted-foreground">{t('movement.current_qty', { qty: item.quantity, unit: item.unit })}</span>
          <span className="text-[15px] font-bold text-foreground tabular-nums">{item.quantity} {item.unit}</span>
        </div>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
          data-testid="stock-movement-form"
          noValidate
        >
          {/* Type */}
          <div className="space-y-1">
            <Label htmlFor="movement-type" className="text-[13px] font-semibold text-foreground">
              {t('movement.type')} <span className="text-destructive">*</span>
            </Label>
            <Select
              value={selectedType}
              onValueChange={(val) =>
                setValue('type', val as MovementType, { shouldValidate: true })
              }
            >
              <SelectTrigger className="rounded-xl border-border/80 text-base sm:text-[13px]" data-testid="select-movement-type-trigger">
                <SelectValue placeholder={t('movement.select_type')} />
              </SelectTrigger>
              <SelectContent>
                {movementTypes.map(mt => (
                  <SelectItem
                    key={mt}
                    value={mt}
                    data-testid={`movement-type-${mt.toLowerCase()}`}
                  >
                    {t(`movement.types.${mt.toLowerCase()}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Quantity */}
          <div className="space-y-1">
            <Label htmlFor="movement-quantity" className="text-[13px] font-semibold text-foreground">
              {t('movement.quantity')} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="movement-quantity"
              type="number"
              min={1}
              data-testid="input-movement-quantity"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              {...register('quantity')}
            />
            {errors.quantity && (
              <p className="text-xs text-destructive" data-testid="error-movement-quantity">
                {errors.quantity.message}
              </p>
            )}
          </div>

          {/* Reason */}
          <div className="space-y-1">
            <Label htmlFor="movement-reason" className="text-[13px] font-semibold text-foreground">
              {t('movement.reason')}{' '}
              <span className="text-muted-foreground">{t('form.optional')}</span>
            </Label>
            <Input
              id="movement-reason"
              data-testid="input-movement-reason"
              placeholder={t('movement.reason_placeholder')}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-primary/20 focus:border-primary/50"
              {...register('reason')}
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-movement"
              onClick={() => onOpenChange(false)}
              className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-muted"
            >
              {t('form.cancel')}
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="btn-save-movement"
              className="bg-primary hover:bg-primary/90 text-primary-foreground font-semibold rounded-xl h-10 px-5 shadow-sm"
            >
              {isSubmitting ? t('form.saving') : t('movement.confirm')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
