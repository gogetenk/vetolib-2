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

  http.post('/api/v1/ai/soap-notes', async ({ request }) => {
    await delay(800)
    const body = await request.json() as {
      reason?: string
      anamnesis?: string
      species?: string
      weight?: number
      temperature?: number
      heartRate?: number
    }

    const reason = body.reason || 'routine examination'
    const anamnesis = body.anamnesis || 'No additional history provided.'
    const species = body.species || 'canine'
    const weight = body.weight ? `${body.weight} kg` : 'not recorded'
    const temp = body.temperature ? `${body.temperature}°C` : 'not recorded'
    const hr = body.heartRate ? `${body.heartRate} bpm` : 'not recorded'

    return HttpResponse.json({
      subjective: `Owner reports ${reason}. ${anamnesis} The ${species} patient has been showing these signs for approximately 2-3 days. No previous episodes reported. Appetite and water intake are normal. No recent travel history.`,
      objective: `Physical examination: Weight ${weight}, Temperature ${temp}, Heart rate ${hr}. Body condition score 5/9. Mucous membranes pink and moist, CRT < 2 seconds. Thoracic auscultation: no abnormal lung sounds, regular cardiac rhythm. Abdominal palpation: no pain or organomegaly. Lymph nodes within normal limits. Hydration status adequate.`,
      assessment: `Based on clinical presentation and history of ${reason}, primary differential diagnoses include: 1) Mild inflammatory process, 2) Early-stage infection, 3) Environmental/seasonal reaction. Prognosis is good with appropriate treatment.`,
      plan: `1. Prescribe anti-inflammatory medication for 5 days\n2. Monitor temperature daily at home\n3. Restrict exercise for 48 hours\n4. Ensure adequate hydration\n5. Follow-up appointment in 7 days if symptoms persist\n6. Owner to contact clinic immediately if condition worsens`,
      disclaimer: 'AI-generated content. Must be reviewed and validated by a licensed veterinarian before use.',
    })
  }),
]
