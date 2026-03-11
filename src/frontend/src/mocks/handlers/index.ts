import { authHandlers } from './auth'
import { billingHandlers } from './billing'
import { appointmentHandlers } from './appointments'
import { patientHandlers } from './patients'
import { userHandlers } from './users'
import { dashboardHandlers } from './dashboard'
import { messagingHandlers } from './messaging'
import { drugHandlers } from './drugs'
import { portalHandlers } from './portal'
import { onboardingHandlers } from './onboarding'
import { preferenceHandlers } from './preferences'
import { aiHandlers } from './ai'

export const handlers = [
  ...authHandlers,
  ...appointmentHandlers,
  ...billingHandlers,
  ...patientHandlers,
  ...userHandlers,
  ...dashboardHandlers,
  ...messagingHandlers,
  ...drugHandlers,
  ...portalHandlers,
  ...onboardingHandlers,
  ...preferenceHandlers,
  ...aiHandlers,
]
