import { authHandlers } from './auth'
import { billingHandlers } from './billing'
import { appointmentHandlers } from './appointments'
import { patientHandlers } from './patients'
import { userHandlers } from './users'
import { dashboardHandlers } from './dashboard'

export const handlers = [
  ...authHandlers,
  ...appointmentHandlers,
  ...billingHandlers,
  ...patientHandlers,
  ...userHandlers,
  ...dashboardHandlers,
]
