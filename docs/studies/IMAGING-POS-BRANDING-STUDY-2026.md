# Imaging, POS Integration & Branding Study — March 2026

> Research study covering three critical topics for Vetolib's product roadmap:
> veterinary medical imaging integration, payment terminal (TPE/POS) integration,
> and the "Vetolib" branding risk.

---

## Table of Contents

1. [PART 1 — Veterinary Medical Imaging](#part-1--veterinary-medical-imaging)
2. [PART 2 — Payment Terminal / POS Integration](#part-2--payment-terminal--pos-integration)
3. [PART 3 — "Vetolib" Branding Risk](#part-3--vetolib-branding-risk)
4. [Recommendations & Next Steps](#recommendations--next-steps)
5. [Sources](#sources)

---

## PART 1 — Veterinary Medical Imaging

### 1.1 Do Veterinary Systems Use DICOM?

**Yes.** Veterinary radiology systems use DICOM (Digital Imaging and Communications in Medicine), the same standard as human medicine. The American College of Veterinary Radiology (ACVR) formally adopted DICOM as the digital imaging standard for veterinary medicine.

However, there are veterinary-specific extensions:

- **DICOM Working Group 25 (WG-25)**, sponsored by the ACVR, was created specifically to incorporate veterinary terminology and identification tags into DICOM.
- In 2006, the DICOM Standards Committee published **Correction Proposal 643**, which formally established how veterinary patient data should be encoded. This added support for:
  - **Species/Taxon** (dog, cat, reptile, equine, etc.)
  - **Breed**
  - **Owner information** (linked to the animal, not the patient directly)
  - **Neuter status**
  - **Microchip number**
  - **Animal strain** (added via CP-1478)
- Standard human DICOM identifies individual human patients; veterinary DICOM requires species, breed, owner, and other animal-specific metadata.

**Key takeaway:** Vetolib must support DICOM 3.0 with WG-25 veterinary extensions if it wants to integrate with imaging hardware.

### 1.2 Is There a Veterinary-Specific Standard?

There is no separate standard. Veterinary imaging uses **DICOM 3.0 with the WG-25 veterinary supplements**. This is the universally accepted approach. All major veterinary PACS and imaging vendors comply with this standard.

Compliance levels vary in practice. A 2018 German study (PMC5788831) found that DICOM conformance in veterinary referral cases was inconsistent, particularly for metadata completeness. This means Vetolib should be tolerant of incomplete DICOM metadata when ingesting images.

### 1.3 How Do Competitors Integrate Imaging?

#### Cornerstone (IDEXX)

- Tight integration with **IDEXX Web PACS** (their own cloud PACS).
- Supports **DICOM Modality Worklist** — sends patient info directly to imaging devices, eliminating manual data entry at the modality.
- Images are linked to patient records via URL; clicking opens the DICOM viewer in Web PACS.
- AI-powered viewer with automatic hanging protocols and vertebral heart score calculation.
- Also supports non-IDEXX imaging systems via generic Modality Worklist integration.

#### ezyVet

- Integrates with multiple imaging partners:
  - **IDEXX Web PACS** — links in patient records open DICOM images in Web PACS viewer.
  - **Vet Rocket / RocketPACS** — view images directly within ezyVet, built-in radiology reports.
  - **Intelerad** — enterprise-grade PACS for larger practices.
- Patient information is sent from ezyVet to local imaging devices (X-ray, ultrasound, dental, CT, MR) via DICOM Modality Worklist.
- Automated billing when images are ordered through the software.

#### Digitail

- Integrates with IDEXX Web PACS and Sound SmartPACS.
- Focus on cloud-based workflow.

#### Common Pattern Across Competitors

1. **Practice management software does NOT embed a full DICOM viewer.** Instead, it links to external PACS/viewer.
2. **DICOM Modality Worklist** is used to push patient data to imaging hardware (reduces errors).
3. **Images are stored in a dedicated PACS** (cloud or on-premise), not in the practice management database.
4. **Links/URLs in the patient record** open images in the PACS viewer.
5. **Automated billing** triggers when imaging is ordered/completed.

### 1.4 Should Vetolib Build a DICOM Viewer or Just Store/Link?

**Recommendation: Link-based approach first, optional lightweight viewer later.**

#### Option A — Link to External PACS (Recommended for MVP)

- Store a reference (URL/study UID) in the patient's medical record.
- When clicked, open the image in the clinic's existing PACS viewer (IDEXX Web PACS, VisioPACS, etc.).
- Implement DICOM Modality Worklist to push patient data to imaging devices.
- **Pros:** Fast to implement, no viewer maintenance, clinics keep their existing PACS.
- **Cons:** Dependent on third-party viewer availability.

#### Option B — Embedded Lightweight Viewer (Phase 2)

- Use **OHIF Viewer** (open source, MIT license) + **Cornerstone.js** (rendering library).
- OHIF is a zero-footprint, web-based DICOM viewer that runs entirely in the browser.
- Cornerstone.js supports DICOMweb, GPU-accelerated rendering, multi-threaded decoding.
- OHIF v3.11 supports multimodality fusion, RT structures, ultrasound mode, DICOM Labelmap.
- **Pros:** Complete in-app experience, no external dependencies, potential differentiator.
- **Cons:** Significant development effort, must handle all DICOM transfer syntaxes, maintenance burden.

#### Option C — Full PACS (Not Recommended)

- Build or host a complete PACS server (Orthanc, dcm4chee).
- **Not recommended** for a practice management SaaS — this is a separate product category.

### 1.5 Major Veterinary Imaging Hardware Manufacturers

| Manufacturer | Products | Integration Protocol |
|---|---|---|
| **IDEXX** | ImageVue DR50 Plus, Web PACS | DICOM 3.0, proprietary Web PACS API, Modality Worklist |
| **Fujifilm** | VXR Veterinary X-Ray Room, CR systems | DICOM 3.0, Synapse Enterprise Imaging |
| **Agfa** | CR (Computed Radiography), DR systems, MUSICA image processing | DICOM 3.0, standard CR/DR protocols |
| **Fovea** | Software-only (Fovea DR) | DICOM 3.0, HL7-friendly, bi-directional PIMS integration |
| **OR Technology** | dicomPACS DX-R acquisition software | DICOM 3.0, controls various X-ray generators |
| **ImageWorks Veterinary** | ViewAll 3.2, dental X-ray systems | DICOM 3.0 (ViewAll meets Vet DICOM 3.0), Send/Receive |
| **Sound (Antech)** | SmartPACS | DICOM 3.0, automated billing integration |
| **Asteris** | Keystone PACS | DICOM 3.0, cloud PACS |
| **VisioPACS** | Cloud PACS | DICOM 3.0 compliant |
| **MedDream** | Web DICOM Viewer | DICOMweb, browser-based, HTML5 |

### 1.6 Available APIs and Protocols

| Protocol | Description | Used By |
|---|---|---|
| **DICOM 3.0** | Core standard for image storage, transfer, print | All vendors |
| **DICOM Modality Worklist (MWL)** | Push patient/study info to imaging devices | Cornerstone, ezyVet, most PIMS |
| **DICOMweb (WADO-RS, STOW-RS, QIDO-RS)** | RESTful HTTP APIs for DICOM operations | OHIF, MedDream, modern cloud PACS |
| **HL7 v2 / HL7 FHIR** | Health data exchange (orders, results) | Fovea, some hospital integrations |
| **Proprietary REST APIs** | Vendor-specific integration endpoints | IDEXX Web PACS, Vet Rocket, Intelerad |

### 1.7 Recommended Integration Roadmap for Vetolib

**Phase 1 (MVP):**
- Add `ImagingStudy` entity to MedicalRecords module (study UID, modality, date, URL, thumbnail URL).
- Implement linking: store PACS URL in patient record, open externally.
- Support manual image upload (JPEG/PNG) for clinics without PACS.

**Phase 2:**
- Implement DICOM Modality Worklist server (push patient data to imaging devices).
- Integrate with IDEXX Web PACS API (largest market share).
- Add DICOMweb (WADO-RS) for retrieving images from compatible PACS.

**Phase 3:**
- Embed OHIF Viewer (Cornerstone.js) for in-app viewing.
- Add AI-assisted reading (Vetology AI, SignalPET) integration.
- DICOM storage (STOW-RS) for clinics that want Vetolib as their PACS.

---

## PART 2 — Payment Terminal / POS Integration

### 2.1 Do Vet Clinics Want Connected Payment Terminals?

**Yes, strongly.** Integrated payment processing is now a standard expectation:

- **IDEXX** migrated all Neo customers to IDEXX Payments (powered by Fiserv) with Clover POS devices as of December 2025.
- **Shepherd Vet** offers Shepherd Pay with USB terminal integration directly from their PIMS.
- **Vetspire** has a full bi-directional integration with Square.
- **Lightspeed** supports credit/debit cards, mobile wallets (Apple Pay, Google Pay), contactless, and split payments for vet retail.
- **Clover** markets specifically to veterinary clinics as a "digital transformation" tool.

The value proposition: **invoice created in PIMS --> payment collected on terminal --> invoice status auto-updated to "Paid"**. No manual reconciliation, no double-entry.

### 2.2 Technical Architecture: How It Works

The general flow for integrated POS in a SaaS veterinary application:

```
[Vetolib Frontend]          [Vetolib Backend]          [Payment Provider]          [Physical Terminal]
       |                          |                          |                          |
  1. Cashier clicks              |                          |                          |
     "Collect Payment"           |                          |                          |
       |----> 2. POST /invoices/{id}/pay                    |                          |
       |              amount, currency                      |                          |
       |                          |----> 3. Create Terminal |                          |
       |                          |      Checkout/Payment   |                          |
       |                          |      (amount, device)   |                          |
       |                          |                          |----> 4. Display amount  |
       |                          |                          |      on terminal screen |
       |                          |                          |                          |
       |                          |                          |   5. Client taps/inserts |
       |                          |                          |      card               |
       |                          |                          |                          |
       |                          |<---- 6. Webhook:        |                          |
       |                          |      payment.succeeded  |                          |
       |                          |                          |                          |
       |<---- 7. Update invoice  |                          |                          |
       |      status = "Paid"    |                          |                          |
       |      + receipt URL      |                          |                          |
```

### 2.3 Provider Comparison

#### Stripe Terminal

| Aspect | Detail |
|---|---|
| **What it is** | SDK to integrate Stripe payments into in-person checkout flows |
| **NOT a POS** | It is a payment layer, not a full POS system — must be integrated into your app |
| **SDKs** | JavaScript, iOS, Android, React Native |
| **Hardware** | BBPOS WisePad 3, Stripe Reader S700, Stripe Reader M2, Tap to Pay (phone) |
| **Fees (US)** | 2.7% + $0.05 per in-person transaction |
| **Fees (EU)** | 1.5% + 0.05 EUR (EEA cards), 2.5% + 0.05 EUR (non-EEA) |
| **Monthly fees** | None |
| **UAE availability** | Stripe is available in UAE (Dubai Internet City office), but **Terminal hardware availability in UAE is unclear** — must verify directly with Stripe |
| **Integration effort** | Medium — requires SDK integration, webhook handling, device pairing |
| **Pros** | Excellent API/docs, unified online+in-person payments, Connect for marketplaces |
| **Cons** | Requires dev work, no inventory/POS features built-in |

#### SumUp

| Aspect | Detail |
|---|---|
| **What it is** | Card reader + Cloud API for in-person payments |
| **Integration options** | Cloud API (HTTPS from any platform), Reader SDKs (native), Payment Switch (app handoff) |
| **Hardware** | SumUp Air, SumUp Solo, SumUp Solo Lite, Tap to Pay |
| **Fees (UK)** | 1.69% flat (standard), 0.99% with SumUp One at 19 GBP/month |
| **Fees (US)** | 2.60% + $0.10 per transaction |
| **Monthly fees** | None (standard), 19 GBP/month (SumUp One) |
| **UAE availability** | SumUp operates in 36+ countries — **UAE availability must be verified** |
| **SDKs** | PHP, Node.js, Python, Java, Go, Rust, .NET |
| **Sandbox** | Full sandbox environment for testing |
| **Pros** | Very simple Cloud API, .NET SDK available, low EU fees, sandbox |
| **Cons** | Fewer enterprise features than Stripe, limited marketplace support |

#### Square Terminal

| Aspect | Detail |
|---|---|
| **What it is** | All-in-one payment device with Terminal API |
| **Integration** | Terminal API (create checkout, receive webhook), Devices API for pairing |
| **Hardware** | Square Terminal (built-in screen + printer), Square Reader |
| **Fees (US)** | 2.6% + $0.10 per tap/dip/swipe |
| **UAE availability** | **NOT available in UAE.** Square only processes payments in US, Canada, Australia, UK, Japan, Ireland, France |
| **Pros** | Simple API, good for retail, built-in receipt printing |
| **Cons** | Not available in UAE (deal-breaker for MVP market) |

#### UAE-Specific Options

| Provider | Notes |
|---|---|
| **Adyen** | Available in UAE, offers POS terminals, advanced certified solutions |
| **Fiserv/Clover** | Powers IDEXX Payments, Clover devices available in some UAE banks |
| **Local bank terminals** | ADCB and other UAE banks provide POS machines (Visa, MasterCard, Amex) |
| **Tabby** | BNPL provider used by some UAE vet clinics (e.g., Umm Suqeim Vet Centre) |

### 2.4 How to Integrate in Vetolib

#### Billing Module Extension

```
Vetolib.Billing/
  Features/
    Payments/
      CollectPayment/
        CollectPaymentCommand.cs        -- { InvoiceId, TerminalId, Amount, Currency }
        CollectPaymentHandler.cs         -- calls payment provider API
        CollectPaymentEndpoint.cs        -- POST /api/invoices/{id}/payments/terminal
      WebhookReceived/
        PaymentWebhookHandler.cs         -- handles provider webhook
        PaymentWebhookEndpoint.cs        -- POST /api/webhooks/stripe (or sumup)
  Infrastructure/
    PaymentProviders/
      IPaymentTerminalProvider.cs        -- interface (in Contracts)
      StripeTerminalProvider.cs          -- Stripe implementation
      SumUpTerminalProvider.cs           -- SumUp implementation
```

#### Key Design Decisions

1. **Abstract the provider** behind `IPaymentTerminalProvider` in `Vetolib.Billing.Contracts` — allows switching providers per market (Stripe in EU/US, Adyen/SumUp in UAE).
2. **Webhook-driven status updates** — never poll. Register webhook endpoint, verify signature, update invoice status.
3. **No hardware dependency in Vetolib** — the terminal is the provider's hardware. Vetolib just sends "collect X amount on terminal Y".
4. **Multi-currency** — UAE uses AED, France uses EUR, Poland uses PLN. The provider handles currency.

### 2.5 Cost Summary

| Provider | In-Person Fee | Online Fee | Monthly | Hardware Cost |
|---|---|---|---|---|
| Stripe Terminal | 1.5% + 0.05 EUR (EU) / 2.7% + $0.05 (US) | 1.5% + 0.25 EUR (EU) | 0 | ~49-249 EUR per reader |
| SumUp | 1.69% (UK/EU) / 2.60% + $0.10 (US) | 2.5% | 0 (or 19 GBP/mo for lower rate) | ~39-139 EUR per reader |
| Square | 2.6% + $0.10 (US) | 2.9% + $0.30 | 0 | ~49-399 USD per terminal |
| Adyen | Custom (volume-based) | Custom | Custom | Varies |

### 2.6 Recommended Strategy for Vetolib

**Phase 1 (UAE MVP):**
- Implement `IPaymentTerminalProvider` abstraction.
- Integrate **Stripe Terminal** if available in UAE, otherwise **Adyen** (confirmed UAE presence).
- Manual payment recording as fallback (cashier marks invoice as paid + payment method).

**Phase 2 (France/EU expansion):**
- Add **Stripe Terminal** integration (confirmed availability in France).
- Add **SumUp** as alternative (lower fees, .NET SDK available, popular in EU small businesses).

**Phase 3:**
- Multi-provider per clinic (clinic chooses their provider in settings).
- Payment analytics dashboard.
- Split payments support.

---

## PART 3 — "Vetolib" Branding Risk

### 3.1 Is the "Vetolib" Trademark Registered in France?

**Yes. The trademark "Vetolib" is registered at INPI and owned by La Compagnie des Animaux (CDA), the parent group of SanteVet.**

Key facts:

- **French verbal trademark No. 17/4 327 687** — "VETOLIB" — registered in classes 35, 38, 42, and 44.
  - Class 35: Advertising, business management
  - Class 38: Telecommunications
  - Class 42: Scientific/technological services, software
  - Class 44: Medical/veterinary services
- Originally filed by companies "Vetolib" and "Boetie Vet".
- **Transferred to La Compagnie des Animaux** via trademark assignment contract dated **July 24, 2020**.
- A second trademark **No. 20/4 662 990** — "VETOLIB" — was filed on July 2, 2020, designating class 36 (insurance and banking services).

### 3.2 Who Owns It and What Do They Do?

**La Compagnie des Animaux (CDA)** is the parent group behind:
- **SanteVet** — France's leading pet health insurance
- **Bulle Bleue** — pet insurance brand
- **Jim & Joe** — pet insurance brand
- **vetolib.vet** — online veterinary appointment booking platform

SanteVet acquired vetolib.vet and relaunched it as a free online appointment booking tool for French veterinarians. The platform offers:
- Online appointment booking (similar to Doctolib for animals)
- Shared schedules by practitioner/service/site
- Instant messaging between vet and client
- Document/photo/report sharing for hospitalized animals
- SMS reminders (0.09 EUR HT per SMS, only paid feature)

SanteVet invested **50 million EUR** in digital transformation, with vetolib.vet as a strategic pillar. Their stated ambition is to create "the Doctolib for animals."

### 3.3 Risk of Confusion

**Risk level: HIGH for France. Low for UAE/international.**

| Factor | Assessment |
|---|---|
| **Identical name** | Our project uses "Vetolib" — exactly the same name as the registered trademark |
| **Same market** | Both are veterinary software platforms (appointment booking, practice management) |
| **Same classes** | Classes 42 (software) and 44 (veterinary services) directly overlap |
| **Same territory** | France is our second target market |
| **Backed by deep pockets** | SanteVet/CDA has 50M EUR investment budget — they will litigate |
| **UAE risk** | Lower — trademark is French, but CDA could file international extensions |

**Legal exposure:** Using "Vetolib" commercially in France for veterinary software would constitute **trademark infringement** under French intellectual property law (Code de la propriete intellectuelle, L.713-2). CDA could seek:
- Injunction to stop use of the name
- Damages for confusion
- Domain seizure

### 3.4 Rebranding Alternatives

If rebranding is necessary (strongly recommended before France launch), here are naming directions:

| Direction | Examples | Notes |
|---|---|---|
| **Vet + Tech suffix** | VetFlow, VetPulse, VetSync, VetNova | Modern, clear category signal |
| **Arabic/UAE flavor** | Hayawan (animal in Arabic), PawSahel | Differentiating for UAE market |
| **Abstract/Invented** | Cliniqa, Pawtera, Vetara, Animatica | Trademarkable, no conflicts |
| **Descriptive** | VetDesk, ClinicHub, PawClinic | Clear but harder to trademark |
| **Premium/Professional** | Apex Vet, Praxis Vet, VetEdge | Professional positioning |

**Recommended approach:**
1. Choose 5-10 candidate names.
2. Check INPI (France), USPTO (US), EUIPO (EU), UAE trademark databases.
3. Check domain availability (.com, .vet, .app).
4. Rebrand BEFORE launching in France.
5. UAE MVP can proceed under "Vetolib" temporarily if no UAE trademark exists, but plan for the rename.

---

## Recommendations & Next Steps

### Imaging — Priority: Medium (Phase 2-3)

1. **Short term:** Add `ImagingStudy` reference entity to MedicalRecords. Support manual image upload (JPEG/PNG).
2. **Medium term:** Implement DICOM Modality Worklist + IDEXX Web PACS integration.
3. **Long term:** Embed OHIF Viewer (Cornerstone.js) for in-app DICOM viewing.
4. **Do NOT build a PACS** — this is a separate product category.

### POS/Payment — Priority: High (Phase 1-2)

1. **Immediately:** Design `IPaymentTerminalProvider` abstraction in Billing.Contracts.
2. **For UAE MVP:** Verify Stripe Terminal UAE availability; if unavailable, integrate Adyen. Always have manual payment recording as fallback.
3. **For France:** Stripe Terminal (confirmed available) + SumUp as alternative.
4. **Architecture:** Webhook-driven, multi-currency, provider-per-clinic configuration.

### Branding — Priority: CRITICAL (Before France launch)

1. **STOP using "Vetolib" commercially in France.** The trademark is owned by La Compagnie des Animaux/SanteVet.
2. **Begin rebranding process immediately** — select new name, verify trademark availability, register.
3. **UAE MVP can proceed** under current name temporarily, but plan the transition.
4. **Budget:** ~500-2000 EUR for INPI/EUIPO trademark registration + domain costs.

---

## Sources

### Imaging
- [Fovea DR Software Solutions](https://foveadr.com/fovea-products-solutions/fovea-software-solutions/)
- [IDEXX ImageVue DR50 Plus](https://www.idexx.com/en/veterinary/diagnostic-imaging-telemedicine-consultants/imagevue-dr50/)
- [IDEXX Web PACS](https://www.idexx.com/en/veterinary/diagnostic-imaging-telemedicine-consultants/web-pacs/)
- [OR Technology dicomPACS DX-R](https://www.or-technology.com/en/products/vet/dicompacs-dx-r-acquisition-and-diagnostic-software.html)
- [ImageWorks ViewAll 3.2](https://radiologyimagingsolutions.com/product/viewall-3-2-veterinary-software/)
- [DICOM Introduction — dvm360](https://www.dvm360.com/view/introduction-dicom)
- [VisioPACS](https://visiopacs.com/en)
- [MedDream Veterinary DICOM Viewer](https://meddream.com/solutions/veterinary-imaging/)
- [Asteris Keystone PACS](https://www.asteris.com/manage-your-images/)
- [DICOM WG-25 Veterinary Medicine](https://www.dicomstandard.org/activity/wgs/wg-25)
- [DICOM in Veterinary Medicine — PostDICOM](https://www.postdicom.com/en/blog/dicom-in-veterinary-medicine)
- [DICOM Conformance in Vet Medicine (PMC)](https://pmc.ncbi.nlm.nih.gov/articles/PMC5788831/)
- [ezyVet Digital Imaging Integrations](https://www.ezyvet.com/partners/all-countries/digital-imaging)
- [ezyVet IDEXX Web PACS Integration](https://docs.ezyvet.com/en/see-all-integrations/diagnostic-imaging/idexx-web-pacs)
- [Cornerstone Imaging Overview](https://cornerstonehelphub.com/docs/imaging-overview/)
- [Vet PACS Integration — Medicai](https://blog.medicai.io/en/vet-pacs-integration/)
- [Fujifilm VXR Veterinary X-Ray Room](https://healthcaresolutions-us.fujifilm.com/resources/press-release/fujifilm-launches-vxr-veterinary-x-ray-room-a-digital-radiography-system-dedicated-to-veterinary-imaging/)
- [Agfa Veterinary Solutions](https://agfaradiologysolutions.com/veterinary-home-page/)
- [OHIF Viewer (GitHub)](https://github.com/OHIF/Viewers)
- [Cornerstone.js](https://www.cornerstonejs.org/)
- [OHIF Documentation](https://docs.ohif.org/)
- [Free DICOM Viewers for Vet Medicine (PubMed)](https://pubmed.ncbi.nlm.nih.gov/30859340/)

### POS / Payment
- [IDEXX Neo Payment Processing](https://merchantcostconsulting.com/lower-credit-card-processing-fees/idexx-neo-integrated-payment-processing-explained/)
- [Best Vet Software for Payment Processing 2026](https://merchantcostconsulting.com/lower-credit-card-processing-fees/vet-software-payment-processing/)
- [Shepherd Pay](https://www.shepherd.vet/clinical-tools/payment-processor/)
- [Lightspeed Vet POS](https://www.lightspeedhq.com/pos/retail/vet-store-pos/)
- [Clover POS for Vets](https://uk.clover.com/insights/the-digital-transformation-animal-clinics-need-pos-system-for-vets/)
- [Stripe Terminal Overview](https://docs.stripe.com/terminal/overview)
- [Stripe Terminal Pricing](https://support.stripe.com/questions/stripe-terminal-pricing)
- [Stripe Pricing](https://stripe.com/pricing)
- [Stripe Terminal Country Availability](https://support.stripe.com/questions/stripe-terminal-country-and-currency-availability)
- [Stripe UAE Launch](https://stripe.com/newsroom/news/stripe-launches-uae)
- [Stripe UAE FAQ](https://support.stripe.com/questions/uae-faq)
- [SumUp Developer Portal](https://developer.sumup.com/)
- [SumUp Cloud API](https://developer.sumup.com/terminal-payments/cloud-api)
- [SumUp In-Person Payments](https://developer.sumup.com/terminal-payments)
- [SumUp Pricing UK](https://www.expertmarket.com/uk/merchant-accounts/sumup-fees-costs)
- [SumUp Pricing US](https://www.sumup.com/en-us/credit-card-processing-fees/)
- [Square Terminal API](https://developer.squareup.com/docs/terminal-api/overview)
- [Square UAE Availability (Community)](https://community.squareup.com/t5/Using-Square/Can-I-use-Square-in-the-United-Arab-Emirates/m-p/54580)
- [Vetspire Square Integration](https://support.vetspire.com/support/solutions/articles/70000098231-setting-up-your-square-integration)
- [Adyen UAE POS](https://www.adyen.com/en_AE/pos-payments)
- [ADCB POS Machines UAE](https://www.adcb.com/en/business/products-solutions/payment-solutions/pos-machines)
- [Dynamix Vet Software Dubai](https://www.dynamixuae.com/it-company-in-dubai-uae/pos-in-dubai-uae/veterinary-clinic-management-software-in-dubai-uae/)

### Branding
- [SanteVet rachete vetolib.vet — Le Point Veterinaire](https://www.lepointveterinaire.fr/actualites/actualites-professionnelles/santevet-rachete-vetolib-vet-service-de-rdv-en-ligne.html)
- [PayVet et Vetolib lances — Le Point Veterinaire](https://www.lepointveterinaire.fr/actualites/actualites-professionnelles/les-services-payvet-et-vetolib-officiellement-lances.html)
- [SanteVet investit 50M EUR — Lyon Entreprises](https://www.lyon-entreprises.com/actualites/article/santevet-investit-50-millions-deuros-pour-developper-le-doctolib-pour-animaux)
- [SanteVet rachete vetolib.vet — Depeche Veterinaire](https://www.depecheveterinaire.com/santevet-rachete-vetolib-vet_679E52843C79BA.html)
- [PayVet et Vetolib — Woopets](https://www.woopets.fr/chien/actualite/payvet-et-vetolib-2-nouveaux-services-de-paiement-et-de-rendez-vous-veterinaires-lances-par-la-compagnie-des-animaux/)
- [Vetolib startup profile — J'aime les Startups](https://www.jaimelesstartups.fr/vetolib-doctolib-des-veterinaires-animaux/)
- [Cour d'appel Paris — Doctrine.fr](https://www.doctrine.fr/d/CA/Paris/2024/CAP8FB5F312B88A4A6CCDF0)
- [SanteVet digital transformation — JDE](https://www.lejournaldesentreprises.com/article/santevet-nous-consacrons-50-millions-deuros-notre-transformation-digitale-sur-quatre-ans-2051632)
