import { apiGet, apiPost, apiPatch, apiDelete } from './client'

export type UserRole = 'ADMIN' | 'VET' | 'ASSISTANT' | 'RECEPTIONIST'

export interface UserDto {
  id: string
  email: string
  fullName: string
  role: UserRole
  isActive: boolean
}

export interface InviteUserRequest {
  email: string
  fullName: string
  role: 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
}

export interface InviteUserResponse {
  user: UserDto
  temporaryPassword: string
}

export interface ChangeRoleRequest {
  role: 'VET' | 'ASSISTANT' | 'RECEPTIONIST'
}

export async function getUsers(): Promise<UserDto[]> {
  return apiGet<UserDto[]>('/api/users')
}

export async function inviteUser(data: InviteUserRequest): Promise<InviteUserResponse> {
  return apiPost<InviteUserResponse>('/api/users', data)
}

export async function changeUserRole(userId: string, data: ChangeRoleRequest): Promise<UserDto> {
  return apiPatch<UserDto>(`/api/users/${userId}/role`, data)
}

export async function deactivateUser(userId: string): Promise<void> {
  return apiDelete(`/api/users/${userId}`)
}
