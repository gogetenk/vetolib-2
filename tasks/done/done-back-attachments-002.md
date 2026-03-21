# todo-back-attachments-002.md — Staff attachment upload + model changes

**Module** : Messaging
**Dependencies** : done-back-attachments-001
**Priority** : high

## Objective
Enable vets to attach files (PDF, JPG, PNG) to their replies.

## Scope
1. Add `PendingUpload` entity + migration
2. New endpoint: POST /api/v1/messaging/upload (multipart/form-data)
   - Validate: max 5 files, max 10MB each, PDF/JPG/PNG only
   - Magic bytes validation (not just Content-Type header)
   - Store via IFileStorage, return attachment IDs
3. Modify `StaffReplyRequest` in Contracts to accept `List<Guid> AttachmentIds`
4. Modify `SendReplyHandler` to attach PendingUploads to the message
5. Replace `StoragePath` with presigned URL in `MessageAttachmentDto`
6. Background cleanup service for orphaned PendingUploads (>24h)

## PO decisions
- Owners CAN see vet attachments (radios, lab results)
- "Download all" button: YES
- Retention: 1 year minimum
- Attach to medical record: YES (future task)

## Completion criteria
- [ ] Upload endpoint functional with validation
- [ ] Staff replies can include attachments
- [ ] Presigned URLs in DTO (not raw storage paths)
- [ ] Cleanup background service
- [ ] Unit tests
- [ ] Build GREEN
