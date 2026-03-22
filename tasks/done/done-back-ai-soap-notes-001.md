# todo-back-ai-soap-notes-001.md — AI SOAP Notes / Scribe

**Module** : AI + MedicalRecords
**Dependencies** : none
**Priority** : CRITICAL (competitive gap #1, all competitors have it)

## Context
AI SOAP Notes is the #1 requested feature in veterinary software (1,680% YoY search growth).
Every major competitor (Vetspire, Digitail, Shepherd) ships it. Not having it risks
disqualification during vendor selection.

## Objective
Auto-generate SOAP notes (Subjective, Objective, Assessment, Plan) from consultation data.

## Scope
1. `ISoapNotesGenerator` interface in AI.Contracts
2. LLM-based implementation (Claude API) that takes consultation inputs and generates SOAP
3. Inputs: symptoms, vitals, diagnosis, treatment plan, prescriptions
4. Output: structured SOAP note ready for medical record
5. Endpoint: POST /api/v1/ai/soap-notes
6. Integration with MedicalRecords: auto-fill SOAP when creating a medical record
7. Edit/approve flow: vet reviews AI-generated note before saving

## Completion criteria
- [ ] SOAP notes generation working via LLM
- [ ] Integration with medical record creation
- [ ] Vet can edit before saving
- [ ] Unit tests
- [ ] Build GREEN
