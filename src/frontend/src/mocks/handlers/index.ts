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
import { stockHandlers } from './stock'
import { aiHandlers } from './ai'
import { bookingHandlers } from './booking'
import { clinicGroupHandlers } from './clinic-group'
import { reminderHandlers } from './reminders'
import { healthAlertHandlers } from './health-alerts'
import { weightHandlers } from './weights'
import { breedingHandlers } from './breeding'
import { recordSharingHandlers } from './record-sharing'
import { petOwnerHandlers } from './pet-owners'
import { workingHoursHandlers } from './working-hours'
import { notificationHandlers } from './notifications'
import { auditHandlers } from './audit'
import { eReportingHandlers } from './e-reporting'

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
  ...stockHandlers,
  ...aiHandlers,
  ...bookingHandlers,
  ...clinicGroupHandlers,
  ...reminderHandlers,
  ...healthAlertHandlers,
  ...weightHandlers,
  ...breedingHandlers,
  ...recordSharingHandlers,
  ...petOwnerHandlers,
  ...workingHoursHandlers,
  ...notificationHandlers,
  ...auditHandlers,
  ...eReportingHandlers,
]
