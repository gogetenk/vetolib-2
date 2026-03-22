"use client";

import { useState, useMemo } from "react";
import Link from "next/link";
import { Search, ChevronDown, BookOpen, Layers, HelpCircle, Wrench, ArrowLeft, ExternalLink } from "lucide-react";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

/* ------------------------------------------------------------------ */
/*  Article data – all 33 articles from the Help Center content       */
/* ------------------------------------------------------------------ */

interface Article {
  id: string;
  section: string;
  title: string;
  content: string;
}

const SECTIONS = [
  { key: "getting-started", label: "Getting Started", icon: BookOpen },
  { key: "features", label: "Features Guide", icon: Layers },
  { key: "faq", label: "FAQ", icon: HelpCircle },
  { key: "troubleshooting", label: "Troubleshooting", icon: Wrench },
] as const;

const ARTICLES: Article[] = [
  /* ── Getting Started ────────────────────────────────────────── */
  {
    id: "1-1",
    section: "getting-started",
    title: "How to Set Up Your Clinic",
    content: `Welcome to Vetara! Before you start booking appointments and seeing patients, take a few minutes to configure your clinic profile. This ensures that your calendar, invoices, and communications all reflect your practice accurately.

**Steps:**

1. Log in to Vetara and navigate to **Settings > Clinic Profile**.
2. Enter your clinic name, address, phone number, and email. These details will appear on invoices and WhatsApp messages sent to pet owners.
3. Go to **Settings > Operating Hours** and set your weekly schedule. In the UAE, many clinics operate Sunday through Thursday — Vetara supports any configuration, including split shifts and Friday-only hours.
4. Under **Settings > Team**, invite your veterinarians and staff by email. Assign each person a role: Admin, Veterinarian, or Receptionist. Each role has different permissions.
5. Navigate to **Settings > Consultation Types** and create the appointment types your clinic offers (e.g., General Consultation, Vaccination, Surgery, Grooming). Set a default duration and color for each — these colors will appear on your calendar.

Once your clinic is set up, you are ready to start adding patients and booking appointments. You can always return to Settings later to adjust hours, add new team members, or create additional consultation types.`,
  },
  {
    id: "1-2",
    section: "getting-started",
    title: "Adding Your First Patient",
    content: `Every visit in Vetara starts with a patient record. A patient is the animal, linked to an owner (the pet parent). Creating a complete patient record from the start saves time later when writing medical notes or sending invoices.

**Steps:**

1. Click **Patients** in the main navigation, then click **+ New Patient**.
2. Fill in the owner's details: full name, phone number (with country code for WhatsApp), and email address.
3. Fill in the animal's details: name, species (dog, cat, bird, reptile, etc.), breed, date of birth or estimated age, sex, and weight.
4. Add any known allergies or chronic conditions in the **Medical Notes** field. This information will be visible to all vets during consultations.
5. Click **Save**. The patient now appears in your patient list and is ready for appointments.

If you have many patients to add at once, consider using the CSV Import feature described in the Features Guide section.`,
  },
  {
    id: "1-3",
    section: "getting-started",
    title: "Creating Your First Appointment",
    content: `The calendar is the heart of Vetara. Booking an appointment takes just a few clicks and immediately blocks the time slot for the assigned veterinarian.

**Steps:**

1. Click **Agenda** in the main navigation. You will see the calendar in day view by default.
2. Click on an empty time slot, or click the **+ New Appointment** button.
3. Search for the patient by owner name or animal name. If the patient does not exist yet, you can create one on the fly.
4. Select the **Consultation Type** (e.g., General Consultation, Vaccination). The duration will auto-fill based on your configuration, but you can adjust it.
5. Assign a **Veterinarian** from the dropdown. Only team members with the Vet role appear here.
6. Add an optional note (e.g., "Owner mentioned limping on right front leg").
7. Click **Confirm**. The appointment appears on the calendar in the color of its consultation type.

The pet owner will automatically receive a confirmation via WhatsApp if you have WhatsApp reminders enabled.`,
  },
  {
    id: "1-4",
    section: "getting-started",
    title: "Sending Your First Invoice",
    content: `Vetara makes billing straightforward. You can generate an invoice directly from a completed consultation, or create a standalone invoice at any time.

**Steps:**

1. After completing a consultation, click the **Billing** tab in the patient's record, or navigate to **Billing** in the main menu.
2. Click **+ New Invoice**.
3. Select the patient (owner + animal). If you are coming from a consultation, this is pre-filled.
4. Add line items: consultation fees, medications dispensed, lab work, procedures, etc. Prices pull from your catalog if configured, or you can enter them manually.
5. VAT is calculated automatically based on UAE tax rules (currently 5%). You can see the subtotal, VAT amount, and total.
6. Click **Generate Invoice**. A PDF is created and stored in the patient's billing history.
7. To send the invoice to the pet owner, click **Send via WhatsApp** or **Send via Email**.

You can also mark invoices as paid, partially paid, or unpaid. Vetara tracks outstanding balances per owner across all their animals.`,
  },
  {
    id: "1-5",
    section: "getting-started",
    title: "Setting Up WhatsApp Reminders",
    content: `WhatsApp is the primary communication channel for pet owners in the UAE. Vetara integrates with WhatsApp to send appointment reminders, confirmations, and follow-ups automatically.

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

You can also send manual one-off messages to any pet owner from the **Messaging** section or directly from a patient's profile.`,
  },

  /* ── Features Guide ──────────────────────────────────────────── */
  {
    id: "2-1",
    section: "features",
    title: "Using the Calendar (Day, Week, and Month Views)",
    content: `The Vetara calendar gives you three ways to view your schedule, so you can plan at any level of detail.

**Day View** shows a single day with time slots for each veterinarian in side-by-side columns. This is the best view when you are at the front desk managing the current day's appointments. You can see exactly who is available and when, drag appointments to reschedule them, and spot gaps in the schedule.

**Week View** displays the full week (Sunday through Saturday, or your configured work week). Appointments appear as colored blocks. This view is useful for planning ahead, spotting overbooking patterns, and ensuring balanced workloads across your team.

**Month View** provides a high-level overview. Each day shows a count of appointments and highlights days that are fully booked. Use this view for long-term planning, identifying slow periods, or scheduling surgeries weeks in advance.

To switch views, use the **Day / Week / Month** toggle at the top of the calendar. You can also filter the calendar by veterinarian if you only want to see one person's schedule. All views are color-coded by consultation type, making it easy to distinguish vaccinations from surgeries at a glance.`,
  },
  {
    id: "2-2",
    section: "features",
    title: "Managing Medical Records and Prescriptions",
    content: `Every consultation in Vetara generates a medical record that becomes part of the animal's permanent history. This ensures continuity of care, even when different vets see the same patient.

**During a consultation:**

1. Open the appointment from the calendar and click **Start Consultation**.
2. The medical record form opens with the patient's history visible on the side panel — previous visits, allergies, weight trends, and past prescriptions.
3. Fill in the consultation details: chief complaint, examination findings, diagnosis, and treatment plan.
4. To add a prescription, click **+ Prescription**. Search for medications from your drug catalog (linked to your stock), set the dosage, frequency, duration, and quantity.
5. Click **Complete Consultation**. The medical record is saved, stock is automatically decremented for dispensed medications, and the record appears in the patient's timeline.

**Accessing past records:**

Navigate to **Patients**, select a patient, and open the **Medical History** tab. All consultations are listed chronologically. You can filter by date range or search for specific diagnoses. Each record can be viewed, printed, or exported as a PDF.

Prescriptions are also tracked separately under the **Prescriptions** tab, making it easy to check what was previously prescribed without scrolling through full consultation notes.`,
  },
  {
    id: "2-3",
    section: "features",
    title: "AI SOAP Notes — How It Works",
    content: `Vetara's AI SOAP Notes feature helps veterinarians write structured consultation notes faster. Instead of typing everything from scratch, you speak or type a brief summary, and the AI generates a complete SOAP note (Subjective, Objective, Assessment, Plan).

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
- Your data is processed securely and is never used to train AI models.`,
  },
  {
    id: "2-4",
    section: "features",
    title: "Stock Management and Drug Catalog",
    content: `Vetara includes a built-in stock management system so you can track medications, vaccines, consumables, and supplies without a separate tool.

**Setting up your catalog:**

1. Navigate to **Stock > Drug Catalog**.
2. Click **+ Add Item** to create a new entry. Fill in the product name, category (medication, vaccine, consumable, etc.), unit (tablet, vial, ml, etc.), and default selling price.
3. For medications, you can also add the active ingredient, concentration, and manufacturer.

**Managing inventory:**

1. Go to **Stock > Inventory**. You will see all items with their current quantity, reorder threshold, and expiration dates.
2. To receive new stock, click **+ Stock Entry**, select the items, enter the quantity received, batch number, expiration date, and purchase price.
3. When medications are dispensed during a consultation (via prescriptions), stock is automatically decremented. You do not need to manually adjust quantities for dispensed items.
4. Vetara alerts you when any item falls below its reorder threshold — you will see a notification badge on the Stock menu and an alert on the dashboard.

**Expiration tracking:**

Items approaching their expiration date (within 30 days by default) are highlighted in orange. Expired items are highlighted in red. You can configure the warning threshold in **Settings > Stock**.`,
  },
  {
    id: "2-5",
    section: "features",
    title: "Messaging with Pet Owners",
    content: `The Messaging module in Vetara centralizes all communication with pet owners. Whether messages are sent automatically (reminders, confirmations) or manually, they all appear in one threaded conversation per owner.

**Viewing conversations:**

Navigate to **Messaging** in the main menu. You will see a list of all conversations, sorted by most recent activity. Each conversation shows the owner's name, their pets, and a preview of the last message. Unread messages are highlighted.

**Sending a manual message:**

1. Open a conversation or start a new one by searching for an owner.
2. Type your message in the text box at the bottom.
3. Click **Send**. The message is delivered via WhatsApp to the owner's registered phone number.

**Automatic messages:**

Automatic messages (appointment confirmations, reminders, follow-ups) appear in the same conversation thread, clearly labeled as "Automatic." This gives you full visibility into what the owner has received.

**Message templates:**

For common messages you send frequently (e.g., "Your pet's lab results are ready"), you can create templates under **Settings > Messaging > Templates**. When composing a message, click **Use Template** to insert one, then personalize it before sending.`,
  },
  {
    id: "2-6",
    section: "features",
    title: "CSV Patient Import",
    content: `If you are migrating from another system or from spreadsheets, Vetara lets you import patients in bulk using a CSV file. This saves hours of manual data entry.

**Steps:**

1. Navigate to **Patients > Import**.
2. Download the **CSV template** provided. This template contains the required columns: owner name, owner phone, owner email, animal name, species, breed, date of birth, sex, and weight.
3. Fill in the template with your data. Each row represents one animal. If an owner has multiple animals, repeat the owner information on each row — Vetara will automatically group them.
4. Save your file as CSV (UTF-8 encoding is recommended for Arabic names).
5. Upload the file on the import page and click **Preview**. Vetara will show you a summary: number of owners, number of animals, and any rows with errors (e.g., missing required fields, invalid phone formats).
6. Fix any errors in your CSV and re-upload, or proceed with the valid rows.
7. Click **Import**. The patients are created and immediately available in your patient list.

Duplicate detection is built in: if an owner with the same phone number already exists, Vetara will link the new animal to the existing owner rather than creating a duplicate.`,
  },
  {
    id: "2-7",
    section: "features",
    title: "Multi-Clinic Management",
    content: `If you operate more than one clinic location, Vetara's multi-clinic feature lets you manage all locations from a single account while keeping each clinic's data completely separate.

**How it works:**

Each clinic in Vetara is an independent workspace with its own patients, appointments, medical records, stock, and billing. Data never leaks between clinics — a patient registered at Clinic A will not appear in Clinic B's patient list unless explicitly added there.

**Setting up a second clinic:**

1. Navigate to **Settings > Clinics** and click **+ Add Clinic**.
2. Enter the new clinic's details: name, address, operating hours, and team members.
3. Team members can belong to multiple clinics. When they log in, they select which clinic they are working in today from the clinic switcher in the top navigation bar.

**Switching between clinics:**

Click the clinic name in the top navigation bar. A dropdown shows all clinics you have access to. Select the one you want to work in. The entire interface — calendar, patients, stock, billing — updates to show that clinic's data.

**Cross-clinic reporting:**

Admins can access a consolidated dashboard under **Settings > Clinics > Overview** that shows key metrics (appointment count, revenue, stock alerts) across all locations side by side. This is useful for owners who want a high-level picture without switching clinics manually.`,
  },
  {
    id: "2-8",
    section: "features",
    title: "Subscription Plans and Billing",
    content: `Vetara offers several subscription plans tailored to clinics of different sizes. You can view, change, or manage your subscription at any time from the app.

**Available plans:**

- **Starter** — Ideal for solo practitioners or small clinics. Includes 1 veterinarian seat, core features (agenda, patients, medical records, billing), and WhatsApp messaging.
- **Professional** — For growing clinics. Includes multiple vet seats, stock management, AI SOAP Notes, CSV import, and priority support.
- **Enterprise** — For multi-clinic operations. Includes everything in Professional plus multi-clinic management, consolidated reporting, and a dedicated account manager.

**Managing your subscription:**

1. Navigate to **Settings > Subscription**.
2. You can see your current plan, billing cycle (monthly or annual), next payment date, and payment method on file.
3. To upgrade, click **Change Plan** and select the new plan. The upgrade takes effect immediately, and you are charged a prorated amount for the remainder of the current billing cycle.
4. To add vet seats, click **Add Seats**. Each additional seat is billed at the per-seat rate for your plan.
5. Invoices for your Vetara subscription are available under **Settings > Subscription > Invoices**. These are your subscription invoices (not to be confused with invoices you send to pet owners).

All plans include a 14-day free trial. No credit card is required to start the trial. You can use all features during the trial period and choose a plan before it expires.`,
  },

  /* ── FAQ ──────────────────────────────────────────────────────── */
  {
    id: "3-1",
    section: "faq",
    title: "How much does Vetara cost?",
    content: `Vetara offers three plans: Starter, Professional, and Enterprise. Pricing depends on the number of veterinarian seats and the features you need. All plans include a 14-day free trial with no credit card required. Visit the **Pricing** page on our website or contact our sales team for a detailed quote. Annual billing includes a discount compared to monthly billing.`,
  },
  {
    id: "3-2",
    section: "faq",
    title: "Is my data secure?",
    content: `Yes. Vetara uses industry-standard encryption for data in transit (TLS 1.3) and at rest (AES-256). Your clinic's data is hosted in secure cloud infrastructure with regular backups. Each clinic's data is completely isolated from other clinics through our multi-tenant architecture — no other clinic can ever access your information. We comply with UAE data protection regulations.`,
  },
  {
    id: "3-3",
    section: "faq",
    title: "Can I access Vetara on my phone or tablet?",
    content: `Vetara is a web application optimized for both desktop and mobile browsers. You can access it from any device with a modern browser — Chrome, Safari, Edge, or Firefox. There is no separate app to download. The interface adapts to your screen size, so you can check the calendar, look up patient records, or send messages from your phone when you are away from the desk.`,
  },
  {
    id: "3-4",
    section: "faq",
    title: "Does Vetara support Arabic?",
    content: `Yes. Vetara supports both English and Arabic. You can switch the interface language from **Settings > Preferences > Language**. The interface fully supports right-to-left (RTL) layout when Arabic is selected. Patient names and notes can be entered in any language regardless of the interface language setting.`,
  },
  {
    id: "3-5",
    section: "faq",
    title: "Can I import data from my previous software?",
    content: `Yes. Vetara supports CSV import for patient records (owners and animals). If you are migrating from another veterinary practice management system, export your data as CSV and use the import tool in Vetara. See the **CSV Patient Import** article in the Features Guide above for step-by-step instructions. If you need help with the migration, our support team can assist with data mapping.`,
  },
  {
    id: "3-6",
    section: "faq",
    title: "Can I export my data?",
    content: `Yes. You can export patient lists, medical records, invoices, and stock reports as CSV or PDF files. Navigate to the relevant section and click the **Export** button. Your data belongs to you, and you can download it at any time.`,
  },
  {
    id: "3-7",
    section: "faq",
    title: "Does Vetara integrate with lab equipment or imaging systems?",
    content: `At this time, Vetara does not have direct integrations with laboratory analyzers or imaging systems (DICOM). We are evaluating partnerships for future releases. You can manually attach lab results and images to medical records by uploading files during a consultation.`,
  },
  {
    id: "3-8",
    section: "faq",
    title: "Can multiple veterinarians use Vetara at the same time?",
    content: `Absolutely. Vetara is designed for teams. Each veterinarian has their own login and sees their own calendar column. Multiple team members can be logged in simultaneously, working on different patients without any conflicts. The number of concurrent users depends on your subscription plan.`,
  },
  {
    id: "3-9",
    section: "faq",
    title: "What happens if my internet goes down?",
    content: `Vetara is a cloud-based application and requires an internet connection to function. If your connection drops briefly, any unsaved work may be lost — we recommend saving frequently. We are exploring offline capabilities for a future release. In the meantime, we recommend having a mobile data backup (e.g., hotspot from your phone) for critical operations.`,
  },
  {
    id: "3-10",
    section: "faq",
    title: "Can I customize my invoice template?",
    content: `Yes. Navigate to **Settings > Billing > Invoice Template**. You can upload your clinic logo, set your preferred invoice numbering format, add your trade license number, and customize the footer text. The VAT registration number and calculation are handled automatically.`,
  },
  {
    id: "3-11",
    section: "faq",
    title: "How do appointment reminders work?",
    content: `When WhatsApp messaging is configured, Vetara sends automatic reminders to pet owners before their appointments. You choose when reminders are sent (e.g., 24 hours before, 1 hour before) in **Settings > Messaging**. Reminders include the appointment date, time, clinic name, and the pet's name. Owners cannot reply to cancel through the automated message — they need to call the clinic.`,
  },
  {
    id: "3-12",
    section: "faq",
    title: "Is there a limit on the number of patients I can add?",
    content: `No. All Vetara plans include unlimited patient records. There is no cap on the number of owners, animals, or medical records you can create. Your subscription is based on the number of veterinarian seats, not the volume of data.`,
  },
  {
    id: "3-13",
    section: "faq",
    title: "Can I use Vetara for exotic animals (birds, reptiles, etc.)?",
    content: `Yes. Vetara is not limited to cats and dogs. When creating a patient, you can select from a wide range of species including birds, reptiles, rabbits, hamsters, horses, and more. You can also add custom species if your practice sees unusual animals. The medical record templates work for all species.`,
  },
  {
    id: "3-14",
    section: "faq",
    title: "How do I add a new team member?",
    content: `Navigate to **Settings > Team** and click **+ Invite Member**. Enter their email address and assign a role (Admin, Veterinarian, or Receptionist). They will receive an invitation email with a link to create their account. Once they accept, they will appear in your team list and can be assigned to appointments.`,
  },
  {
    id: "3-15",
    section: "faq",
    title: "Who do I contact for support?",
    content: `You can reach the Vetara support team through several channels:

- **In-app chat**: Click the help icon in the bottom-right corner of any page.
- **Email**: support@vetara.ae
- **WhatsApp**: Send a message to our support number (available on the website).

Our support team operates Sunday through Thursday, 9:00 AM to 6:00 PM Gulf Standard Time. We aim to respond to all inquiries within 2 hours during business hours.`,
  },

  /* ── Troubleshooting ─────────────────────────────────────────── */
  {
    id: "4-1",
    section: "troubleshooting",
    title: "WhatsApp Messages Are Not Sending",
    content: `If appointment confirmations, reminders, or manual messages are not being delivered to pet owners, follow these steps to diagnose the issue.

**Check your WhatsApp connection:**

1. Navigate to **Settings > Messaging**. Check the connection status indicator at the top. It should show **Connected** in green.
2. If it shows **Disconnected**, click **Reconnect** and follow the prompts to re-authenticate your WhatsApp Business number.

**Check the owner's phone number:**

1. Open the patient's profile and verify the owner's phone number. It must include the country code (e.g., +971 for UAE numbers). Numbers without a country code cannot be reached.
2. Make sure the phone number is a valid WhatsApp number. Not all phone numbers have WhatsApp enabled.

**Check the message log:**

1. Go to **Messaging** and open the conversation with the owner. Failed messages are marked with a red warning icon and an error description.
2. Common errors include "Number not on WhatsApp," "Rate limit exceeded" (too many messages in a short period), or "Template not approved" (for automatic messages).

If the connection is active, the number is correct, and you still cannot send messages, contact Vetara support with the error message from the message log.`,
  },
  {
    id: "4-2",
    section: "troubleshooting",
    title: "I Cannot See Appointments on the Calendar",
    content: `If the calendar appears empty or some appointments are missing, the issue is usually a filter or permission setting.

**Check your filters:**

1. Look at the top of the calendar for any active filters. If a specific veterinarian is selected, you will only see their appointments. Click **All Vets** to reset the filter.
2. Check the date. Make sure you are looking at the correct day, week, or month. Use the date navigation arrows or click **Today** to jump to the current date.

**Check your clinic:**

If you work in multiple clinics, make sure you have the correct clinic selected in the clinic switcher (top navigation bar). Appointments from other clinics will not appear.

**Check your permissions:**

Receptionist accounts can see all appointments. Veterinarian accounts can also see all appointments by default. If your admin has restricted visibility, you may only see your own appointments. Contact your clinic administrator to adjust permissions if needed.

If none of the above resolves the issue, try refreshing the page (Ctrl+Shift+R or Cmd+Shift+R) to force a fresh load.`,
  },
  {
    id: "4-3",
    section: "troubleshooting",
    title: "I Cannot Log In to My Account",
    content: `Login problems are usually caused by incorrect credentials, an expired session, or a browser issue.

**Steps to resolve:**

1. **Check your email address.** Make sure you are using the exact email that was invited to the clinic. Vetara accounts are case-insensitive but must match the invitation email.
2. **Reset your password.** On the login page, click **Forgot Password**. Enter your email and check your inbox (and spam folder) for the reset link. The link expires after 1 hour.
3. **Clear your browser cache.** Sometimes old session data causes login loops. Clear your browser's cookies and cache for the Vetara domain, then try again.
4. **Try a different browser.** If the issue persists, try logging in from a different browser (e.g., Chrome instead of Safari) to rule out browser-specific problems.
5. **Check if your account is active.** If your clinic administrator has deactivated your account, you will see a message saying "Account deactivated." Contact your administrator to reactivate it.

If you still cannot log in after trying all steps, contact support with your email address and a screenshot of any error message you see.`,
  },
  {
    id: "4-4",
    section: "troubleshooting",
    title: "Missing Patient Data After Import",
    content: `If you imported patients via CSV and some data is missing or incorrect, the issue is usually in the CSV file format.

**Common causes:**

- **Encoding issues.** If patient or owner names contain Arabic characters and appear garbled, your CSV was likely saved in a non-UTF-8 encoding. Open the file in a text editor (e.g., Notepad++ or VS Code), re-save it as UTF-8, and re-import.
- **Wrong column mapping.** The import uses the column headers from the template to map data. If you renamed or reordered columns, some fields may have been imported into the wrong place. Download a fresh template and re-organize your data to match.
- **Missing required fields.** Rows with missing required fields (owner name, animal name, species) are skipped during import. Check the import summary for skipped rows and the reason for each skip.
- **Phone number format.** Phone numbers must include the country code (e.g., +971501234567). Numbers without a country code may be saved but will not work for WhatsApp messaging.

**To fix imported data:**

You can edit any patient record manually after import. Open the patient's profile, update the incorrect fields, and save. For large-scale corrections, it may be faster to delete the imported records and re-import with a corrected CSV.`,
  },
  {
    id: "4-5",
    section: "troubleshooting",
    title: "Invoice PDF Is Not Generating",
    content: `If you click **Generate Invoice** and the PDF does not appear, or you see an error, try the following steps.

**Steps to resolve:**

1. **Check for missing fields.** An invoice requires at least one line item with a description and amount. If all line items are empty, the PDF cannot be generated. Add at least one item and try again.
2. **Check your clinic profile.** The invoice PDF pulls your clinic name, address, and VAT number from **Settings > Clinic Profile**. If these fields are empty, the PDF generator may fail. Fill in the required fields and try again.
3. **Check your browser's popup blocker.** The PDF opens in a new tab. If your browser blocks popups, the PDF may have been generated but blocked from displaying. Look for a popup-blocked notification in your browser's address bar and allow popups for Vetara.
4. **Try downloading instead of viewing.** If the PDF viewer is not loading, click the **Download** button (if available) instead of **View**. This saves the file directly to your computer.
5. **Refresh and retry.** If you see a generic error, refresh the page and try generating the invoice again. Temporary server issues can occasionally cause failures.

If the problem persists after trying all steps, navigate to **Billing**, find the invoice in the list, and check its status. If it shows "Generated," the PDF exists — try downloading it again. If it shows "Error," contact support with the invoice number.`,
  },
];

/* ------------------------------------------------------------------ */
/*  Simple markdown-like renderer for bold text                       */
/* ------------------------------------------------------------------ */

function renderContent(text: string) {
  // Split into paragraphs
  const paragraphs = text.split("\n\n");

  return paragraphs.map((para, i) => {
    const trimmed = para.trim();
    if (!trimmed) return null;

    // Check if it's a numbered list
    if (/^\d+\.\s/.test(trimmed)) {
      const items = trimmed.split(/\n/).filter((l) => l.trim());
      return (
        <ol key={i} className="list-decimal space-y-1.5 pl-6 text-stone-700">
          {items.map((item, j) => (
            <li key={j} className="leading-relaxed">
              <span
                dangerouslySetInnerHTML={{
                  __html: item
                    .replace(/^\d+\.\s*/, "")
                    .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
                }}
              />
            </li>
          ))}
        </ol>
      );
    }

    // Check if it's a bulleted list
    if (/^[-*]\s/.test(trimmed) || /^\s+[-*]\s/.test(trimmed)) {
      const items = trimmed.split(/\n/).filter((l) => l.trim());
      return (
        <ul key={i} className="list-disc space-y-1.5 pl-6 text-stone-700">
          {items.map((item, j) => (
            <li key={j} className="leading-relaxed">
              <span
                dangerouslySetInnerHTML={{
                  __html: item
                    .replace(/^\s*[-*]\s*/, "")
                    .replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
                }}
              />
            </li>
          ))}
        </ul>
      );
    }

    // Bold header lines (e.g., "**Steps:**")
    if (/^\*\*.*\*\*$/.test(trimmed) || /^\*\*.*:\*\*$/.test(trimmed)) {
      return (
        <h4
          key={i}
          className="mt-2 text-sm font-semibold text-stone-900"
          dangerouslySetInnerHTML={{
            __html: trimmed.replace(/\*\*(.*?)\*\*/g, "$1"),
          }}
        />
      );
    }

    // Regular paragraph
    return (
      <p
        key={i}
        className="leading-relaxed text-stone-700"
        dangerouslySetInnerHTML={{
          __html: trimmed.replace(/\*\*(.*?)\*\*/g, "<strong>$1</strong>"),
        }}
      />
    );
  });
}

/* ------------------------------------------------------------------ */
/*  Help Center Page Component                                        */
/* ------------------------------------------------------------------ */

export default function HelpCenterPage() {
  const [searchQuery, setSearchQuery] = useState("");
  const [activeSection, setActiveSection] = useState<string | null>(null);
  const [activeArticleId, setActiveArticleId] = useState<string | null>(null);
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  // Filter articles based on search
  const filteredArticles = useMemo(() => {
    if (!searchQuery.trim()) return ARTICLES;
    const q = searchQuery.toLowerCase();
    return ARTICLES.filter(
      (a) =>
        a.title.toLowerCase().includes(q) ||
        a.content.toLowerCase().includes(q)
    );
  }, [searchQuery]);

  // Group filtered articles by section
  const groupedArticles = useMemo(() => {
    const groups: Record<string, Article[]> = {};
    for (const article of filteredArticles) {
      if (!groups[article.section]) groups[article.section] = [];
      groups[article.section].push(article);
    }
    return groups;
  }, [filteredArticles]);

  // Currently selected article
  const activeArticle = activeArticleId
    ? ARTICLES.find((a) => a.id === activeArticleId) ?? null
    : null;

  // Visible sections (either filtered or all)
  const visibleSections = SECTIONS.filter(
    (s) => groupedArticles[s.key] && groupedArticles[s.key].length > 0
  );

  function handleArticleClick(articleId: string) {
    setActiveArticleId(articleId);
    setMobileNavOpen(false);
  }

  function handleSectionClick(sectionKey: string) {
    setActiveSection(activeSection === sectionKey ? null : sectionKey);
    setActiveArticleId(null);
  }

  function handleBackToList() {
    setActiveArticleId(null);
  }

  /* ── Sidebar content (shared between desktop and mobile) ── */
  const sidebarContent = (
    <nav className="space-y-1" data-testid="help-sidebar-nav">
      {visibleSections.map((section) => {
        const Icon = section.icon;
        const isExpanded =
          activeSection === section.key ||
          !!activeArticle?.section ||
          searchQuery.trim().length > 0;
        const sectionArticles = groupedArticles[section.key] || [];

        return (
          <div key={section.key}>
            <button
              onClick={() => handleSectionClick(section.key)}
              className={`flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                activeSection === section.key ||
                activeArticle?.section === section.key
                  ? "bg-primary/5 text-primary"
                  : "text-stone-600 hover:bg-stone-50 hover:text-stone-900"
              }`}
              data-testid={`help-section-${section.key}`}
            >
              <Icon className="h-4 w-4 shrink-0" />
              <span className="flex-1 text-left">{section.label}</span>
              <ChevronDown
                className={`h-3.5 w-3.5 shrink-0 transition-transform ${
                  isExpanded && (activeSection === section.key || searchQuery)
                    ? "rotate-180"
                    : ""
                }`}
              />
            </button>
            {(activeSection === section.key || searchQuery.trim().length > 0) &&
              sectionArticles.length > 0 && (
                <div className="ml-4 mt-1 space-y-0.5 border-l border-stone-200 pl-3">
                  {sectionArticles.map((article) => (
                    <button
                      key={article.id}
                      onClick={() => handleArticleClick(article.id)}
                      className={`block w-full rounded-md px-2.5 py-1.5 text-left text-sm transition-colors ${
                        activeArticleId === article.id
                          ? "bg-primary/5 font-medium text-primary"
                          : "text-stone-500 hover:bg-stone-50 hover:text-stone-700"
                      }`}
                      data-testid={`help-article-${article.id}`}
                    >
                      {article.title}
                    </button>
                  ))}
                </div>
              )}
          </div>
        );
      })}

      {visibleSections.length === 0 && searchQuery.trim().length > 0 && (
        <p className="px-3 py-4 text-sm text-stone-400" data-testid="help-no-results">
          No articles found for &quot;{searchQuery}&quot;
        </p>
      )}
    </nav>
  );

  return (
    <div className="min-h-screen bg-white">
      {/* ── Header ── */}
      <header className="sticky top-0 z-50 border-b border-stone-100 bg-white/80 backdrop-blur-md">
        <nav className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <Link
            href="/"
            className="text-xl font-bold tracking-tight text-primary"
            data-testid="help-nav-logo"
          >
            Vetara
          </Link>
          <div className="flex items-center gap-4">
            <Link
              href="/"
              className="text-sm font-medium text-stone-600 transition-colors hover:text-primary"
              data-testid="help-nav-home"
            >
              Home
            </Link>
            <span
              className="text-sm font-medium text-primary"
              data-testid="help-nav-help"
            >
              Help Center
            </span>
          </div>
        </nav>
      </header>

      {/* ── Hero ── */}
      <section className="border-b border-stone-100 bg-gradient-to-br from-primary/5 via-white to-teal-50 py-12 sm:py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center">
            <h1
              className="text-3xl font-bold tracking-tight text-stone-900 sm:text-4xl"
              data-testid="help-title"
            >
              Help Center
            </h1>
            <p className="mt-3 text-lg text-stone-600">
              Find answers, guides, and troubleshooting tips to get the most out
              of Vetara.
            </p>
            <div className="relative mx-auto mt-6 max-w-md">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-stone-400" />
              <Input
                type="search"
                placeholder="Search articles..."
                value={searchQuery}
                onChange={(e) => {
                  setSearchQuery(e.target.value);
                  setActiveArticleId(null);
                }}
                className="pl-10"
                data-testid="help-search"
              />
            </div>
          </div>
        </div>
      </section>

      {/* ── Main Content ── */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
        <div className="flex gap-8">
          {/* ── Sidebar (desktop) ── */}
          <aside
            className="hidden w-64 shrink-0 lg:block"
            data-testid="help-sidebar"
          >
            <div className="sticky top-24">{sidebarContent}</div>
          </aside>

          {/* ── Mobile navigation dropdown ── */}
          <div className="mb-4 w-full lg:hidden">
            <Button
              variant="outline"
              className="flex w-full items-center justify-between"
              onClick={() => setMobileNavOpen(!mobileNavOpen)}
              data-testid="help-mobile-nav-toggle"
            >
              <span className="text-sm font-medium">
                {activeArticle
                  ? activeArticle.title
                  : "Browse articles"}
              </span>
              <ChevronDown
                className={`h-4 w-4 transition-transform ${
                  mobileNavOpen ? "rotate-180" : ""
                }`}
              />
            </Button>
            {mobileNavOpen && (
              <div className="mt-2 rounded-lg border border-stone-200 bg-white p-3 shadow-lg">
                {sidebarContent}
              </div>
            )}
          </div>

          {/* ── Content Area ── */}
          <main className="min-w-0 flex-1" data-testid="help-content">
            {activeArticle ? (
              /* ── Single article view ── */
              <article data-testid="help-article-detail">
                <button
                  onClick={handleBackToList}
                  className="mb-4 flex items-center gap-1.5 text-sm font-medium text-primary transition-colors hover:text-primary/90"
                  data-testid="help-back-btn"
                >
                  <ArrowLeft className="h-3.5 w-3.5" />
                  Back to articles
                </button>
                <div className="mb-2">
                  <span className="inline-block rounded-full bg-primary/5 px-2.5 py-0.5 text-xs font-medium text-primary">
                    {SECTIONS.find((s) => s.key === activeArticle.section)
                      ?.label ?? activeArticle.section}
                  </span>
                </div>
                <h2 className="text-2xl font-bold text-stone-900 sm:text-3xl">
                  {activeArticle.title}
                </h2>
                <div className="mt-6 space-y-4 text-sm sm:text-base">
                  {renderContent(activeArticle.content)}
                </div>
              </article>
            ) : (
              /* ── Article list view ── */
              <div data-testid="help-article-list">
                {visibleSections.map((section) => {
                  const Icon = section.icon;
                  const sectionArticles =
                    groupedArticles[section.key] || [];
                  return (
                    <div key={section.key} className="mb-10">
                      <div className="mb-4 flex items-center gap-2">
                        <Icon className="h-5 w-5 text-primary" />
                        <h2 className="text-lg font-semibold text-stone-900">
                          {section.label}
                        </h2>
                        <span className="rounded-full bg-stone-100 px-2 py-0.5 text-xs font-medium text-stone-500">
                          {sectionArticles.length}
                        </span>
                      </div>
                      <div className="grid gap-3 sm:grid-cols-2">
                        {sectionArticles.map((article) => (
                          <button
                            key={article.id}
                            onClick={() => handleArticleClick(article.id)}
                            className="group flex items-start gap-3 rounded-xl border border-stone-100 bg-white p-4 text-left transition-all hover:border-primary/20 hover:shadow-sm"
                            data-testid={`help-card-${article.id}`}
                          >
                            <div className="flex-1">
                              <h3 className="text-sm font-medium text-stone-900 group-hover:text-primary">
                                {article.title}
                              </h3>
                              <p className="mt-1 line-clamp-2 text-xs text-stone-500">
                                {article.content.slice(0, 120)}...
                              </p>
                            </div>
                            <ExternalLink className="mt-0.5 h-3.5 w-3.5 shrink-0 text-stone-300 group-hover:text-primary" />
                          </button>
                        ))}
                      </div>
                    </div>
                  );
                })}

                {visibleSections.length === 0 && (
                  <div className="py-16 text-center" data-testid="help-empty-state">
                    <HelpCircle className="mx-auto h-12 w-12 text-stone-300" />
                    <h3 className="mt-4 text-lg font-medium text-stone-700">
                      No articles found
                    </h3>
                    <p className="mt-2 text-sm text-stone-500">
                      Try a different search term or browse a section from the
                      sidebar.
                    </p>
                  </div>
                )}
              </div>
            )}

            {/* ── Contact support banner ── */}
            <div className="mt-12 rounded-xl border border-stone-200 bg-stone-50 p-6 text-center" data-testid="help-contact-banner">
              <h3 className="text-base font-semibold text-stone-900">
                Still need help?
              </h3>
              <p className="mt-1 text-sm text-stone-600">
                Our support team is available Sunday through Thursday, 9 AM - 6
                PM GST.
              </p>
              <div className="mt-4 flex flex-col items-center gap-3 sm:flex-row sm:justify-center">
                <a
                  href="mailto:support@vetara.ae"
                  className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-primary/90"
                  data-testid="help-contact-email"
                >
                  Email Support
                </a>
                <a
                  href="https://wa.me/"
                  className="inline-flex items-center gap-1.5 rounded-lg border border-stone-200 bg-white px-4 py-2 text-sm font-medium text-stone-700 transition-colors hover:bg-stone-50"
                  data-testid="help-contact-whatsapp"
                >
                  WhatsApp Support
                </a>
              </div>
            </div>
          </main>
        </div>
      </div>
    </div>
  );
}
