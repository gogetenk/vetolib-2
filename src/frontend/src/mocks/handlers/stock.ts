import { http, HttpResponse } from 'msw'
import type {
  StockItemDto,
  StockAlertDto,
  StockMovementDto,
  CreateStockItemRequest,
  UpdateStockItemRequest,
  CreateStockMovementRequest,
} from '@/lib/api/stock'

const CLINIC_ID = 'clinic-001'

const MOCK_STOCK_ITEMS: StockItemDto[] = [
  {
    id: 'stock-0000-0000-0000-000000000001',
    name: 'Meloxicam 1.5mg/ml',
    category: 'Medication',
    quantity: 5,
    unit: 'bottles',
    threshold: 20,
    expiryDate: '2026-08-15',
    clinicId: CLINIC_ID,
    isLowStock: true,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000002',
    name: 'Amoxicillin 250mg',
    category: 'Medication',
    quantity: 120,
    unit: 'tablets',
    threshold: 30,
    expiryDate: '2027-03-01',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000003',
    name: 'Ketamine 100mg/ml',
    category: 'Medication',
    quantity: 8,
    unit: 'vials',
    threshold: 5,
    expiryDate: '2026-04-30',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: true,
  },
  {
    id: 'stock-0000-0000-0000-000000000004',
    name: 'Surgical Gloves (M)',
    category: 'Supply',
    quantity: 12,
    unit: 'boxes',
    threshold: 15,
    expiryDate: null,
    clinicId: CLINIC_ID,
    isLowStock: true,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000005',
    name: 'Syringes 5ml',
    category: 'Supply',
    quantity: 200,
    unit: 'units',
    threshold: 50,
    expiryDate: null,
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
  {
    id: 'stock-0000-0000-0000-000000000006',
    name: 'Rabies Vaccine',
    category: 'Vaccine',
    quantity: 24,
    unit: 'doses',
    threshold: 10,
    expiryDate: '2026-12-31',
    clinicId: CLINIC_ID,
    isLowStock: false,
    isExpiringSoon: false,
  },
]

const MOCK_MOVEMENTS: StockMovementDto[] = []

export const stockHandlers = [
  // GET /api/v1/stock
  http.get('/api/v1/stock', ({ request }) => {
    const url = new URL(request.url)
    const category = url.searchParams.get('category')
    const status = url.searchParams.get('status')

    let items = [...MOCK_STOCK_ITEMS]

    if (category) {
      items = items.filter(item => item.category === category)
    }

    if (status === 'low-stock') {
      items = items.filter(item => item.isLowStock)
    } else if (status === 'expiring-soon') {
      items = items.filter(item => item.isExpiringSoon)
    }

    return HttpResponse.json<StockItemDto[]>(items)
  }),

  // GET /api/v1/stock/alerts
  http.get('/api/v1/stock/alerts', () => {
    const alerts: StockAlertDto[] = MOCK_STOCK_ITEMS
      .filter(item => item.isLowStock || item.isExpiringSoon)
      .map(item => ({
        id: item.id,
        name: item.name,
        category: item.category,
        quantity: item.quantity,
        threshold: item.threshold,
        expiryDate: item.expiryDate,
        alertType: item.isLowStock ? 'low-stock' : 'expiring-soon',
      }))

    return HttpResponse.json<StockAlertDto[]>(alerts)
  }),

  // POST /api/v1/stock
  http.post('/api/v1/stock', async ({ request }) => {
    const body = await request.json() as CreateStockItemRequest

    const newItem: StockItemDto = {
      id: crypto.randomUUID(),
      name: body.name,
      category: body.category,
      quantity: body.quantity,
      unit: body.unit,
      threshold: body.threshold,
      expiryDate: body.expiryDate ?? null,
      clinicId: CLINIC_ID,
      isLowStock: body.quantity <= body.threshold,
      isExpiringSoon: false,
    }

    MOCK_STOCK_ITEMS.push(newItem)
    return HttpResponse.json<StockItemDto>(newItem, { status: 201 })
  }),

  // PATCH /api/v1/stock/:id
  http.patch('/api/v1/stock/:id', async ({ params, request }) => {
    const item = MOCK_STOCK_ITEMS.find(s => s.id === params.id)
    if (!item) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as UpdateStockItemRequest

    if (body.name !== undefined) item.name = body.name
    if (body.category !== undefined) item.category = body.category
    if (body.quantity !== undefined) item.quantity = body.quantity
    if (body.unit !== undefined) item.unit = body.unit
    if (body.threshold !== undefined) item.threshold = body.threshold
    if (Object.prototype.hasOwnProperty.call(body, 'expiryDate')) {
      item.expiryDate = body.expiryDate ?? null
    }

    item.isLowStock = item.quantity <= item.threshold

    return HttpResponse.json<StockItemDto>(item)
  }),

  // POST /api/v1/stock/:stockItemId/movements
  http.post('/api/v1/stock/:stockItemId/movements', async ({ params, request }) => {
    const stockItemId = params.stockItemId as string
    const item = MOCK_STOCK_ITEMS.find(s => s.id === stockItemId)
    if (!item) return new HttpResponse(null, { status: 404 })

    const body = await request.json() as CreateStockMovementRequest

    const delta =
      body.type === 'IN' ? body.quantity :
      body.type === 'OUT' ? -body.quantity :
      body.quantity // ADJUSTMENT: set directly (treat as delta)

    const newQuantity = Math.max(0, item.quantity + delta)
    item.quantity = newQuantity
    item.isLowStock = newQuantity <= item.threshold

    const movement: StockMovementDto = {
      id: crypto.randomUUID(),
      stockItemId,
      type: body.type,
      quantity: body.quantity,
      reason: body.reason ?? null,
      newQuantity,
      createdAt: new Date().toISOString(),
    }

    MOCK_MOVEMENTS.push(movement)
    return HttpResponse.json<StockMovementDto>(movement, { status: 201 })
  }),
]
