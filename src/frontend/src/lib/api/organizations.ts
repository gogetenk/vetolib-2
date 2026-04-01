import { apiGet } from './client'

export interface OrganizationSummary {
  id: string
  name: string
  address: string
}

export interface MyOrganizationsResponse {
  organizations: OrganizationSummary[]
}

/**
 * GET /api/v1/auth/my-organizations
 * Returns the list of Keycloak organizations (clinics) the current user belongs to.
 */
export async function getMyOrganizations(): Promise<MyOrganizationsResponse> {
  return apiGet<MyOrganizationsResponse>('/api/v1/auth/my-organizations')
}
