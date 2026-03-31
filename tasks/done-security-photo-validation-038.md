# Task: Add content-type validation to patient photo upload

**Module:** MedicalRecords
**Priority:** HIGH
**Source:** docs/audits/qa-night-security-20260401.md — 6.2

## Problem
POST /api/v1/patients/{id}/photo validates file size but not content type. An attacker could upload a file with spoofed Content-Type. If served back with stored content-type, XSS possible.

## Fix
1. In PatientEndpoints.cs photo upload, validate content-type against allowlist: image/jpeg, image/png, image/webp
2. Add magic-byte validation (check file header bytes match declared type)
3. Look at how Messaging's UploadFilesHandler uses FileTypeValidator.DetectContentType — replicate that pattern

## Definition of Done
- [ ] `dotnet build` 0 errors
- [ ] `dotnet test` GREEN
- [ ] Content-type validated against allowlist before storage
- [ ] Magic-byte validation on uploaded file
- [ ] Returns 400 for invalid file types
