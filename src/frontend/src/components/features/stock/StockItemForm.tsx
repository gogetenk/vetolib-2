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
import type { StockItemDto, StockCategory } from '@/lib/api/stock'

const stockItemSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  category: z.enum(['Medication', 'Vaccine', 'Supply'] as [StockCategory, ...StockCategory[]]),
  quantity: z.coerce.number().int().min(0, 'Quantity must be >= 0'),
  unit: z.string().min(1, 'Unit is required'),
  threshold: z.coerce.number().int().min(1, 'Threshold must be >= 1'),
  expiryDate: z.string().optional(),
})

type StockItemFormValues = z.infer<typeof stockItemSchema>

interface StockItemFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  item?: StockItemDto
  onSubmit: (data: StockItemFormValues) => Promise<void>
}

export function StockItemForm({ open, onOpenChange, item, onSubmit }: StockItemFormProps) {
  const t = useTranslations('stock')
  const isEdit = !!item

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<StockItemFormValues>({
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    resolver: zodResolver(stockItemSchema) as any,
    defaultValues: item
      ? {
          name: item.name,
          category: item.category,
          quantity: item.quantity,
          unit: item.unit,
          threshold: item.threshold,
          expiryDate: item.expiryDate ?? '',
        }
      : { category: 'Medication' },
  })

  // eslint-disable-next-line react-hooks/incompatible-library
  const selectedCategory = watch('category')

  useEffect(() => {
    if (!open) return
    if (item) {
      reset({
        name: item.name,
        category: item.category,
        quantity: item.quantity,
        unit: item.unit,
        threshold: item.threshold,
        expiryDate: item.expiryDate ?? '',
      })
    } else {
      reset({ category: 'Medication' })
    }
  }, [open, item, reset])

  const categories: StockCategory[] = ['Medication', 'Vaccine', 'Supply']

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="rounded-2xl" data-testid="stock-item-dialog">
        <DialogHeader>
          <DialogTitle className="text-[18px] font-bold text-[#061e44]">
            {isEdit ? t('form.edit_title') : t('form.add_title')}
          </DialogTitle>
          <p className="text-[13px] text-muted-foreground">
            {isEdit ? 'Update the stock item details below.' : 'Fill in the details to add a new stock item.'}
          </p>
        </DialogHeader>

        <form
          onSubmit={handleSubmit(onSubmit)}
          className="space-y-4"
          data-testid="stock-item-form"
          noValidate
        >
          {/* Name */}
          <div className="space-y-1">
            <Label htmlFor="stock-name" className="text-[13px] font-semibold text-[#061e44]">
              {t('form.name')} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="stock-name"
              data-testid="input-stock-name"
              placeholder={t('form.name_placeholder')}
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
              {...register('name')}
            />
            {errors.name && (
              <p className="text-xs text-destructive" data-testid="error-stock-name">
                {errors.name.message}
              </p>
            )}
          </div>

          {/* Category */}
          <div className="space-y-1">
            <Label htmlFor="stock-category" className="text-[13px] font-semibold text-[#061e44]">
              {t('form.category')} <span className="text-destructive">*</span>
            </Label>
            <Select
              value={selectedCategory}
              onValueChange={(val) =>
                setValue('category', val as StockCategory, { shouldValidate: true })
              }
            >
              <SelectTrigger className="rounded-xl border-border/80 text-[13px]" data-testid="select-stock-category-trigger">
                <SelectValue placeholder={t('form.select_category')} />
              </SelectTrigger>
              <SelectContent>
                {categories.map(cat => (
                  <SelectItem
                    key={cat}
                    value={cat}
                    data-testid={`category-option-${cat.toLowerCase()}`}
                  >
                    {t(`categories.${cat.toLowerCase()}`)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.category && (
              <p className="text-xs text-destructive" data-testid="error-stock-category">
                {errors.category.message}
              </p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-3">
            {/* Quantity */}
            <div className="space-y-1">
              <Label htmlFor="stock-quantity" className="text-[13px] font-semibold text-[#061e44]">
                {t('form.quantity')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="stock-quantity"
                type="number"
                min={0}
                data-testid="input-stock-quantity"
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
                {...register('quantity')}
              />
              {errors.quantity && (
                <p className="text-xs text-destructive" data-testid="error-stock-quantity">
                  {errors.quantity.message}
                </p>
              )}
            </div>

            {/* Unit */}
            <div className="space-y-1">
              <Label htmlFor="stock-unit" className="text-[13px] font-semibold text-[#061e44]">
                {t('form.unit')} <span className="text-destructive">*</span>
              </Label>
              <Input
                id="stock-unit"
                data-testid="input-stock-unit"
                placeholder={t('form.unit_placeholder')}
                className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
                {...register('unit')}
              />
              {errors.unit && (
                <p className="text-xs text-destructive" data-testid="error-stock-unit">
                  {errors.unit.message}
                </p>
              )}
            </div>
          </div>

          {/* Threshold */}
          <div className="space-y-1">
            <Label htmlFor="stock-threshold" className="text-[13px] font-semibold text-[#061e44]">
              {t('form.threshold')} <span className="text-destructive">*</span>
            </Label>
            <Input
              id="stock-threshold"
              type="number"
              min={1}
              data-testid="input-stock-threshold"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
              {...register('threshold')}
            />
            {errors.threshold && (
              <p className="text-xs text-destructive" data-testid="error-stock-threshold">
                {errors.threshold.message}
              </p>
            )}
          </div>

          {/* Expiry date */}
          <div className="space-y-1">
            <Label htmlFor="stock-expiry" className="text-[13px] font-semibold text-[#061e44]">
              {t('form.expiry_date')}{' '}
              <span className="text-muted-foreground">{t('form.optional')}</span>
            </Label>
            <Input
              id="stock-expiry"
              type="date"
              data-testid="input-stock-expiry"
              className="rounded-xl border-border/80 text-[13px] focus:ring-2 focus:ring-[#303ef5]/20 focus:border-[#303ef5]/50"
              {...register('expiryDate')}
            />
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button
              type="button"
              variant="outline"
              data-testid="btn-cancel-stock-item"
              onClick={() => onOpenChange(false)}
              className="rounded-xl h-10 px-5 font-semibold border-border/80 hover:bg-[#f4f6f9]"
            >
              {t('form.cancel')}
            </Button>
            <Button
              type="submit"
              disabled={isSubmitting}
              data-testid="btn-save-stock-item"
              className="bg-[#303ef5] hover:bg-[#2530c4] text-white font-semibold rounded-xl h-10 px-5 shadow-sm"
            >
              {isSubmitting ? t('form.saving') : t('form.save')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
