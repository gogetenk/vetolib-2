import type { Metadata } from 'next'
import { MessagesPage } from '@/components/features/messaging/MessagesPage'

export const metadata: Metadata = {
  title: 'Messages',
}

export default function Page() {
  return <MessagesPage />
}
