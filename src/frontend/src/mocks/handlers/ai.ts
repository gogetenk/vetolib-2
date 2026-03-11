import { http, HttpResponse, delay } from 'msw'

export const aiHandlers = [
  http.post('/api/v1/ai/triage', async ({ request }) => {
    await delay(500)
    const body = await request.json() as { symptoms: string; species?: string }

    const isUrgent = body.symptoms?.toLowerCase().includes('blood') ||
                     body.symptoms?.toLowerCase().includes('breathing') ||
                     body.symptoms?.toLowerCase().includes('poison')

    return HttpResponse.json({
      severity: isUrgent ? 'URGENT' : 'MODERATE',
      recommendation: isUrgent
        ? 'Seek immediate veterinary care. This appears to be an emergency.'
        : 'Schedule appointment within 24-48 hours for examination.',
      reasoning: `Based on the reported symptoms, this case has been assessed as ${isUrgent ? 'urgent' : 'moderate'} priority.`,
      suggestedCategory: isUrgent ? 'MedicalUrgency' : 'MedicalQuestion',
      confidence: isUrgent ? 0.91 : 0.78,
    })
  }),
]
