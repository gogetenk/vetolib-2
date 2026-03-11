'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useTranslations } from 'next-intl'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { formatAED } from '@/lib/utils'
import { createInvoice } from '@/lib/api/billing'
import type { CreateInvoiceLineItem } from '@/lib/api/billing'
import { trackEvent, AnalyticsEvents, bucketAed } from '@/lib/analytics'

interface LineItem {
  description: string
  quantity: number
  unitPrice: number
}

const MOCK_PATIENTS = [
  { id: 'pat-0000-0000-0000-000000000001', name: 'Max', ownerName: 'Ahmed Al-Rashid', ownerPhone: '+971 50 123 4567' },
  { id: 'pat-0000-0000-0000-000000000002', name: 'Luna', ownerName: 'Fatima Hassan', ownerPhone: '+971 55 987 6543' },
  { id: 'pat-0000-0000-0000-000000000003', name: 'Rocky', ownerName: 'Mohammed Al-Zaabi', ownerPhone: '+971 54 321 0987' },
  { id: 'pat-0000-0000-0000-000000000004', name: 'Bella', ownerName: 'Sara Al-Mansoori', ownerPhone: '+971 52 456 7890' },
]

export function InvoiceForm() {
  const router = useRouter()
  const t = useTranslations('billing.form')
  const [selectedPatientId, setSelectedPatientId] = useState('')
  const [notes, setNotes] = useState('')
  const [items, setItems] = useState<LineItem[]>([
    { description: '', quantity: 1, unitPrice: 0 },
  ])
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [patientSearch, setPatientSearch] = useState('')

  const selectedPatient = MOCK_PATIENTS.find((p) => p.id === selectedPatientId)

  const filteredPatients = MOCK_PATIENTS.filter((p) =>
    p.name.toLowerCase().includes(patientSearch.toLowerCase()) ||
    p.ownerName.toLowerCase().includes(patientSearch.toLowerCase())
  )

  const subtotal = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0)
  const vatAmount = Math.round(subtotal * 0.05 * 100) / 100
  const total = Math.round((subtotal + vatAmount) * 100) / 100

  function addItem() {
    setItems((prev) => [...prev, { description: '', quantity: 1, unitPrice: 0 }])
  }

  function removeItem(index: number) {
    setItems((prev) => prev.filter((_, i) => i !== index))
  }

  function updateItem(index: number, field: keyof LineItem, value: string | number) {
    setItems((prev) =>
      prev.map((item, i) =>
        i === index ? { ...item, [field]: field === 'description' ? value : Number(value) } : item
      )
    )
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!selectedPatient) {
      setError(t('errors.select_patient'))
      return
    }
    if (items.length === 0 || items.some((item) => !item.description)) {
      setError(t('errors.add_item_description'))
      return
    }

    setSubmitting(true)
    setError(null)

    try {
      const invoice = await createInvoice({
        patientId: selectedPatient.id,
        patientName: selectedPatient.name,
        ownerName: selectedPatient.ownerName,
        ownerPhone: selectedPatient.ownerPhone,
        items: items as CreateInvoiceLineItem[],
        notes: notes || null,
      })
      trackEvent(AnalyticsEvents.INVOICE_CREATED, {
        item_count: String(items.length),
        total_aed: bucketAed(total),
      })
      router.push(`/billing/${invoice.id}`)
    } catch {
      setError(t('errors.create_failed'))
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} data-testid="invoice-form">
      <div className="space-y-6">
        {/* Patient Selection */}
        <Card>
          <CardHeader>
            <CardTitle>{t('patient_section')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div>
              <Label htmlFor="patient-search">{t('search_patient')}</Label>
              <Input
                id="patient-search"
                data-testid="patient-search-input"
                placeholder={t('search_patient_placeholder')}
                value={patientSearch}
                onChange={(e) => setPatientSearch(e.target.value)}
              />
              {patientSearch && !selectedPatient && (
                <div
                  className="mt-1 rounded-md border bg-popover shadow-md"
                  data-testid="patient-dropdown"
                >
                  {filteredPatients.map((p) => (
                    <button
                      key={p.id}
                      type="button"
                      data-testid={`patient-option-${p.id}`}
                      className="w-full px-3 py-2 text-left text-sm hover:bg-accent"
                      onClick={() => {
                        setSelectedPatientId(p.id)
                        setPatientSearch(p.name)
                      }}
                    >
                      <span className="font-medium">{p.name}</span>{' '}
                      <span className="text-muted-foreground">— {p.ownerName}</span>
                    </button>
                  ))}
                  {filteredPatients.length === 0 && (
                    <p className="px-3 py-2 text-sm text-muted-foreground">{t('no_patients_found')}</p>
                  )}
                </div>
              )}
            </div>
            {selectedPatient && (
              <div className="rounded-md bg-muted px-3 py-2 text-sm" data-testid="selected-patient">
                <p className="font-medium">{selectedPatient.name}</p>
                <p className="text-muted-foreground">
                  {t('owner_label')}: {selectedPatient.ownerName} — {selectedPatient.ownerPhone}
                </p>
                <button
                  type="button"
                  data-testid="clear-patient-btn"
                  className="text-xs text-primary underline mt-1"
                  onClick={() => {
                    setSelectedPatientId('')
                    setPatientSearch('')
                  }}
                >
                  {t('change_patient')}
                </button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Line Items */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle>{t('items_section')}</CardTitle>
              <Button
                type="button"
                variant="outline"
                size="sm"
                data-testid="add-item-btn"
                onClick={addItem}
              >
                {t('add_item')}
              </Button>
            </div>
          </CardHeader>
          <CardContent className="space-y-3">
            {items.map((item, index) => (
              <div key={index} className="grid grid-cols-12 gap-2 items-end" data-testid={`line-item-${index}`}>
                <div className="col-span-5">
                  {index === 0 && <Label>{t('description')}</Label>}
                  <Input
                    data-testid={`item-description-${index}`}
                    placeholder={t('description_placeholder')}
                    aria-label={`Item ${index + 1} description`}
                    value={item.description}
                    onChange={(e) => updateItem(index, 'description', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-2">
                  {index === 0 && <Label>{t('qty')}</Label>}
                  <Input
                    data-testid={`item-quantity-${index}`}
                    type="number"
                    min={1}
                    aria-label={`Item ${index + 1} quantity`}
                    value={item.quantity}
                    onChange={(e) => updateItem(index, 'quantity', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-3">
                  {index === 0 && <Label>{t('unit_price')}</Label>}
                  <Input
                    data-testid={`item-unit-price-${index}`}
                    type="number"
                    min={0}
                    step={0.01}
                    aria-label={`Item ${index + 1} unit price in AED`}
                    value={item.unitPrice}
                    onChange={(e) => updateItem(index, 'unitPrice', e.target.value)}
                    required
                  />
                </div>
                <div className="col-span-1 text-right text-sm font-medium" data-testid={`item-subtotal-${index}`}>
                  {formatAED(item.quantity * item.unitPrice)}
                </div>
                <div className="col-span-1 flex justify-end">
                  {items.length > 1 && (
                    <button
                      type="button"
                      data-testid={`remove-item-${index}`}
                      aria-label={`Remove item ${index + 1}`}
                      className="text-destructive text-lg leading-none"
                      onClick={() => removeItem(index)}
                    >
                      <span aria-hidden="true">×</span>
                    </button>
                  )}
                </div>
              </div>
            ))}

            {/* Totals */}
            <div className="mt-4 border-t pt-4 space-y-1 text-sm" data-testid="invoice-totals">
              <div className="flex justify-between">
                <span>{t('subtotal_excl_vat')}</span>
                <span data-testid="form-subtotal">{formatAED(subtotal)}</span>
              </div>
              <div className="flex justify-between text-muted-foreground">
                <span>{t('vat_percent')}</span>
                <span data-testid="form-vat">{formatAED(vatAmount)}</span>
              </div>
              <div className="flex justify-between font-bold text-base border-t pt-1">
                <span>{t('total_aed')}</span>
                <span data-testid="form-total">{formatAED(total)}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Notes */}
        <Card>
          <CardHeader>
            <CardTitle>{t('notes_section')}</CardTitle>
          </CardHeader>
          <CardContent>
            <Textarea
              data-testid="invoice-notes"
              className="resize-y"
              placeholder={t('notes_placeholder')}
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </CardContent>
        </Card>

        {error && (
          <p className="text-sm text-destructive" data-testid="form-error">
            {error}
          </p>
        )}

        <div className="flex gap-3 justify-end">
          <Button
            type="button"
            variant="outline"
            data-testid="cancel-form-btn"
            onClick={() => router.push('/billing')}
          >
            {t('cancel')}
          </Button>
          <Button
            type="submit"
            data-testid="submit-invoice-btn"
            disabled={submitting}
          >
            {submitting ? t('creating') : t('create_invoice')}
          </Button>
        </div>
      </div>
    </form>
  )
}
