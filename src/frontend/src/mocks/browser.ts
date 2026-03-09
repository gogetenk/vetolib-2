// This file must only be imported in browser context (client components with dynamic import).
// Do NOT import this at module level in any server component.
import { setupWorker } from 'msw/browser'
import { handlers } from './handlers'

export const worker = setupWorker(...handlers)
