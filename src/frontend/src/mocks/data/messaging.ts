/**
 * Realistic mock data for Messaging module.
 * UAE context: Arabic/English names, AED currency, Asia/Dubai timezone.
 */
import type {
  ConversationDto,
  MessageDto,
  ResponseTemplateDto,
  MessagingHoursDto,
  PortalConversationDto,
  PortalMessageDto,
  PortalPetDto,
  AiSuggestion,
  MessageClassificationDto,
} from '@/lib/api/messaging-types'

// ─── Classification helpers ──────────────────────────────────────────────────

function makeClassification(
  urgency: MessageClassificationDto['urgency'],
  category: MessageClassificationDto['category'],
  confidence: number,
): MessageClassificationDto {
  return {
    urgency,
    category,
    confidence,
    overriddenByUserId: null,
    overriddenAt: null,
  }
}

// ─── Conversations ────────────────────────────────────────────────────────────

export const MOCK_CONVERSATIONS: ConversationDto[] = [
  {
    id: 'conv-0000-0000-0000-000000000001',
    clinicId: 'clinic-001',
    ownerId: 'owner-001',
    ownerName: 'Ahmed Al-Rashid',
    ownerEmail: 'ahmed.alrashid@email.ae',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    subject: 'Max has been limping since this morning',
    category: 'MedicalUrgency',
    priority: 'Critical',
    status: 'Open',
    assignedToUserId: null,
    assignedToUserName: null,
    assignedToRole: 'VET',
    aiTriageConfidence: 0.91,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 1,
    lastMessageAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
    createdAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000002',
    clinicId: 'clinic-001',
    ownerId: 'owner-002',
    ownerName: 'Fatima Hassan',
    ownerEmail: 'fatima.hassan@email.ae',
    patientId: 'pat-0000-0000-0000-000000000002',
    patientName: 'Luna',
    subject: 'Luna\'s post-surgery follow-up — stitches look red',
    category: 'PostOperativeFollowUp',
    priority: 'High',
    status: 'InProgress',
    assignedToUserId: 'user-vet-001',
    assignedToUserName: 'Dr. Sarah Johnson',
    assignedToRole: 'VET',
    aiTriageConfidence: 0.88,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 0,
    lastMessageAt: new Date('2026-03-10T09:20:00+04:00').toISOString(),
    createdAt: new Date('2026-03-09T14:30:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000003',
    clinicId: 'clinic-001',
    ownerId: 'owner-003',
    ownerName: 'Mohammed Al-Farsi',
    ownerEmail: 'mohammed.alfarsi@email.ae',
    patientId: 'pat-0000-0000-0000-000000000003',
    patientName: 'Simba',
    subject: 'Booking appointment for annual vaccination',
    category: 'AppointmentRequest',
    priority: 'Normal',
    status: 'Open',
    assignedToUserId: 'user-rec-001',
    assignedToUserName: 'Khalid Al-Nuaimi',
    assignedToRole: 'RECEPTIONIST',
    aiTriageConfidence: 0.95,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 1,
    lastMessageAt: new Date('2026-03-10T08:10:00+04:00').toISOString(),
    createdAt: new Date('2026-03-10T08:10:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000004',
    clinicId: 'clinic-001',
    ownerId: 'owner-004',
    ownerName: 'Sara Al-Mansoori',
    ownerEmail: 'sara.almansoori@email.ae',
    patientId: 'pat-0000-0000-0000-000000000004',
    patientName: 'Bella',
    subject: 'What are your clinic opening hours on Friday?',
    category: 'Administrative',
    priority: 'Low',
    status: 'Resolved',
    assignedToUserId: 'user-rec-001',
    assignedToUserName: 'Khalid Al-Nuaimi',
    assignedToRole: 'RECEPTIONIST',
    aiTriageConfidence: 0.93,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 0,
    lastMessageAt: new Date('2026-03-09T11:00:00+04:00').toISOString(),
    createdAt: new Date('2026-03-09T10:00:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000005',
    clinicId: 'clinic-001',
    ownerId: 'owner-005',
    ownerName: 'Khalid Al-Mazrouei',
    ownerEmail: 'khalid.almazrouei@email.ae',
    patientId: null,
    patientName: null,
    subject: 'General enquiry about microchipping',
    category: 'Administrative',
    priority: 'Low',
    status: 'Open',
    assignedToUserId: null,
    assignedToUserName: null,
    assignedToRole: 'RECEPTIONIST',
    aiTriageConfidence: 0.62,
    isTriageUncertain: true,
    isSpam: false,
    unreadCount: 1,
    lastMessageAt: new Date('2026-03-10T06:30:00+04:00').toISOString(),
    createdAt: new Date('2026-03-10T06:30:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000006',
    clinicId: 'clinic-001',
    ownerId: 'owner-006',
    ownerName: 'Noura Al-Maktoum',
    ownerEmail: 'noura.almaktoum@email.ae',
    patientId: 'pat-0000-0000-0000-000000000002',
    patientName: 'Luna',
    subject: 'قطتي لا تأكل منذ يومين',
    category: 'MedicalQuestion',
    priority: 'Normal',
    status: 'InProgress',
    assignedToUserId: 'user-vet-001',
    assignedToUserName: 'Dr. Sarah Johnson',
    assignedToRole: 'VET',
    aiTriageConfidence: 0.84,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 2,
    lastMessageAt: new Date('2026-03-10T10:00:00+04:00').toISOString(),
    createdAt: new Date('2026-03-10T08:00:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000007',
    clinicId: 'clinic-001',
    ownerId: 'owner-007',
    ownerName: 'Reem Al-Suwaidi',
    ownerEmail: 'reem.alsuwaidi@email.ae',
    patientId: 'pat-0000-0000-0000-000000000001',
    patientName: 'Max',
    subject: 'Thank you for the excellent care during Max\'s surgery',
    category: 'Feedback',
    priority: 'Low',
    status: 'Resolved',
    assignedToUserId: null,
    assignedToUserName: null,
    assignedToRole: 'ADMIN',
    aiTriageConfidence: 0.97,
    isTriageUncertain: false,
    isSpam: false,
    unreadCount: 0,
    lastMessageAt: new Date('2026-03-08T15:00:00+04:00').toISOString(),
    createdAt: new Date('2026-03-08T15:00:00+04:00').toISOString(),
  },
  {
    id: 'conv-0000-0000-0000-000000000008',
    clinicId: 'clinic-001',
    ownerId: 'owner-008',
    ownerName: 'Omar Al-Zaabi',
    ownerEmail: 'omar.alzaabi@email.ae',
    patientId: null,
    patientName: null,
    subject: 'Buy cheap medications online — no prescription needed',
    category: 'Other',
    priority: 'Low',
    status: 'Closed',
    assignedToUserId: null,
    assignedToUserName: null,
    assignedToRole: 'RECEPTIONIST',
    aiTriageConfidence: 0.55,
    isTriageUncertain: false,
    isSpam: true,
    unreadCount: 0,
    lastMessageAt: new Date('2026-03-07T12:00:00+04:00').toISOString(),
    createdAt: new Date('2026-03-07T12:00:00+04:00').toISOString(),
  },
]

// ─── Messages ─────────────────────────────────────────────────────────────────

export const MOCK_MESSAGES: Record<string, MessageDto[]> = {
  'conv-0000-0000-0000-000000000001': [
    {
      id: 'msg-001-01',
      conversationId: 'conv-0000-0000-0000-000000000001',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Ahmed Al-Rashid',
      body: 'Good morning. My dog Max has been limping since this morning and is whimpering when he puts weight on his front right leg. He did not fall or get injured that I know of. Should I bring him in today? This is urgent.',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Critical', 'MedicalUrgency', 0.91),
    },
  ],
  'conv-0000-0000-0000-000000000002': [
    {
      id: 'msg-002-01',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Fatima Hassan',
      body: 'Luna had her spay surgery 5 days ago. The stitches look red and swollen today. There is a small amount of discharge. I am worried this might be infected.',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T14:30:00+04:00').toISOString(),
      attachments: [
        {
          id: 'att-001',
          fileName: 'luna_stitches.jpg',
          contentType: 'image/jpeg',
          fileSizeBytes: 1_240_000,
          url: '/mock-attachments/luna_stitches.jpg',
        },
      ],
      classification: makeClassification('High', 'PostOperativeFollowUp', 0.88),
    },
    {
      id: 'msg-002-02',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Vet',
      senderUserId: 'user-vet-001',
      senderName: 'Dr. Sarah Johnson',
      body: 'Thank you for the photo, Fatima. Some redness is normal at this stage, but the discharge is something we should look at. Please bring Luna to the clinic today. I will reserve a slot for you at 2 PM. In the meantime, do not let her lick the area — use the e-collar if you have it.',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T15:10:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-002-03',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Vet',
      senderUserId: 'user-vet-001',
      senderName: 'Dr. Sarah Johnson',
      body: 'Possible early-stage wound infection. Prescribed chlorhexidine wash + oral amoxicillin. Monitor for 48 hours. If worsening, escalate to surgery team.',
      isInternalNote: true,
      sentAt: new Date('2026-03-09T16:30:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-002-04',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Fatima Hassan',
      body: 'We came in this afternoon. Thank you for seeing us so quickly. Dr. Sarah was very reassuring. Luna has her antibiotics and is resting. We will monitor as advised.',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T18:00:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-002-05',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Vet',
      senderUserId: 'user-vet-001',
      senderName: 'Dr. Sarah Johnson',
      body: 'Great to hear. Please send us a photo in 48 hours so we can check progress remotely. Call us immediately if Luna develops a fever or stops eating.',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T18:15:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-002-06',
      conversationId: 'conv-0000-0000-0000-000000000002',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Fatima Hassan',
      body: 'Update: the area looks much better this morning. Less swelling, no more discharge. Luna ate her food and seems more comfortable. Attaching a photo.',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T09:20:00+04:00').toISOString(),
      attachments: [
        {
          id: 'att-002',
          fileName: 'luna_day2.jpg',
          contentType: 'image/jpeg',
          fileSizeBytes: 980_000,
          url: '/mock-attachments/luna_day2.jpg',
        },
      ],
    },
  ],
  'conv-0000-0000-0000-000000000003': [
    {
      id: 'msg-003-01',
      conversationId: 'conv-0000-0000-0000-000000000003',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Mohammed Al-Farsi',
      body: 'Hello, I would like to book an appointment for Simba\'s annual vaccination. He is due this month. What slots are available this week? Preferred afternoon if possible.',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T08:10:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Normal', 'AppointmentRequest', 0.95),
    },
  ],
  'conv-0000-0000-0000-000000000004': [
    {
      id: 'msg-004-01',
      conversationId: 'conv-0000-0000-0000-000000000004',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Sara Al-Mansoori',
      body: 'Hi, could you please tell me what your opening hours are on Friday? I need to bring Bella for a check-up.',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T10:00:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Low', 'Administrative', 0.93),
    },
    {
      id: 'msg-004-02',
      conversationId: 'conv-0000-0000-0000-000000000004',
      sender: 'Staff',
      senderUserId: 'user-rec-001',
      senderName: 'Khalid Al-Nuaimi',
      body: 'Hello Sara! On Fridays we are open from 08:00 to 12:00. You are welcome to book online or call us on +971 4 123 4567. We would be happy to see Bella!',
      isInternalNote: false,
      sentAt: new Date('2026-03-09T11:00:00+04:00').toISOString(),
      attachments: [],
    },
  ],
  'conv-0000-0000-0000-000000000005': [
    {
      id: 'msg-005-01',
      conversationId: 'conv-0000-0000-0000-000000000005',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Khalid Al-Mazrouei',
      body: 'Good morning. I would like to know more about microchipping. What is the cost and how long does the procedure take? Also, do I need to make an appointment or can I walk in?',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T06:30:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Low', 'Administrative', 0.62),
    },
  ],
  'conv-0000-0000-0000-000000000006': [
    {
      id: 'msg-006-01',
      conversationId: 'conv-0000-0000-0000-000000000006',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Noura Al-Maktoum',
      body: 'قطتي لا تأكل منذ يومين وتبدو خاملة جداً. لقد شربت بعض الماء لكن لم تلمس طعامها. هل هذا خطير؟',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T08:00:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Normal', 'MedicalQuestion', 0.84),
    },
    {
      id: 'msg-006-02',
      conversationId: 'conv-0000-0000-0000-000000000006',
      sender: 'Vet',
      senderUserId: 'user-vet-001',
      senderName: 'Dr. Sarah Johnson',
      body: 'مرحباً نورة. فقدان الشهية لأكثر من 48 ساعة مع الخمول يستوجب الفحص. يرجى إحضار قطتك للعيادة اليوم. هل لاحظت أي أعراض أخرى مثل القيء أو الإسهال؟',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T09:00:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-006-03',
      conversationId: 'conv-0000-0000-0000-000000000006',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Noura Al-Maktoum',
      body: 'نعم، قاءت مرة البارحة. لا إسهال. سأحضرها اليوم. في أي وقت يمكنني الحضور؟',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T09:30:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'msg-006-04',
      conversationId: 'conv-0000-0000-0000-000000000006',
      sender: 'Vet',
      senderUserId: 'user-vet-001',
      senderName: 'Dr. Sarah Johnson',
      body: 'يمكنك الحضور في أي وقت اليوم بين الساعة 10 صباحاً و8 مساءً. سنعطيك الأولوية نظراً لحالة قطتك. اطلبي من الاستقبال مراجعة قسم الطوارئ.',
      isInternalNote: false,
      sentAt: new Date('2026-03-10T10:00:00+04:00').toISOString(),
      attachments: [],
    },
  ],
  'conv-0000-0000-0000-000000000007': [
    {
      id: 'msg-007-01',
      conversationId: 'conv-0000-0000-0000-000000000007',
      sender: 'Owner',
      senderUserId: null,
      senderName: 'Reem Al-Suwaidi',
      body: 'Dear Desert Paws team, I just wanted to say thank you for the incredible care you gave Max during his surgery last week. Dr. Ahmed and the whole team were professional, kind, and thorough. Max is recovering beautifully. We are so grateful.',
      isInternalNote: false,
      sentAt: new Date('2026-03-08T15:00:00+04:00').toISOString(),
      attachments: [],
      classification: makeClassification('Low', 'Feedback', 0.97),
    },
  ],
}

// ─── AI Suggestions ───────────────────────────────────────────────────────────

export const MOCK_AI_SUGGESTIONS: Record<string, AiSuggestion[]> = {
  'conv-0000-0000-0000-000000000001': [
    {
      text: 'Thank you for contacting us, Ahmed. Limping combined with pain on weight-bearing can indicate a sprain, joint issue, or injury that requires prompt evaluation. Please bring Max to the clinic as soon as possible — we will prioritise your visit.',
      language: 'en',
    },
    {
      text: 'We understand your concern. Please come to the clinic today and we will examine Max thoroughly. If the limping is severe or Max cannot bear any weight, please call our emergency line immediately.',
      language: 'en',
    },
  ],
  'conv-0000-0000-0000-000000000003': [
    {
      text: 'Hello Mohammed! We have availability this week for Simba\'s vaccination. We have slots available on Wednesday at 3 PM and Thursday at 4 PM and 5 PM. Would either of those work for you?',
      language: 'en',
    },
    {
      text: 'Thank you for getting in touch. We can book Simba\'s annual vaccination this week. Our next available afternoon slots are Wednesday 15:00 or Thursday 16:00. Please let us know your preference and we will confirm.',
      language: 'en',
    },
    {
      text: 'Great news — Simba\'s due date fits perfectly with this week\'s schedule! We have afternoon slots on Wed, Thu, and Fri. Please select one and we will send you a confirmation.',
      language: 'en',
    },
  ],
  'conv-0000-0000-0000-000000000005': [
    {
      text: 'Hello Khalid! Microchipping at our clinic costs 150 AED and the procedure takes about 5 minutes. No anaesthesia is required. An appointment is recommended but we can accommodate walk-ins during off-peak hours.',
      language: 'en',
    },
  ],
  'conv-0000-0000-0000-000000000006': [
    {
      text: 'مرحباً نورة. يسعدنا مساعدتك. يمكنك الحضور في أي وقت يناسبك اليوم. فريق الاستقبال سيرتب لك موعداً فورياً مع الطبيبة سارة.',
      language: 'ar',
    },
  ],
}

// ─── AI Summaries ─────────────────────────────────────────────────────────────

export const MOCK_AI_SUMMARIES: Record<string, string> = {
  'conv-0000-0000-0000-000000000002': 'Owner Fatima Hassan reported post-surgical wound redness and discharge for patient Luna (cat, spay, 5 days post-op). Dr. Sarah Johnson examined the patient same-day and prescribed chlorhexidine wash and oral amoxicillin. The owner confirmed improvement at the 48-hour mark, reporting reduced swelling and normal appetite. Follow-up photo confirmed positive progress.',
  'conv-0000-0000-0000-000000000006': 'Owner Noura Al-Maktoum reported that her cat has not eaten for two days and appears lethargic, with one episode of vomiting. Dr. Sarah Johnson assessed the symptoms as requiring in-person examination and offered a priority same-day appointment. The owner confirmed she would bring the cat in and requested available times. Dr. Johnson confirmed availability from 10 AM onwards.',
}

// ─── Response Templates ───────────────────────────────────────────────────────

export const MOCK_TEMPLATES: ResponseTemplateDto[] = [
  {
    id: 'tmpl-0000-0000-0000-000000000001',
    clinicId: 'clinic-001',
    name: 'Appointment Confirmation',
    contentEn: 'Your appointment has been confirmed for [DATE] at [TIME]. Please arrive 10 minutes early. If you need to cancel, please let us know at least 24 hours in advance.',
    contentAr: 'تم تأكيد موعدك في [DATE] الساعة [TIME]. يرجى الحضور قبل 10 دقائق. إذا احتجت إلى الإلغاء، يرجى إبلاغنا قبل 24 ساعة على الأقل.',
    category: 'AppointmentRequest',
    createdAt: new Date('2026-01-15T08:00:00+04:00').toISOString(),
    updatedAt: new Date('2026-01-15T08:00:00+04:00').toISOString(),
  },
  {
    id: 'tmpl-0000-0000-0000-000000000002',
    clinicId: 'clinic-001',
    name: 'Opening Hours',
    contentEn: 'Our clinic is open Sunday to Thursday from 08:00 to 20:00, and Friday from 08:00 to 12:00. We are closed on Saturdays and UAE public holidays.',
    contentAr: 'عيادتنا مفتوحة من الأحد إلى الخميس من 08:00 إلى 20:00، والجمعة من 08:00 إلى 12:00. نحن مغلقون أيام السبت والعطلات الرسمية الإماراتية.',
    category: 'Administrative',
    createdAt: new Date('2026-01-15T08:00:00+04:00').toISOString(),
    updatedAt: new Date('2026-02-01T09:00:00+04:00').toISOString(),
  },
  {
    id: 'tmpl-0000-0000-0000-000000000003',
    clinicId: 'clinic-001',
    name: 'Vaccination Reminder',
    contentEn: 'Your pet is due for their annual vaccination. Please book an appointment at your earliest convenience to keep their protection up to date.',
    contentAr: 'حيوانك الأليف بحاجة إلى التطعيم السنوي. يرجى حجز موعد في أقرب وقت ممكن للحفاظ على حمايته.',
    category: null,
    createdAt: new Date('2026-02-01T09:00:00+04:00').toISOString(),
    updatedAt: new Date('2026-02-01T09:00:00+04:00').toISOString(),
  },
  {
    id: 'tmpl-0000-0000-0000-000000000004',
    clinicId: 'clinic-001',
    name: 'Post-Op Care Instructions',
    contentEn: 'Thank you for your message. Following the surgery, please monitor the incision site for redness, swelling, or discharge. Keep the e-collar on at all times. If you notice any concerning changes, please contact us immediately.',
    contentAr: 'شكراً لرسالتك. بعد الجراحة، يرجى مراقبة موضع الشق بحثاً عن احمرار أو تورم أو إفراز. احرص على إبقاء الياقة الواقية في جميع الأوقات. إذا لاحظت أي تغيرات مقلقة، يرجى الاتصال بنا فوراً.',
    category: 'PostOperativeFollowUp',
    createdAt: new Date('2026-02-10T10:00:00+04:00').toISOString(),
    updatedAt: new Date('2026-02-10T10:00:00+04:00').toISOString(),
  },
  {
    id: 'tmpl-0000-0000-0000-000000000005',
    clinicId: 'clinic-001',
    name: 'Out of Hours Acknowledgment',
    contentEn: 'Your message has been received. Our team will respond during our next business hours (Sunday–Thursday 08:00–20:00, Friday 08:00–12:00). For emergencies, please call our 24/7 emergency line.',
    contentAr: 'تم استلام رسالتك. سيرد فريقنا خلال ساعات العمل القادمة (الأحد–الخميس 08:00–20:00، الجمعة 08:00–12:00). في حالات الطوارئ، يرجى الاتصال بخطنا على مدار الساعة.',
    category: null,
    createdAt: new Date('2026-01-20T08:00:00+04:00').toISOString(),
    updatedAt: new Date('2026-01-20T08:00:00+04:00').toISOString(),
  },
]

// ─── Messaging Hours ──────────────────────────────────────────────────────────

export const MOCK_MESSAGING_HOURS: MessagingHoursDto[] = [
  { dayOfWeek: 0, openTime: '08:00', closeTime: '20:00', isClosed: false }, // Sunday
  { dayOfWeek: 1, openTime: '08:00', closeTime: '20:00', isClosed: false }, // Monday
  { dayOfWeek: 2, openTime: '08:00', closeTime: '20:00', isClosed: false }, // Tuesday
  { dayOfWeek: 3, openTime: '08:00', closeTime: '20:00', isClosed: false }, // Wednesday
  { dayOfWeek: 4, openTime: '08:00', closeTime: '20:00', isClosed: false }, // Thursday
  { dayOfWeek: 5, openTime: '08:00', closeTime: '12:00', isClosed: false }, // Friday (half day)
  { dayOfWeek: 6, openTime: '00:00', closeTime: '00:00', isClosed: true },  // Saturday (closed)
]

// ─── Portal data ──────────────────────────────────────────────────────────────

export const MOCK_PORTAL_CONVERSATIONS: PortalConversationDto[] = [
  {
    id: 'conv-0000-0000-0000-000000000001',
    subject: 'Max has been limping since this morning',
    category: 'MedicalUrgency',
    status: 'Open',
    lastMessageAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
    createdAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
    unreadByOwnerCount: 0,
  },
  {
    id: 'conv-portal-002',
    subject: 'Invoice question for last visit',
    category: 'Administrative',
    status: 'Resolved',
    lastMessageAt: new Date('2026-03-05T14:30:00+04:00').toISOString(),
    createdAt: new Date('2026-03-04T10:00:00+04:00').toISOString(),
    unreadByOwnerCount: 0,
  },
]

export const MOCK_PORTAL_MESSAGES: Record<string, PortalMessageDto[]> = {
  'conv-0000-0000-0000-000000000001': [
    {
      id: 'msg-001-01',
      sender: 'Owner',
      senderName: 'Ahmed Al-Rashid',
      body: 'Good morning. My dog Max has been limping since this morning and is whimpering when he puts weight on his front right leg. He did not fall or get injured that I know of. Should I bring him in today? This is urgent.',
      sentAt: new Date('2026-03-10T07:45:00+04:00').toISOString(),
      attachments: [],
    },
  ],
  'conv-portal-002': [
    {
      id: 'pmsg-002-01',
      sender: 'Owner',
      senderName: 'Ahmed Al-Rashid',
      body: 'Hello, I received an invoice but I am not sure it reflects the correct amount for the consultation. Can you clarify?',
      sentAt: new Date('2026-03-04T10:00:00+04:00').toISOString(),
      attachments: [],
    },
    {
      id: 'pmsg-002-02',
      sender: 'Staff',
      senderName: 'Desert Paws Clinic',
      body: 'Hello Ahmed! Thank you for getting in touch. The invoice INV-2026-001 includes the consultation fee (200 AED) plus the medication provided (50 AED). Please let us know if you have further questions.',
      sentAt: new Date('2026-03-05T14:30:00+04:00').toISOString(),
      attachments: [],
    },
  ],
}

export const MOCK_PORTAL_PETS: PortalPetDto[] = [
  {
    id: 'pat-0000-0000-0000-000000000001',
    name: 'Max',
    species: 'Dog',
    breed: 'Golden Retriever',
    ageYears: 6,
  },
]
