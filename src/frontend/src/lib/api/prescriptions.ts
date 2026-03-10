import { apiPost } from './client'
import type { PreflightRequest, PrescriptionPreflightResult } from './types'

export async function checkPrescriptionPreflight(
  data: PreflightRequest
): Promise<PrescriptionPreflightResult> {
  return apiPost<PrescriptionPreflightResult>(
    '/api/medical-records/prescriptions/preflight',
    data
  )
}
