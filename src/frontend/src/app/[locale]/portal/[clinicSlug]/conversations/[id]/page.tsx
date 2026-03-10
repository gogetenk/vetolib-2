import { PortalConversation } from '@/components/features/portal/PortalConversation'

interface Props {
  params: Promise<{ id: string }>
}

export default async function ConversationPage({ params }: Props) {
  const { id } = await params
  return <PortalConversation conversationId={id} />
}
