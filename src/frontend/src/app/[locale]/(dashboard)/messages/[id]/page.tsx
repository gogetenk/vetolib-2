import type { Metadata } from 'next'
import { ConversationDetailPage } from '@/components/features/messaging/ConversationDetailPage'

export const metadata: Metadata = {
  title: 'Conversation — Vetolib',
}

interface PageProps {
  params: Promise<{ id: string }>
}

export default async function Page({ params }: PageProps) {
  const { id } = await params
  return <ConversationDetailPage conversationId={id} />
}
