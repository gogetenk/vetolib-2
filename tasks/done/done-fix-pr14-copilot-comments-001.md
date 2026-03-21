# todo-fix-pr14-copilot-comments-001.md — Fix Copilot review comments on PR #14

**Module**: Infra
**Type**: fix
**Priority**: high (unblocks PR merge)
**Branch**: feat/infra-sentry-impl-001

## Context
PR #14 has 2 Copilot review comments. Both were reportedly fixed in commit afcd3969 (DSN validation).
Verify the fixes are correct and push any remaining changes.

Additionally, PR #15 (worktree-agent-af7a900d) only adds 2 auth test files on top of PR #14.
Cherry-pick those commits onto feat/infra-sentry-impl-001 and close PR #15.

## Copilot Comments (PR #14)
1. `appsettings.Production.json:22` — Sentry DSN `__OVERRIDE_VIA_ENV__` could init with invalid DSN → reportedly fixed with Uri.IsWellFormedUriString validation
2. `Program.cs:68` — Same DSN validation concern → reportedly fixed

## Cherry-pick from PR #15
- Branch: worktree-agent-af7a900d
- Files: `tests/Vetolib.Tests.Unit/Auth/DismissChecklistValidatorTests.cs` and `DismissWelcomeBannerValidatorTests.cs`
- Cherry-pick the test commit(s) onto feat/infra-sentry-impl-001

## Additional Copilot Comments (PR #15, pre-existing code in same branch)
These are in the diff because the branch is large. Fix the easy ones:
1. `.claude/hooks/verify-before-push.sh` — add `set -o pipefail` (pipeline exit code comes from `tail`, not `dotnet`)
2. `docker-compose.prod.yml:14` — rename env var to `Cors__AllowedOrigins` to match appsettings key
3. `src/frontend/src/lib/api/client.ts:198` — `apiPostFormData` should handle 204 No Content (return undefined instead of parsing JSON)
4. `src/backend/Vetolib.Api/appsettings.Development.json:5` — remove concrete Sentry DSN, leave empty

Do NOT fix:
- ChangePassword.feature HTTP status codes (style preference, tests pass)
- Patients.feature capitalization (tests pass, bindings work)
- SuggestSlotHandler N+1 (pre-existing, separate refacto task)
- Sidebar.tsx locale link (pre-existing, separate task)

## Acceptance criteria
- [ ] DSN validation fix verified in Program.cs
- [ ] Auth test files cherry-picked onto feat/infra-sentry-impl-001
- [ ] PR #15 closed with comment
- [ ] pipefail added to verify-before-push.sh
- [ ] docker-compose CORS env var fixed
- [ ] apiPostFormData 204 handling added
- [ ] Dev Sentry DSN removed from appsettings.Development.json
- [ ] All tests GREEN
- [ ] Push to feat/infra-sentry-impl-001
