# todo-front-attachments-001.md — Staff attachment UI in ReplyComposer

**Module** : Messaging (frontend)
**Dependencies** : done-back-attachments-002
**Priority** : high
**MSW** : oui

## Objective
Add file upload UI to the staff reply composer.

## Scope
1. Paperclip button in ReplyComposer opens file picker (PDF, JPG, PNG)
2. Preview thumbnails of selected files before sending
3. Upload progress indicator
4. Remove file from selection
5. Max 5 files, max 10MB each — client-side validation with user-friendly error
6. "Download all" button on conversation attachments
7. MSW handler for POST /api/v1/messaging/upload

## Completion criteria
- [ ] File picker working
- [ ] Preview + remove
- [ ] Upload progress
- [ ] Download all button
- [ ] MSW handler
- [ ] data-testid on all interactive elements
- [ ] Build GREEN
