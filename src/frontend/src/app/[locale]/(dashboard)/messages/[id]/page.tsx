import type { Metadata } from 'next'
import { ConversationDetailPage } from '@/components/features/messaging/ConversationDetailPage'

interface PageProps {
  params: Promise<{ id: string; locale: string }>
}

export async function generateMetadata(): Promise<Metadata> {
  return {
    title: 'Conversation — Vetolib',
  }
}

export default async function Page({ params }: PageProps) {
  const { id } = await params
  return <ConversationDetailPage conversationId={id} />
}
