import { http, HttpResponse } from 'msw'
import type { PreferenceCategoryDto, PreferenceItemDto } from '@/lib/api/preferences'

// In-memory store for user preferences (mutable for session persistence)
let MOCK_USER_PREFERENCES: Record<string, string> = {
  'notifications.email': 'true',
  'notifications.push': 'false',
  'notifications.sms': 'false',
  'notifications.appointment_reminders': 'true',
  'notifications.invoice': 'true',
  'ai.triage_suggestions': 'true',
  'ai.no_show_predictions': 'true',
  'ai.messaging_assistance': 'false',
  'ai.drug_interaction_alerts': 'true',
  'privacy.analytics_tracking': 'true',
  'privacy.usage_data': 'true',
  'privacy.cross_clinic_sharing': 'false',
  'privacy.marketing_emails': 'false',
  'communication.quiet_hours_start': '22:00',
  'communication.quiet_hours_end': '07:00',
  'communication.preferred_channel': 'email',
  'communication.language': 'en',
}

// Clinic-level defaults (Admin-managed)
const CLINIC_DEFAULTS: Record<string, string> = {
  'ai.triage_suggestions': 'true',
  'ai.no_show_predictions': 'true',
  'ai.messaging_assistance': 'false',
  'ai.drug_interaction_alerts': 'true',
  'privacy.analytics_tracking': 'true',
  'privacy.usage_data': 'true',
}

function resolveSource(
  key: string,
  systemDefault: string
): 'system_default' | 'clinic_default' | 'user_override' {
  const userValue = MOCK_USER_PREFERENCES[key]
  const clinicValue = CLINIC_DEFAULTS[key]

  if (userValue !== undefined && userValue !== (clinicValue ?? systemDefault)) {
    return 'user_override'
  }
  if (clinicValue !== undefined) {
    return 'clinic_default'
  }
  return 'system_default'
}

function buildCategories(): PreferenceCategoryDto[] {
  const notifItems: PreferenceItemDto[] = [
    {
      key: 'notifications.email',
      label: 'Email notifications',
      description: 'Receive updates and alerts via email',
      value: MOCK_USER_PREFERENCES['notifications.email'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('notifications.email', 'true'),
      valueType: 'boolean',
    },
    {
      key: 'notifications.push',
      label: 'Push notifications',
      description: 'Browser push notifications',
      value: MOCK_USER_PREFERENCES['notifications.push'] ?? 'false',
      defaultValue: 'false',
      source: resolveSource('notifications.push', 'false'),
      valueType: 'boolean',
      disabled: true,
      disabledReason: 'Coming soon',
    },
    {
      key: 'notifications.sms',
      label: 'SMS notifications',
      description: 'Receive SMS alerts',
      value: MOCK_USER_PREFERENCES['notifications.sms'] ?? 'false',
      defaultValue: 'false',
      source: resolveSource('notifications.sms', 'false'),
      valueType: 'boolean',
      disabled: true,
      disabledReason: 'Coming soon',
    },
    {
      key: 'notifications.appointment_reminders',
      label: 'Appointment reminders',
      description: 'Get reminded before upcoming appointments',
      value: MOCK_USER_PREFERENCES['notifications.appointment_reminders'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('notifications.appointment_reminders', 'true'),
      valueType: 'boolean',
    },
    {
      key: 'notifications.invoice',
      label: 'Invoice notifications',
      description: 'Notifications for new and overdue invoices',
      value: MOCK_USER_PREFERENCES['notifications.invoice'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('notifications.invoice', 'true'),
      valueType: 'boolean',
    },
  ]

  const aiItems: PreferenceItemDto[] = [
    {
      key: 'ai.triage_suggestions',
      label: 'AI Triage suggestions',
      description: 'Show AI-powered triage recommendations during consultations',
      value: MOCK_USER_PREFERENCES['ai.triage_suggestions'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('ai.triage_suggestions', 'true'),
      valueType: 'boolean',
      adminOnly: true,
    },
    {
      key: 'ai.no_show_predictions',
      label: 'No-show predictions',
      description: 'Display AI predictions for appointment no-shows',
      value: MOCK_USER_PREFERENCES['ai.no_show_predictions'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('ai.no_show_predictions', 'true'),
      valueType: 'boolean',
      adminOnly: true,
    },
    {
      key: 'ai.messaging_assistance',
      label: 'AI messaging assistance',
      description: 'Use AI to help draft messages to clients',
      value: MOCK_USER_PREFERENCES['ai.messaging_assistance'] ?? 'false',
      defaultValue: 'false',
      source: resolveSource('ai.messaging_assistance', 'false'),
      valueType: 'boolean',
      adminOnly: true,
    },
    {
      key: 'ai.drug_interaction_alerts',
      label: 'Drug interaction alerts',
      description: 'Show alerts when prescribing potentially interacting medications',
      value: 'true',
      defaultValue: 'true',
      source: 'system_default',
      valueType: 'boolean',
      disabled: true,
      disabledReason: 'This safety feature cannot be disabled',
    },
  ]

  const privacyItems: PreferenceItemDto[] = [
    {
      key: 'privacy.analytics_tracking',
      label: 'Analytics tracking',
      description: 'Allow usage analytics to improve the product',
      value: MOCK_USER_PREFERENCES['privacy.analytics_tracking'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('privacy.analytics_tracking', 'true'),
      valueType: 'boolean',
      adminOnly: true,
    },
    {
      key: 'privacy.usage_data',
      label: 'Usage data collection',
      description: 'Share anonymised usage data for product improvement',
      value: MOCK_USER_PREFERENCES['privacy.usage_data'] ?? 'true',
      defaultValue: 'true',
      source: resolveSource('privacy.usage_data', 'true'),
      valueType: 'boolean',
      adminOnly: true,
    },
    {
      key: 'privacy.cross_clinic_sharing',
      label: 'Cross-clinic data sharing',
      description: 'Allow your clinic data to be used in anonymous cross-clinic benchmarks',
      value: MOCK_USER_PREFERENCES['privacy.cross_clinic_sharing'] ?? 'false',
      defaultValue: 'false',
      source: resolveSource('privacy.cross_clinic_sharing', 'false'),
      valueType: 'boolean',
    },
    {
      key: 'privacy.marketing_emails',
      label: 'Marketing emails',
      description: 'Receive product updates and promotional content',
      value: MOCK_USER_PREFERENCES['privacy.marketing_emails'] ?? 'false',
      defaultValue: 'false',
      source: resolveSource('privacy.marketing_emails', 'false'),
      valueType: 'boolean',
    },
  ]

  const commItems: PreferenceItemDto[] = [
    {
      key: 'communication.quiet_hours_start',
      label: 'Quiet hours start',
      description: 'No notifications will be sent after this time',
      value: MOCK_USER_PREFERENCES['communication.quiet_hours_start'] ?? '22:00',
      defaultValue: '22:00',
      source: resolveSource('communication.quiet_hours_start', '22:00'),
      valueType: 'time',
    },
    {
      key: 'communication.quiet_hours_end',
      label: 'Quiet hours end',
      description: 'Notifications resume after this time',
      value: MOCK_USER_PREFERENCES['communication.quiet_hours_end'] ?? '07:00',
      defaultValue: '07:00',
      source: resolveSource('communication.quiet_hours_end', '07:00'),
      valueType: 'time',
    },
    {
      key: 'communication.preferred_channel',
      label: 'Preferred channel',
      description: 'Your preferred method to receive notifications',
      value: MOCK_USER_PREFERENCES['communication.preferred_channel'] ?? 'email',
      defaultValue: 'email',
      source: resolveSource('communication.preferred_channel', 'email'),
      valueType: 'string',
      options: [
        { value: 'email', label: 'Email' },
        { value: 'sms', label: 'SMS' },
        { value: 'push', label: 'Push' },
      ],
    },
    {
      key: 'communication.language',
      label: 'Language',
      description: 'Your preferred interface language',
      value: MOCK_USER_PREFERENCES['communication.language'] ?? 'en',
      defaultValue: 'en',
      source: resolveSource('communication.language', 'en'),
      valueType: 'string',
      options: [
        { value: 'en', label: 'English' },
        { value: 'ar', label: 'Arabic' },
      ],
    },
  ]

  return [
    {
      key: 'Notifications',
      label: 'Notifications',
      description: 'Control how and when you receive notifications',
      items: notifItems,
    },
    {
      key: 'AIFeatures',
      label: 'AI Features',
      description: 'Manage AI-powered features for your clinic',
      items: aiItems,
    },
    {
      key: 'Privacy',
      label: 'Privacy & Analytics',
      description: 'Control your data and privacy settings',
      items: privacyItems,
    },
    {
      key: 'Communication',
      label: 'Communication',
      description: 'Set your communication preferences',
      items: commItems,
    },
  ]
}

export const preferenceHandlers = [
  // GET /api/preferences
  http.get('/api/preferences', () => {
    return HttpResponse.json<PreferenceCategoryDto[]>(buildCategories())
  }),

  // PUT /api/preferences/:key
  http.put('/api/preferences/:key', async ({ params, request }) => {
    const key = params.key as string
    const body = await request.json() as { value: string }

    // Drug interaction alerts cannot be disabled
    if (key === 'ai.drug_interaction_alerts' && body.value === 'false') {
      return HttpResponse.json(
        { title: 'Drug interaction alerts cannot be disabled', status: 422 },
        { status: 422 }
      )
    }

    MOCK_USER_PREFERENCES[key] = body.value
    return new HttpResponse(null, { status: 204 })
  }),

  // PUT /api/preferences/bulk
  http.put('/api/preferences/bulk', async ({ request }) => {
    const body = await request.json() as { preferences: { key: string; value: string }[] }
    for (const pref of body.preferences) {
      if (pref.key === 'ai.drug_interaction_alerts' && pref.value === 'false') {
        return HttpResponse.json(
          { title: 'Drug interaction alerts cannot be disabled', status: 422 },
          { status: 422 }
        )
      }
      MOCK_USER_PREFERENCES[pref.key] = pref.value
    }
    return new HttpResponse(null, { status: 204 })
  }),

  // GET /api/clinics/preferences
  http.get('/api/clinics/preferences', () => {
    return HttpResponse.json<PreferenceCategoryDto[]>(buildCategories())
  }),

  // PUT /api/clinics/preferences
  http.put('/api/clinics/preferences', async ({ request }) => {
    const body = await request.json() as { preferences: { key: string; value: string }[] }
    for (const pref of body.preferences) {
      CLINIC_DEFAULTS[pref.key] = pref.value
    }
    return new HttpResponse(null, { status: 204 })
  }),

  // POST /api/preferences/consent/revoke
  http.post('/api/preferences/consent/revoke', async ({ request }) => {
    const body = await request.json() as { category: string }
    const prefix = `privacy.`
    // Revoke all keys for the given category
    if (body.category === 'analytics') {
      MOCK_USER_PREFERENCES[`${prefix}analytics_tracking`] = 'false'
      MOCK_USER_PREFERENCES[`${prefix}usage_data`] = 'false'
    } else if (body.category === 'marketing') {
      MOCK_USER_PREFERENCES[`${prefix}marketing_emails`] = 'false'
    }
    return new HttpResponse(null, { status: 204 })
  }),
]
