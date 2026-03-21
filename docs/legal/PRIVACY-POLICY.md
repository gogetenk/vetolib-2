# Vetolib — Privacy Policy

**Effective Date:** March 21, 2026
**Last Updated:** March 21, 2026

This Privacy Policy explains how **[Vetolib FZ-LLC]** ("Vetolib", "we", "us", or "our") collects, uses, stores, and protects personal data when you use the Vetolib platform ("Service").

We are committed to protecting your privacy in accordance with **UAE Federal Decree-Law No. 45/2021** on the Protection of Personal Data ("PDPL") and its implementing regulations.

---

## 1. Data We Collect

### 1.1 Clinic and Staff Data (Account Holders)

When a veterinary clinic registers and uses the Service, we collect:

| Data Category | Examples |
|---|---|
| **Clinic information** | Clinic name, address, phone, email, license number, VAT/TRN number |
| **Staff accounts** | Full name, email, phone, role (vet, assistant, receptionist), profile photo (optional) |
| **Authentication data** | Email, hashed password, session tokens, MFA settings |
| **Billing data** | Subscription plan, billing address, invoice history (payment card details are handled by Stripe — see Section 4) |

### 1.2 Pet Owner Data (Clients of the Clinic)

Clinics enter data about their clients (pet owners) into the Service:

| Data Category | Examples |
|---|---|
| **Contact information** | Full name, email, phone number, address |
| **Communication history** | Messages sent via the platform (WhatsApp, SMS, email), appointment reminders |
| **Billing records** | Invoices, payment status |

### 1.3 Animal Patient Data

The Service stores data about animal patients:

| Data Category | Examples |
|---|---|
| **Identity** | Name, species, breed, date of birth, sex, microchip number, photo |
| **Medical records** | Consultation notes, diagnoses, treatments, prescriptions, lab results, vaccination history, weight history |
| **Appointments** | Appointment history, cancellations, no-show records |

**Important:** Vetolib processes **animal health data**, not human health data. Animal medical records are not subject to human health data regulations (such as HIPAA). However, pet owner contact information constitutes personal data under the PDPL and is treated accordingly.

### 1.4 Usage and Technical Data

We automatically collect:

| Data Category | Examples |
|---|---|
| **Usage analytics** | Pages visited, features used, session duration (via PostHog — see Section 4) |
| **Device information** | Browser type, operating system, screen resolution |
| **Network data** | IP address, approximate location (country/city level) |
| **Error data** | Application errors and crash reports (via Sentry — see Section 4) |

---

## 2. Legal Basis for Processing

Under the UAE PDPL, we process personal data based on the following legal grounds:

| Legal Basis | Applies To |
|---|---|
| **Performance of a contract** (Art. 5) | Clinic/staff data — necessary to provide the Service under your subscription agreement |
| **Legitimate interest** (Art. 5) | Usage analytics — improving the Service, preventing fraud, ensuring security |
| **Consent** (Art. 5) | Marketing communications, optional analytics cookies, AI feature usage |
| **Legal obligation** (Art. 5) | Tax records retention, compliance with UAE regulatory requests |

**Pet owner data:** Clinics are the **data controllers** for their clients' personal data. Vetolib acts as a **data processor** on behalf of the clinic. Each clinic is responsible for obtaining appropriate consent or legal basis from their clients for storing their data in the Service.

---

## 3. How We Use Your Data

We use the data we collect to:

| Purpose | Data Used |
|---|---|
| **Provide the Service** | All clinic, staff, owner, and patient data — core platform functionality |
| **Process payments** | Billing data (forwarded to Stripe) |
| **Send transactional communications** | Email, SMS — appointment reminders, password resets, billing notifications |
| **Improve the Service** | Aggregated and anonymized usage analytics |
| **AI-assisted features** | Appointment data for scheduling optimization; consultation data for triage suggestions (only when enabled by the clinic) |
| **Security and fraud prevention** | Authentication data, IP addresses, access logs |
| **Legal compliance** | Tax records, regulatory responses |

We do **not**:

- Sell personal data to third parties.
- Use personal data for advertising or ad targeting.
- Use pet owner data for purposes unrelated to the clinic's veterinary services.
- Train AI models on individual clinic data without explicit opt-in consent.

---

## 4. Third-Party Data Sharing

We share data with the following third-party service providers, solely for the purposes described:

### 4.1 Stripe (Payment Processing)

- **Data shared:** Billing contact name, email, subscription details
- **Purpose:** Process subscription payments, generate invoices
- **Card data:** Entered directly into Stripe's PCI-DSS compliant forms. Vetolib **never** receives, stores, or processes payment card numbers.
- **Privacy policy:** [https://stripe.com/privacy](https://stripe.com/privacy)

### 4.2 WhatsApp / Meta (Messaging)

- **Data shared:** Pet owner phone number, message content (appointment reminders, clinic communications)
- **Purpose:** Deliver clinic-to-client messages via WhatsApp Business API
- **Note:** Only used when the clinic enables WhatsApp messaging and the pet owner's phone number is provided by the clinic
- **Privacy policy:** [https://www.whatsapp.com/legal/privacy-policy](https://www.whatsapp.com/legal/privacy-policy)

### 4.3 Sentry (Error Monitoring)

- **Data shared:** Application error reports, stack traces, browser/device info, IP address (anonymized)
- **Purpose:** Detect and fix software bugs
- **Personal data in errors:** We configure Sentry to strip personally identifiable information from error reports. Incidental PII in stack traces is automatically scrubbed.
- **Privacy policy:** [https://sentry.io/privacy/](https://sentry.io/privacy/)

### 4.4 PostHog (Product Analytics)

- **Data shared:** Anonymized usage events, page views, feature usage, device info
- **Purpose:** Understand how the Service is used, identify UX issues, measure feature adoption
- **Consent:** Analytics tracking is enabled only after the user provides cookie consent (see Section 8)
- **Hosting:** PostHog Cloud (EU/US) or self-hosted instance (to be determined based on PDPL requirements)
- **Privacy policy:** [https://posthog.com/privacy](https://posthog.com/privacy)

### 4.5 Cloud Infrastructure

- **Provider:** Cloud hosting in the **Middle East region** (UAE or nearest available region)
- **Data stored:** All Service data (databases, file storage, backups)
- **Encryption:** Data encrypted at rest (AES-256) and in transit (TLS 1.2+)

We do not share data with any other third parties unless required by law (see Section 10).

---

## 5. Data Retention

We retain data for the following periods:

| Data Type | Retention Period | Reason |
|---|---|---|
| **Active account data** | Duration of subscription | Service delivery |
| **Data after account cancellation** | 30 days | Grace period for data export |
| **Billing and tax records** | 5 years after transaction | UAE tax compliance (Federal Tax Authority) |
| **Application logs** | 90 days | Security monitoring and debugging |
| **Analytics data (PostHog)** | 24 months (anonymized) | Product improvement |
| **Error reports (Sentry)** | 90 days | Bug resolution |
| **Backup data** | 30 days after deletion from production | Disaster recovery |

After the retention period, data is permanently deleted or irreversibly anonymized.

---

## 6. Your Rights Under the PDPL

Under the UAE Personal Data Protection Law, you have the following rights:

### 6.1 Right of Access

You may request a copy of the personal data we hold about you. We will respond within **30 days** of receiving your request.

### 6.2 Right to Rectification

You may request correction of inaccurate or incomplete personal data. For data entered by clinics (pet owner records), please contact your veterinary clinic directly.

### 6.3 Right to Erasure

You may request deletion of your personal data, subject to:

- Legal retention obligations (e.g., tax records).
- Ongoing contractual obligations.
- Legitimate interests that override the request.

### 6.4 Right to Data Portability

You may request your data in a structured, commonly used, machine-readable format (JSON or CSV). Clinic administrators can also export data directly from the Service at any time.

### 6.5 Right to Restrict Processing

You may request that we limit the processing of your data to storage only, while a dispute about accuracy or legal basis is resolved.

### 6.6 Right to Object

You may object to processing based on legitimate interest. We will cease processing unless we demonstrate compelling legitimate grounds.

### 6.7 Right to Withdraw Consent

Where processing is based on consent (e.g., marketing, optional analytics), you may withdraw consent at any time. Withdrawal does not affect the lawfulness of processing performed before withdrawal.

### How to Exercise Your Rights

Send your request to **dpo@vetolib.com** with:

- Your full name and the clinic name
- The specific right you wish to exercise
- Any details that help us locate the relevant data

We will verify your identity before processing your request and respond within **30 days**. If a request is complex, we may extend this by an additional 30 days with notice.

---

## 7. Security Measures

We implement the following technical and organizational measures to protect your data:

### 7.1 Technical Measures

| Measure | Details |
|---|---|
| **Encryption at rest** | AES-256 encryption for all stored data |
| **Encryption in transit** | TLS 1.2+ for all network communications |
| **Multi-tenant isolation** | Database-level query filters ensure each clinic's data is logically isolated; no clinic can access another's data |
| **Authentication** | Bcrypt password hashing, optional multi-factor authentication (MFA) |
| **Access control** | Role-based access control (RBAC) — staff see only what their role permits |
| **Audit logging** | All data access and modifications are logged with timestamps and user identity |
| **Automated backups** | Daily encrypted backups with 30-day retention |

### 7.2 Organizational Measures

- Access to production systems is restricted to authorized personnel with a business need.
- All team members sign confidentiality agreements.
- Security incidents are investigated and affected users notified within **72 hours** as required by the PDPL.
- Regular security reviews and dependency vulnerability scanning.

### 7.3 Breach Notification

In the event of a personal data breach that poses a risk to your rights and freedoms:

- We will notify the **UAE Data Office** within **72 hours** of becoming aware of the breach.
- We will notify affected data subjects without undue delay if the breach is likely to result in a high risk to their rights.
- Notification will include: nature of the breach, data affected, measures taken, and contact information for our DPO.

---

## 8. Cookies and Analytics

### 8.1 Essential Cookies

We use strictly necessary cookies for:

- Session management (keeping you logged in)
- Security (CSRF protection)
- User preferences (language, theme)

These cookies do not require consent as they are necessary for the Service to function.

### 8.2 Analytics Cookies

We use **PostHog** for product analytics. Analytics cookies are:

- **Disabled by default** — only activated after you provide consent via our cookie banner.
- Used to understand feature usage and improve the Service.
- Not used for advertising or cross-site tracking.

You can withdraw cookie consent at any time through the cookie settings in the Service.

### 8.3 No Advertising Cookies

We do **not** use advertising cookies, retargeting pixels, or any third-party ad tracking.

---

## 9. International Data Transfers

### 9.1 Primary Data Location

All primary data is stored in the **Middle East region** (UAE or nearest available cloud region).

### 9.2 Third-Party Transfers

Some of our third-party processors (Stripe, Sentry, PostHog) may process data outside the UAE. Where this occurs:

- We ensure appropriate safeguards are in place as required by the PDPL (Art. 22), including:
  - Standard contractual clauses
  - Adequacy assessments of the receiving country's data protection framework
  - Data processing agreements with each processor
- We minimize the personal data transferred to what is strictly necessary for the service.

### 9.3 Your Consent

By using the Service, you acknowledge that limited data may be processed outside the UAE by our third-party processors as described in Section 4. You may contact us for details about specific transfer safeguards.

---

## 10. Law Enforcement and Legal Requests

We may disclose personal data if required by:

- A valid UAE court order or legal process.
- A request from a UAE regulatory authority with lawful jurisdiction.
- An urgent need to prevent fraud, security threats, or harm to individuals.

We will notify you of such requests where legally permitted and not prohibited by the court order.

---

## 11. Children's Data

The Service is designed for use by veterinary professionals and adult pet owners. We do not knowingly collect personal data from individuals under **18 years of age**.

If a clinic enters contact information for a minor pet owner (e.g., a family member), the clinic is responsible for ensuring appropriate parental or guardian consent has been obtained.

If we discover that we have inadvertently collected data from a minor without appropriate consent, we will delete it promptly. Please contact **dpo@vetolib.com** to report such cases.

---

## 12. AI Features and Automated Decision-Making

### 12.1 AI-Assisted Features

Vetolib may offer AI-assisted features including:

- **Scheduling optimization** — suggests optimal appointment times based on historical patterns
- **Triage suggestions** — suggests urgency levels based on reported symptoms
- **No-show prediction** — estimates the likelihood that an appointment will not be attended

### 12.2 Important Safeguards

- All AI features are **advisory only** — they do not make automated decisions that produce legal or significant effects on individuals.
- AI suggestions are always presented alongside a clear disclaimer that they do not replace professional veterinary judgment.
- No-show prediction scores are **never** displayed to pet owners. They are internal clinic tools only.
- Clinics may enable or disable AI features at any time in their settings.

### 12.3 Data Used by AI Features

AI features use aggregated and anonymized appointment data. Individual clinic data is not used to train shared models without explicit opt-in consent from the clinic administrator.

---

## 13. Changes to This Privacy Policy

We may update this Privacy Policy to reflect changes in our practices, technology, or legal requirements. When we do:

- We will notify you by email at least **30 days** before significant changes take effect.
- We will update the "Last Updated" date at the top of this document.
- We will post the revised policy on our website.

Continued use of the Service after the effective date of changes constitutes acceptance. If you disagree with the changes, you may cancel your account.

---

## 14. Data Protection Officer (DPO)

For any privacy-related questions, concerns, or requests, contact our Data Protection Officer:

**Data Protection Officer**
**[Vetolib FZ-LLC]**
Email: **dpo@vetolib.com**
Address: [Registered Address], Dubai, United Arab Emirates

We aim to respond to all inquiries within **5 business days** and to formal rights requests within **30 days**.

---

## 15. Supervisory Authority

If you believe we have not adequately addressed your privacy concerns, you have the right to lodge a complaint with the **UAE Data Office** (the competent authority under the PDPL).

---

## 16. Contact Us

**General inquiries:** support@vetolib.com
**Legal inquiries:** legal@vetolib.com
**Privacy and data requests:** dpo@vetolib.com

**[Vetolib FZ-LLC]**
[Registered Address]
Dubai, United Arab Emirates

---

*This Privacy Policy was last updated on March 21, 2026.*
