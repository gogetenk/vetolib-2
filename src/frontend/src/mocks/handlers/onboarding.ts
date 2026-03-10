import { http, HttpResponse, delay } from 'msw'
import type { OnboardingStateDto } from '@/lib/api/onboarding'

// In-memory onboarding state keyed by userId (sub claim) so each user gets
// independent state. Falls back to role if no sub is present.
type OnboardingStateMap = Record<string, OnboardingStateDto>

function makeDefaultState(role: string): OnboardingStateDto {
  const upperRole = role.toUpperCase()

  const adminSteps = [
    { id: 'invite_team_member', title: 'Invite your first team member', completed: false, order: 1 },
    { id: 'add_first_patient', title: 'Add your first patient', completed: false, order: 2 },
    { id: 'book_first_appointment', title: 'Book your first appointment', completed: false, order: 3 },
    { id: 'create_first_invoice', title: 'Create your first invoice', completed: false, order: 4 },
    { id: 'explore_dashboard', title: 'Explore the dashboard', completed: false, order: 5 },
  ]

  const vetSteps = [
    { id: 'view_appointments', title: 'View your appointments', completed: false, order: 1 },
    { id: 'open_patient_record', title: 'Open a patient record', completed: false, order: 2 },
    { id: 'add_medical_record', title: 'Add a medical record', completed: false, order: 3 },
    { id: 'write_prescription', title: 'Write a prescription', completed: false, order: 4 },
  ]

  const receptionistSteps = [
    { id: 'book_appointment', title: 'Book an appointment', completed: false, order: 1 },
    { id: 'check_in_patient', title: 'Check in a patient', completed: false, order: 2 },
    { id: 'create_invoice', title: 'Create an invoice', completed: false, order: 3 },
    { id: 'send_invoice', title: 'Send an invoice', completed: false, order: 4 },
  ]

  const assistantSteps = [
    { id: 'browse_patients', title: 'Browse patients', completed: false, order: 1 },
    { id: 'view_medical_record', title: 'View a medical record', completed: false, order: 2 },
    { id: 'check_today_schedule', title: "Check today's schedule", completed: false, order: 3 },
  ]

  const steps =
    upperRole === 'ADMIN' ? adminSteps
    : upperRole === 'VET' ? vetSteps
    : upperRole === 'RECEPTIONIST' ? receptionistSteps
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

const stateByUser: OnboardingStateMap = {}

interface TokenPayload {
  sub?: string
  role?: string
}

function parseTokenPayload(request: Request): TokenPayload {
  const authHeader = request.headers.get('Authorization') ?? ''
  const token = authHeader.replace('Bearer ', '')
  if (!token) return {}
  try {
    const parts = token.split('.')
    // Restore base64 padding that may have been stripped
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64 + '=='.slice(0, (4 - (base64.length % 4)) % 4)
    return JSON.parse(atob(padded)) as TokenPayload
  } catch {
    return {}
  }
}

function getOrCreateState(request: Request): OnboardingStateDto {
  const payload = parseTokenPayload(request)
  // Key by sub (user email) first, fall back to role for anonymous
  const key = payload.sub ?? payload.role ?? 'anonymous'
  const role = payload.role ?? 'VET'
  if (!stateByUser[key]) {
    stateByUser[key] = makeDefaultState(role)
  }
  return stateByUser[key]
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
    const state = getOrCreateState(request)
    return HttpResponse.json<OnboardingStateDto>(state)
  }),

  // POST /api/onboarding/banner/dismiss
  http.post('/api/onboarding/banner/dismiss', async ({ request }) => {
    await delay(80)
    const state = getOrCreateState(request)
    state.welcomeBannerVisible = false
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/onboarding/checklist/dismiss
  http.post('/api/onboarding/checklist/dismiss', async ({ request }) => {
    await delay(80)
    const state = getOrCreateState(request)
    state.checklistVisible = false
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/onboarding/steps/:stepId/complete
  http.post('/api/onboarding/steps/:stepId/complete', async ({ request, params }) => {
    await delay(80)
    const state = getOrCreateState(request)
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
