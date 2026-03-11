import { http, HttpResponse, delay } from 'msw'
import type { TriageRequest, TriageResponse } from '@/lib/api/ai-triage'

export const aiHandlers = [
  // POST /api/v1/ai/triage
  http.post('/api/v1/ai/triage', async ({ request }) => {
    const body = await request.json() as TriageRequest
    await delay(600)

    // Vary severity based on keywords in symptoms for realistic mock behaviour
    const symptoms = (body.symptoms ?? '').toLowerCase()
    let severity: TriageResponse['severity'] = 'Routine'
    let reasoning = 'Based on the symptoms described, this appears to be a non-urgent condition that can be addressed during a standard appointment.'
    let estimatedDurationMinutes = 20
    let recommendedSpecialty = 'General Practice'

    if (
      symptoms.includes('bleeding') ||
      symptoms.includes('collapse') ||
      symptoms.includes('seizure') ||
      symptoms.includes('unconscious') ||
      symptoms.includes('difficulty breathing')
    ) {
      severity = 'Emergency'
      reasoning = 'The symptoms described indicate a potentially life-threatening condition requiring immediate attention. Seek emergency veterinary care immediately.'
      estimatedDurationMinutes = 60
      recommendedSpecialty = 'Emergency & Critical Care'
    } else if (
      symptoms.includes('vomit') ||
      symptoms.includes('limp') ||
      symptoms.includes('loss of appetite') ||
      symptoms.includes('fever') ||
      symptoms.includes('pain')
    ) {
      severity = 'Normal'
      reasoning = 'Based on the symptoms described, this appears to be a moderate condition that should be evaluated soon but is not immediately life-threatening. Schedule an appointment within 24-48 hours.'
      estimatedDurationMinutes = 30
      recommendedSpecialty = 'General Practice'
    }

    const response: TriageResponse = {
      triageId: crypto.randomUUID(),
      severity,
      estimatedDurationMinutes,
      recommendedSpecialty,
      reasoning,
      disclaimer:
        'This AI triage is for informational purposes only and does not constitute veterinary advice. Always consult a licensed veterinarian for diagnosis and treatment.',
      confidence: 0.82,
    }

    return HttpResponse.json<TriageResponse>(response)
  }),
]
