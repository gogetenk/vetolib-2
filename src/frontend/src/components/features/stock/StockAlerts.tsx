'use client'

import { AlertTriangle, Clock } from 'lucide-react'
import { useTranslations } from 'next-intl'
import type { StockAlertDto } from '@/lib/api/stock'

interface StockAlertsProps {
  alerts: StockAlertDto[]
}

export function StockAlerts({ alerts }: StockAlertsProps) {
  const t = useTranslations('stock')

  const lowStockAlerts = alerts.filter(a => a.alertType === 'low-stock')
  const expiringAlerts = alerts.filter(a => a.alertType === 'expiring-soon')

  if (alerts.length === 0) return null

  return (
    <div className="space-y-2" data-testid="stock-alerts">
      {lowStockAlerts.length > 0 && (
        <div
          className="flex items-start gap-3 rounded-md border border-destructive/30 bg-destructive/10 px-4 py-3"
          data-testid="alert-low-stock"
          role="alert"
        >
          <AlertTriangle className="mt-0.5 h-4 w-4 shrink-0 text-destructive" />
          <div className="flex-1">
            <p className="text-sm font-semibold text-destructive">
              {t('alerts.low_stock_title', { count: lowStockAlerts.length })}
            </p>
            <ul className="mt-1 space-y-0.5">
              {lowStockAlerts.map(alert => (
                <li
                  key={alert.id}
                  className="text-xs text-destructive/80"
                  data-testid={`alert-low-stock-item-${alert.id}`}
                >
                  {alert.name} — {t('alerts.qty_threshold', { qty: alert.quantity, threshold: alert.threshold })}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}

      {expiringAlerts.length > 0 && (
        <div
          className="flex items-start gap-3 rounded-md border border-orange-300 bg-orange-50 px-4 py-3"
          data-testid="alert-expiring"
          role="alert"
        >
          <Clock className="mt-0.5 h-4 w-4 shrink-0 text-orange-600" />
          <div className="flex-1">
            <p className="text-sm font-semibold text-orange-700">
              {t('alerts.expiring_title', { count: expiringAlerts.length })}
            </p>
            <ul className="mt-1 space-y-0.5">
              {expiringAlerts.map(alert => (
                <li
                  key={alert.id}
                  className="text-xs text-orange-600"
                  data-testid={`alert-expiring-item-${alert.id}`}
                >
                  {alert.name} — {t('alerts.expires_on', { date: alert.expiryDate ?? '' })}
                </li>
              ))}
            </ul>
          </div>
        </div>
      )}
    </div>
  )
}
