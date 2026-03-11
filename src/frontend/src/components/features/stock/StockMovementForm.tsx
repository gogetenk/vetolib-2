'use client'

import { useEffect } from 'react'
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

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<MovementFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(buildSchema(item?.quantity ?? 0)) as any,
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
      <DialogContent data-testid="stock-movement-dialog">
        <DialogHeader>
          <DialogTitle>
            {t('movement.title')} — {item.name}
          </DialogTitle>
        </DialogHeader>

        <p className="text-sm text-muted-foreground" data-testid="movement-current-qty">
          {t('movement.current_qty', { qty: item.quantity, unit: item.unit })}
        </p>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
          data-testid="stock-movement-form"
          noValidate
        >
          {/* Type */}
          <div className="space-y-1">
            <Label htmlFor="movement-type">
              {t('movement.type')} <span className="text-destructive">*</span>
            </Label>
            <Select
              value={selectedType}
              onValueChange={(val) =>
                setValue('type', val as MovementType, { shouldValidate: true })
              }
            >
              <SelectTrigger data-testid="select-movement-type-trigger">
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
            <Label htmlFor="movement-quantity">
              {t('movement.quantity')} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="movement-quantity"
              type="number"
              min={1}
              data-testid="input-movement-quantity"
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
            <Label htmlFor="movement-reason">
              {t('movement.reason')}{' '}
              <span className="text-muted-foreground">{t('form.optional')}</span>
            </Label>
            <Input
              id="movement-reason"
              data-testid="input-movement-reason"
              placeholder={t('movement.reason_placeholder')}
              {...register('reason')}
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-movement"
              onClick={() => onOpenChange(false)}
            >
              {t('form.cancel')}
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="btn-save-movement"
            >
              {isSubmitting ? t('form.saving') : t('movement.confirm')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
