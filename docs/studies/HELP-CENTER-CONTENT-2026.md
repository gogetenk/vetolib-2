# Vetolib Help Center

> Last updated: March 2026
> Language: English (Arabic translation planned)
> Audience: Veterinary clinic staff (non-technical)

---

# Section 1 — Getting Started

---

## 1.1 How to Set Up Your Clinic

Welcome to Vetolib! Before you start booking appointments and seeing patients, take a few minutes to configure your clinic profile. This ensures that your calendar, invoices, and communications all reflect your practice accurately.

**Steps:**

1. Log in to Vetolib and navigate to **Settings > Clinic Profile**.
2. Enter your clinic name, address, phone number, and email. These details will appear on invoices and WhatsApp messages sent to pet owners.
3. Go to **Settings > Operating Hours** and set your weekly schedule. In the UAE, many clinics operate Sunday through Thursday — Vetolib supports any configuration, including split shifts and Friday-only hours.
4. Under **Settings > Team**, invite your veterinarians and staff by email. Assign each person a role: Admin, Veterinarian, or Receptionist. Each role has different permissions.
5. Navigate to **Settings > Consultation Types** and create the appointment types your clinic offers (e.g., General Consultation, Vaccination, Surgery, Grooming). Set a default duration and color for each — these colors will appear on your calendar.

Once your clinic is set up, you are ready to start adding patients and booking appointments. You can always return to Settings later to adjust hours, add new team members, or create additional consultation types.

---

## 1.2 Adding Your First Patient

Every visit in Vetolib starts with a patient record. A patient is the animal, linked to an owner (the pet parent). Creating a complete patient record from the start saves time later when writing medical notes or sending invoices.

**Steps:**

1. Click **Patients** in the main navigation, then click **+ New Patient**.
2. Fill in the owner's details: full name, phone number (with country code for WhatsApp), and email address.
3. Fill in the animal's details: name, species (dog, cat, bird, reptile, etc.), breed, date of birth or estimated age, sex, and weight.
4. Add any known allergies or chronic conditions in the **Medical Notes** field. This information will be visible to all vets during consultations.
5. Click **Save**. The patient now appears in your patient list and is ready for appointments.

If you have many patients to add at once, consider using the CSV Import feature described in the Features Guide section below.

---

## 1.3 Creating Your First Appointment

The calendar is the heart of Vetolib. Booking an appointment takes just a few clicks and immediately blocks the time slot for the assigned veterinarian.

**Steps:**

1. Click **Agenda** in the main navigation. You will see the calendar in day view by default.
2. Click on an empty time slot, or click the **+ New Appointment** button.
3. Search for the patient by owner name or animal name. If the patient does not exist yet, you can create one on the fly.
4. Select the **Consultation Type** (e.g., General Consultation, Vaccination). The duration will auto-fill based on your configuration, but you can adjust it.
5. Assign a **Veterinarian** from the dropdown. Only team members with the Vet role appear here.
6. Add an optional note (e.g., "Owner mentioned limping on right front leg").
7. Click **Confirm**. The appointment appears on the calendar in the color of its consultation type.

The pet owner will automatically receive a confirmation via WhatsApp if you have WhatsApp reminders enabled.

---

## 1.4 Sending Your First Invoice

Vetolib makes billing straightforward. You can generate an invoice directly from a completed consultation, or create a standalone invoice at any time.

**Steps:**

1. After completing a consultation, click the **Billing** tab in the patient's record, or navigate to **Billing** in the main menu.
2. Click **+ New Invoice**.
3. Select the patient (owner + animal). If you are coming from a consultation, this is pre-filled.
4. Add line items: consultation fees, medications dispensed, lab work, procedures, etc. Prices pull from your catalog if configured, or you can enter them manually.
5. VAT is calculated automatically based on UAE tax rules (currently 5%). You can see the subtotal, VAT amount, and total.
6. Click **Generate Invoice**. A PDF is created and stored in the patient's billing history.
7. To send the invoice to the pet owner, click **Send via WhatsApp** or **Send via Email**.

You can also mark invoices as paid, partially paid, or unpaid. Vetolib tracks outstanding balances per owner across all their animals.

---

## 1.5 Setting Up WhatsApp Reminders

WhatsApp is the primary communication channel for pet owners in the UAE. Vetolib integrates with WhatsApp to send appointment reminders, confirmations, and follow-ups automatically.

**Steps:**

1. Navigate to **Settings > Messaging**.
2. Follow the guided setup to connect your clinic's WhatsApp Business number. You will need to verify the number via a code sent by WhatsApp.
3. Once connected, enable the message types you want to send automatically:
   - **Appointment Confirmation** — sent immediately when an appointment is booked.
   - **Reminder (24h before)** — sent the day before the appointment.
   - **Reminder (1h before)** — sent one hour before the appointment.
   - **Follow-up** — sent a configurable number of days after the consultation.
4. Review the default message templates. You can customize the wording, but the templates must comply with WhatsApp Business API guidelines.
5. Click **Save**. Messages will now be sent automatically based on your settings.

You can also send manual one-off messages to any pet owner from the **Messaging** section or directly from a patient's profile.

---

# Section 2 — Features Guide

---

## 2.1 Using the Calendar (Day, Week, and Month Views)

The Vetolib calendar gives you three ways to view your schedule, so you can plan at any level of detail.

**Day View** shows a single day with time slots for each veterinarian in side-by-side columns. This is the best view when you are at the front desk managing the current day's appointments. You can see exactly who is available and when, drag appointments to reschedule them, and spot gaps in the schedule.

**Week View** displays the full week (Sunday through Saturday, or your configured work week). Appointments appear as colored blocks. This view is useful for planning ahead, spotting overbooking patterns, and ensuring balanced workloads across your team.

**Month View** provides a high-level overview. Each day shows a count of appointments and highlights days that are fully booked. Use this view for long-term planning, identifying slow periods, or scheduling surgeries weeks in advance.

To switch views, use the **Day / Week / Month** toggle at the top of the calendar. You can also filter the calendar by veterinarian if you only want to see one person's schedule. All views are color-coded by consultation type, making it easy to distinguish vaccinations from surgeries at a glance.

---

## 2.2 Managing Medical Records and Prescriptions

Every consultation in Vetolib generates a medical record that becomes part of the animal's permanent history. This ensures continuity of care, even when different vets see the same patient.

**During a consultation:**

1. Open the appointment from the calendar and click **Start Consultation**.
2. The medical record form opens with the patient's history visible on the side panel — previous visits, allergies, weight trends, and past prescriptions.
3. Fill in the consultation details: chief complaint, examination findings, diagnosis, and treatment plan.
4. To add a prescription, click **+ Prescription**. Search for medications from your drug catalog (linked to your stock), set the dosage, frequency, duration, and quantity.
5. Click **Complete Consultation**. The medical record is saved, stock is automatically decremented for dispensed medications, and the record appears in the patient's timeline.

**Accessing past records:**

Navigate to **Patients**, select a patient, and open the **Medical History** tab. All consultations are listed chronologically. You can filter by date range or search for specific diagnoses. Each record can be viewed, printed, or exported as a PDF.

Prescriptions are also tracked separately under the **Prescriptions** tab, making it easy to check what was previously prescribed without scrolling through full consultation notes.

---

## 2.3 AI SOAP Notes — How It Works

Vetolib's AI SOAP Notes feature helps veterinarians write structured consultation notes faster. Instead of typing everything from scratch, you speak or type a brief summary, and the AI generates a complete SOAP note (Subjective, Objective, Assessment, Plan).

**How to use it:**

1. During a consultation, click the **AI SOAP** button in the medical record editor.
2. Enter or dictate a brief description of the visit. For example: "3-year-old male Golden Retriever, owner reports vomiting for 2 days, mild dehydration on exam, suspect dietary indiscretion, prescribed metoclopramide and bland diet."
3. Click **Generate**. The AI structures your input into the four SOAP sections with professional medical language.
4. Review the generated note carefully. You can edit any section before saving.
5. Click **Accept** to insert the SOAP note into the medical record, or **Regenerate** if you want a different version.

**Important notes about AI SOAP:**

- The AI is a writing assistant, not a diagnostic tool. It structures your clinical observations — it does not make diagnoses or suggest treatments on its own.
- Every AI-generated note includes a small disclaimer at the bottom: "This note was structured with AI assistance and reviewed by the attending veterinarian."
- The AI never replaces your clinical judgment. Always review the generated text before saving.
- Your data is processed securely and is never used to train AI models.

---

## 2.4 Stock Management and Drug Catalog

Vetolib includes a built-in stock management system so you can track medications, vaccines, consumables, and supplies without a separate tool.

**Setting up your catalog:**

1. Navigate to **Stock > Drug Catalog**.
2. Click **+ Add Item** to create a new entry. Fill in the product name, category (medication, vaccine, consumable, etc.), unit (tablet, vial, ml, etc.), and default selling price.
3. For medications, you can also add the active ingredient, concentration, and manufacturer.

**Managing inventory:**

1. Go to **Stock > Inventory**. You will see all items with their current quantity, reorder threshold, and expiration dates.
2. To receive new stock, click **+ Stock Entry**, select the items, enter the quantity received, batch number, expiration date, and purchase price.
3. When medications are dispensed during a consultation (via prescriptions), stock is automatically decremented. You do not need to manually adjust quantities for dispensed items.
4. Vetolib alerts you when any item falls below its reorder threshold — you will see a notification badge on the Stock menu and an alert on the dashboard.

**Expiration tracking:**

Items approaching their expiration date (within 30 days by default) are highlighted in orange. Expired items are highlighted in red. You can configure the warning threshold in **Settings > Stock**.

---

## 2.5 Messaging with Pet Owners

The Messaging module in Vetolib centralizes all communication with pet owners. Whether messages are sent automatically (reminders, confirmations) or manually, they all appear in one threaded conversation per owner.

**Viewing conversations:**

Navigate to **Messaging** in the main menu. You will see a list of all conversations, sorted by most recent activity. Each conversation shows the owner's name, their pets, and a preview of the last message. Unread messages are highlighted.

**Sending a manual message:**

1. Open a conversation or start a new one by searching for an owner.
2. Type your message in the text box at the bottom.
3. Click **Send**. The message is delivered via WhatsApp to the owner's registered phone number.

**Automatic messages:**

Automatic messages (appointment confirmations, reminders, follow-ups) appear in the same conversation thread, clearly labeled as "Automatic." This gives you full visibility into what the owner has received.

**Message templates:**

For common messages you send frequently (e.g., "Your pet's lab results are ready"), you can create templates under **Settings > Messaging > Templates**. When composing a message, click **Use Template** to insert one, then personalize it before sending.

---

## 2.6 CSV Patient Import

If you are migrating from another system or from spreadsheets, Vetolib lets you import patients in bulk using a CSV file. This saves hours of manual data entry.

**Steps:**

1. Navigate to **Patients > Import**.
2. Download the **CSV template** provided. This template contains the required columns: owner name, owner phone, owner email, animal name, species, breed, date of birth, sex, and weight.
3. Fill in the template with your data. Each row represents one animal. If an owner has multiple animals, repeat the owner information on each row — Vetolib will automatically group them.
4. Save your file as CSV (UTF-8 encoding is recommended for Arabic names).
5. Upload the file on the import page and click **Preview**. Vetolib will show you a summary: number of owners, number of animals, and any rows with errors (e.g., missing required fields, invalid phone formats).
6. Fix any errors in your CSV and re-upload, or proceed with the valid rows.
7. Click **Import**. The patients are created and immediately available in your patient list.

Duplicate detection is built in: if an owner with the same phone number already exists, Vetolib will link the new animal to the existing owner rather than creating a duplicate.

---

## 2.7 Multi-Clinic Management

If you operate more than one clinic location, Vetolib's multi-clinic feature lets you manage all locations from a single account while keeping each clinic's data completely separate.

**How it works:**

Each clinic in Vetolib is an independent workspace with its own patients, appointments, medical records, stock, and billing. Data never leaks between clinics — a patient registered at Clinic A will not appear in Clinic B's patient list unless explicitly added there.

**Setting up a second clinic:**

1. Navigate to **Settings > Clinics** and click **+ Add Clinic**.
2. Enter the new clinic's details: name, address, operating hours, and team members.
3. Team members can belong to multiple clinics. When they log in, they select which clinic they are working in today from the clinic switcher in the top navigation bar.

**Switching between clinics:**

Click the clinic name in the top navigation bar. A dropdown shows all clinics you have access to. Select the one you want to work in. The entire interface — calendar, patients, stock, billing — updates to show that clinic's data.

**Cross-clinic reporting:**

Admins can access a consolidated dashboard under **Settings > Clinics > Overview** that shows key metrics (appointment count, revenue, stock alerts) across all locations side by side. This is useful for owners who want a high-level picture without switching clinics manually.

---

## 2.8 Subscription Plans and Billing

Vetolib offers several subscription plans tailored to clinics of different sizes. You can view, change, or manage your subscription at any time from the app.

**Available plans:**

- **Starter** — Ideal for solo practitioners or small clinics. Includes 1 veterinarian seat, core features (agenda, patients, medical records, billing), and WhatsApp messaging.
- **Professional** — For growing clinics. Includes multiple vet seats, stock management, AI SOAP Notes, CSV import, and priority support.
- **Enterprise** — For multi-clinic operations. Includes everything in Professional plus multi-clinic management, consolidated reporting, and a dedicated account manager.

**Managing your subscription:**

1. Navigate to **Settings > Subscription**.
2. You can see your current plan, billing cycle (monthly or annual), next payment date, and payment method on file.
3. To upgrade, click **Change Plan** and select the new plan. The upgrade takes effect immediately, and you are charged a prorated amount for the remainder of the current billing cycle.
4. To add vet seats, click **Add Seats**. Each additional seat is billed at the per-seat rate for your plan.
5. Invoices for your Vetolib subscription are available under **Settings > Subscription > Invoices**. These are your subscription invoices (not to be confused with invoices you send to pet owners).

All plans include a 14-day free trial. No credit card is required to start the trial. You can use all features during the trial period and choose a plan before it expires.

---

# Section 3 — Frequently Asked Questions

---

### 3.1 How much does Vetolib cost?

Vetolib offers three plans: Starter, Professional, and Enterprise. Pricing depends on the number of veterinarian seats and the features you need. All plans include a 14-day free trial with no credit card required. Visit the **Pricing** page on our website or contact our sales team for a detailed quote. Annual billing includes a discount compared to monthly billing.

---

### 3.2 Is my data secure?

Yes. Vetolib uses industry-standard encryption for data in transit (TLS 1.3) and at rest (AES-256). Your clinic's data is hosted in secure cloud infrastructure with regular backups. Each clinic's data is completely isolated from other clinics through our multi-tenant architecture — no other clinic can ever access your information. We comply with UAE data protection regulations.

---

### 3.3 Can I access Vetolib on my phone or tablet?

Vetolib is a web application optimized for both desktop and mobile browsers. You can access it from any device with a modern browser — Chrome, Safari, Edge, or Firefox. There is no separate app to download. The interface adapts to your screen size, so you can check the calendar, look up patient records, or send messages from your phone when you are away from the desk.

---

### 3.4 Does Vetolib support Arabic?

Yes. Vetolib supports both English and Arabic. You can switch the interface language from **Settings > Preferences > Language**. The interface fully supports right-to-left (RTL) layout when Arabic is selected. Patient names and notes can be entered in any language regardless of the interface language setting.

---

### 3.5 Can I import data from my previous software?

Yes. Vetolib supports CSV import for patient records (owners and animals). If you are migrating from another veterinary practice management system, export your data as CSV and use the import tool in Vetolib. See the **CSV Patient Import** article in the Features Guide above for step-by-step instructions. If you need help with the migration, our support team can assist with data mapping.

---

### 3.6 Can I export my data?

Yes. You can export patient lists, medical records, invoices, and stock reports as CSV or PDF files. Navigate to the relevant section and click the **Export** button. Your data belongs to you, and you can download it at any time.

---

### 3.7 Does Vetolib integrate with lab equipment or imaging systems?

At this time, Vetolib does not have direct integrations with laboratory analyzers or imaging systems (DICOM). We are evaluating partnerships for future releases. You can manually attach lab results and images to medical records by uploading files during a consultation.

---

### 3.8 Can multiple veterinarians use Vetolib at the same time?

Absolutely. Vetolib is designed for teams. Each veterinarian has their own login and sees their own calendar column. Multiple team members can be logged in simultaneously, working on different patients without any conflicts. The number of concurrent users depends on your subscription plan.

---

### 3.9 What happens if my internet goes down?

Vetolib is a cloud-based application and requires an internet connection to function. If your connection drops briefly, any unsaved work may be lost — we recommend saving frequently. We are exploring offline capabilities for a future release. In the meantime, we recommend having a mobile data backup (e.g., hotspot from your phone) for critical operations.

---

### 3.10 Can I customize my invoice template?

Yes. Navigate to **Settings > Billing > Invoice Template**. You can upload your clinic logo, set your preferred invoice numbering format, add your trade license number, and customize the footer text. The VAT registration number and calculation are handled automatically.

---

### 3.11 How do appointment reminders work?

When WhatsApp messaging is configured, Vetolib sends automatic reminders to pet owners before their appointments. You choose when reminders are sent (e.g., 24 hours before, 1 hour before) in **Settings > Messaging**. Reminders include the appointment date, time, clinic name, and the pet's name. Owners cannot reply to cancel through the automated message — they need to call the clinic.

---

### 3.12 Is there a limit on the number of patients I can add?

No. All Vetolib plans include unlimited patient records. There is no cap on the number of owners, animals, or medical records you can create. Your subscription is based on the number of veterinarian seats, not the volume of data.

---

### 3.13 Can I use Vetolib for exotic animals (birds, reptiles, etc.)?

Yes. Vetolib is not limited to cats and dogs. When creating a patient, you can select from a wide range of species including birds, reptiles, rabbits, hamsters, horses, and more. You can also add custom species if your practice sees unusual animals. The medical record templates work for all species.

---

### 3.14 How do I add a new team member?

Navigate to **Settings > Team** and click **+ Invite Member**. Enter their email address and assign a role (Admin, Veterinarian, or Receptionist). They will receive an invitation email with a link to create their account. Once they accept, they will appear in your team list and can be assigned to appointments.

---

### 3.15 Who do I contact for support?

You can reach the Vetolib support team through several channels:

- **In-app chat**: Click the help icon in the bottom-right corner of any page.
- **Email**: support@vetolib.com
- **WhatsApp**: Send a message to our support number (available on the website).

Our support team operates Sunday through Thursday, 9:00 AM to 6:00 PM Gulf Standard Time. We aim to respond to all inquiries within 2 hours during business hours.

---

# Section 4 — Troubleshooting

---

## 4.1 WhatsApp Messages Are Not Sending

If appointment confirmations, reminders, or manual messages are not being delivered to pet owners, follow these steps to diagnose the issue.

**Check your WhatsApp connection:**

1. Navigate to **Settings > Messaging**. Check the connection status indicator at the top. It should show **Connected** in green.
2. If it shows **Disconnected**, click **Reconnect** and follow the prompts to re-authenticate your WhatsApp Business number.

**Check the owner's phone number:**

1. Open the patient's profile and verify the owner's phone number. It must include the country code (e.g., +971 for UAE numbers). Numbers without a country code cannot be reached.
2. Make sure the phone number is a valid WhatsApp number. Not all phone numbers have WhatsApp enabled.

**Check the message log:**

1. Go to **Messaging** and open the conversation with the owner. Failed messages are marked with a red warning icon and an error description.
2. Common errors include "Number not on WhatsApp," "Rate limit exceeded" (too many messages in a short period), or "Template not approved" (for automatic messages).

If the connection is active, the number is correct, and you still cannot send messages, contact Vetolib support with the error message from the message log.

---

## 4.2 I Cannot See Appointments on the Calendar

If the calendar appears empty or some appointments are missing, the issue is usually a filter or permission setting.

**Check your filters:**

1. Look at the top of the calendar for any active filters. If a specific veterinarian is selected, you will only see their appointments. Click **All Vets** to reset the filter.
2. Check the date. Make sure you are looking at the correct day, week, or month. Use the date navigation arrows or click **Today** to jump to the current date.

**Check your clinic:**

If you work in multiple clinics, make sure you have the correct clinic selected in the clinic switcher (top navigation bar). Appointments from other clinics will not appear.

**Check your permissions:**

Receptionist accounts can see all appointments. Veterinarian accounts can also see all appointments by default. If your admin has restricted visibility, you may only see your own appointments. Contact your clinic administrator to adjust permissions if needed.

If none of the above resolves the issue, try refreshing the page (Ctrl+Shift+R or Cmd+Shift+R) to force a fresh load.

---

## 4.3 I Cannot Log In to My Account

Login problems are usually caused by incorrect credentials, an expired session, or a browser issue.

**Steps to resolve:**

1. **Check your email address.** Make sure you are using the exact email that was invited to the clinic. Vetolib accounts are case-insensitive but must match the invitation email.
2. **Reset your password.** On the login page, click **Forgot Password**. Enter your email and check your inbox (and spam folder) for the reset link. The link expires after 1 hour.
3. **Clear your browser cache.** Sometimes old session data causes login loops. Clear your browser's cookies and cache for the Vetolib domain, then try again.
4. **Try a different browser.** If the issue persists, try logging in from a different browser (e.g., Chrome instead of Safari) to rule out browser-specific problems.
5. **Check if your account is active.** If your clinic administrator has deactivated your account, you will see a message saying "Account deactivated." Contact your administrator to reactivate it.

If you still cannot log in after trying all steps, contact support with your email address and a screenshot of any error message you see.

---

## 4.4 Missing Patient Data After Import

If you imported patients via CSV and some data is missing or incorrect, the issue is usually in the CSV file format.

**Common causes:**

- **Encoding issues.** If patient or owner names contain Arabic characters and appear garbled, your CSV was likely saved in a non-UTF-8 encoding. Open the file in a text editor (e.g., Notepad++ or VS Code), re-save it as UTF-8, and re-import.
- **Wrong column mapping.** The import uses the column headers from the template to map data. If you renamed or reordered columns, some fields may have been imported into the wrong place. Download a fresh template and re-organize your data to match.
- **Missing required fields.** Rows with missing required fields (owner name, animal name, species) are skipped during import. Check the import summary for skipped rows and the reason for each skip.
- **Phone number format.** Phone numbers must include the country code (e.g., +971501234567). Numbers without a country code may be saved but will not work for WhatsApp messaging.

**To fix imported data:**

You can edit any patient record manually after import. Open the patient's profile, update the incorrect fields, and save. For large-scale corrections, it may be faster to delete the imported records and re-import with a corrected CSV.

---

## 4.5 Invoice PDF Is Not Generating

If you click **Generate Invoice** and the PDF does not appear, or you see an error, try the following steps.

**Steps to resolve:**

1. **Check for missing fields.** An invoice requires at least one line item with a description and amount. If all line items are empty, the PDF cannot be generated. Add at least one item and try again.
2. **Check your clinic profile.** The invoice PDF pulls your clinic name, address, and VAT number from **Settings > Clinic Profile**. If these fields are empty, the PDF generator may fail. Fill in the required fields and try again.
3. **Check your browser's popup blocker.** The PDF opens in a new tab. If your browser blocks popups, the PDF may have been generated but blocked from displaying. Look for a popup-blocked notification in your browser's address bar and allow popups for Vetolib.
4. **Try downloading instead of viewing.** If the PDF viewer is not loading, click the **Download** button (if available) instead of **View**. This saves the file directly to your computer.
5. **Refresh and retry.** If you see a generic error, refresh the page and try generating the invoice again. Temporary server issues can occasionally cause failures.

If the problem persists after trying all steps, navigate to **Billing**, find the invoice in the list, and check its status. If it shows "Generated," the PDF exists — try downloading it again. If it shows "Error," contact support with the invoice number.

---

*For additional help, contact the Vetolib support team via in-app chat, email (support@vetolib.com), or WhatsApp during business hours (Sun-Thu, 9 AM - 6 PM GST).*
