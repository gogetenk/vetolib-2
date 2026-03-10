import { http, HttpResponse, delay } from 'msw'
import type { OnboardingStateDto } from '@/lib/api/onboarding'

// In-memory onboarding state keyed by role (simulates per-user state)
type OnboardingStateMap = Record<string, OnboardingStateDto>

function makeDefaultState(role: string): OnboardingStateDto {
  const isAdmin = role === 'ADMIN'
  const isVet = role === 'VET'

  const adminSteps = [
    { id: 'add-team-member', title: 'Add a team member', completed: false, order: 1 },
    { id: 'configure-working-hours', title: 'Configure working hours', completed: false, order: 2 },
    { id: 'add-first-patient', title: 'Add your first patient', completed: false, order: 3 },
    { id: 'create-invoice', title: 'Create your first invoice', completed: false, order: 4 },
  ]

  const vetSteps = [
    { id: 'add-first-patient', title: 'Add your first patient', completed: false, order: 1 },
    { id: 'create-medical-record', title: 'Create a medical record', completed: false, order: 2 },
    { id: 'add-prescription', title: 'Add a prescription', completed: false, order: 3 },
  ]

  const receptionistSteps = [
    { id: 'book-first-appointment', title: 'Book your first appointment', completed: false, order: 1 },
    { id: 'add-owner', title: 'Add an owner', completed: false, order: 2 },
  ]

  const assistantSteps = [
    { id: 'view-patient-record', title: 'View a patient record', completed: false, order: 1 },
    { id: 'take-consultation-notes', title: 'Take consultation notes', completed: false, order: 2 },
  ]

  const steps =
    isAdmin ? adminSteps
    : isVet ? vetSteps
    : role === 'RECEPTIONIST' ? receptionistSteps
    : assistantSteps

  return {
    welcomeBannerVisible: true,
    checklistVisible: true,
    progress: {
      totalSteps: steps.length,
      completedSteps: 0,
      percentComplete: 0,
    },
    steps,
  }
}

const stateByRole: OnboardingStateMap = {}

function getOrCreateState(role: string): OnboardingStateDto {
  if (!stateByRole[role]) {
    stateByRole[role] = makeDefaultState(role)
  }
  return stateByRole[role]
}

function parseRoleFromRequest(request: Request): string {
  const authHeader = request.headers.get('Authorization') ?? ''
  const token = authHeader.replace('Bearer ', '')
  if (!token) return 'VET'
  try {
    const parts = token.split('.')
    // Restore base64 padding that may have been stripped
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64 + '=='.slice(0, (4 - (base64.length % 4)) % 4)
    const payload = JSON.parse(atob(padded)) as Record<string, unknown>
    return (payload['role'] as string) ?? 'VET'
  } catch {
    return 'VET'
  }
}

function recalcProgress(state: OnboardingStateDto): void {
  const total = state.steps.length
  const completed = state.steps.filter((s) => s.completed).length
  state.progress = {
    totalSteps: total,
    completedSteps: completed,
    percentComplete: total === 0 ? 0 : Math.round((completed / total) * 100),
  }
}

export const onboardingHandlers = [
  // GET /api/onboarding
  http.get('/api/onboarding', async ({ request }) => {
    await delay(100)
    const role = parseRoleFromRequest(request)
    const state = getOrCreateState(role)
    return HttpResponse.json<OnboardingStateDto>(state)
  }),

  // POST /api/onboarding/banner/dismiss
  http.post('/api/onboarding/banner/dismiss', async ({ request }) => {
    await delay(80)
    const role = parseRoleFromRequest(request)
    const state = getOrCreateState(role)
    state.welcomeBannerVisible = false
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/onboarding/checklist/dismiss
  http.post('/api/onboarding/checklist/dismiss', async ({ request }) => {
    await delay(80)
    const role = parseRoleFromRequest(request)
    const state = getOrCreateState(role)
    state.checklistVisible = false
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/onboarding/steps/:stepId/complete
  http.post('/api/onboarding/steps/:stepId/complete', async ({ request, params }) => {
    await delay(80)
    const role = parseRoleFromRequest(request)
    const state = getOrCreateState(role)
    const stepId = params.stepId as string
    const step = state.steps.find((s) => s.id === stepId)
    if (!step) {
      return HttpResponse.json({ title: 'Step not found' }, { status: 404 })
    }
    step.completed = true
    recalcProgress(state)
    return new HttpResponse(null, { status: 204 })
  }),
]
