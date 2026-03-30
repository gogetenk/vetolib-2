# CI Pipeline Reliability Audit

**Date:** 2026-03-30
**Scope:** `.github/workflows/ci.yml`, `.github/workflows/nightly.yml`, `.github/workflows/deploy.yml`, `.github/workflows/release.yml`
**Status:** Audit only -- no changes made

---

## Executive Summary

Three reliability issues confirmed:

1. **Nightly Playwright cancelled every night** -- root cause identified (timeout + webServer startup + single worker + 37 spec files)
2. **Concurrency group cancelling CI runs on rapid pushes to develop** -- by design, but too aggressive for a multi-agent workflow
3. **PR triggers appear to be working** -- recent data shows PR-triggered runs completing; if PRs have had no checks, the issue is likely branch protection config, not the workflow trigger

---

## Issue 1: Nightly Playwright Cancelled Every Night

### Evidence

Every nightly run from 2026-03-22 to 2026-03-30 has `conclusion: cancelled`. Each run starts at ~03:05 UTC and is killed at exactly ~03:35 UTC (30 minutes later).

```
2026-03-30  started: 03:05:02  cancelled: 03:35:41  (30m 39s)
2026-03-29  started: 03:04:34  cancelled: 03:34:54  (30m 20s)
2026-03-28  started: 03:01:38  cancelled: 03:32:01  (30m 23s)
...same pattern every night...
```

### Root Cause

The job has `timeout-minutes: 30`. The run uses up the entire timeout and gets killed. The logs show:

1. **`webServer` command is `npm run dev:webpack -- --port 6100`** which runs `next dev` (development mode). Next.js dev server compiles pages on-demand, which is slow on a CI runner.
2. **Playwright `webServer.timeout` is 120s** (2 minutes) for the dev server to be ready. This alone consumes significant time.
3. **37 spec files running with `workers: 1`** (single-threaded in CI). With retries set to 2, a failing test can consume 3x its normal time.
4. **`video: "retain-on-failure"` and `trace: "on-first-retry"`** generate large artifacts per failure, slowing things further.
5. **Hydration mismatch errors in ConsentBanner** visible in logs -- the dev server is throwing React errors which may cause test failures and trigger retries.

The 30-minute timeout is insufficient for 37 spec files running sequentially on a dev server with retries.

### Recommendations

| Priority | Action | Impact |
|----------|--------|--------|
| **P0** | Increase `timeout-minutes` to 60 | Stops the bleeding immediately |
| **P1** | Use `npm run build && npm run start` instead of `npm run dev` for the webServer | Production server is 5-10x faster than dev mode |
| **P1** | Increase `workers` from 1 to 2 or 3 | Cuts test time proportionally |
| **P2** | Fix the ConsentBanner hydration mismatch | Reduces noise and potential test failures/retries |
| **P2** | Consider sharding Playwright across multiple jobs | GitHub-native parallelism |

**Recommended webServer change for nightly:**
```typescript
// Instead of: command: "npm run dev:webpack -- --port 6100"
// Use:
webServer: {
  command: "npm run build && npm run start -- -p 6100",
  url: "http://localhost:6100",
  reuseExistingServer: true,
  timeout: 120000,
}
```
Or better: have the nightly workflow build first, then run Playwright against the production build. The CI workflow already uploads a `nextjs-build` artifact.

---

## Issue 2: Concurrency Group Cancelling CI Runs

### Current Config

```yaml
concurrency:
  group: ci-${{ github.ref }}
  cancel-in-progress: true
```

### Problem

The concurrency group is `ci-${{ github.ref }}`. For pushes to `develop`, `github.ref` is always `refs/heads/develop`. This means every push to develop cancels the previous in-progress CI run.

**Evidence from the last 30 runs:**
```
CI | push | develop | 19:00:55 | cancelled
CI | push | develop | 18:56:25 | cancelled
CI | push | develop | 18:52:13 | cancelled
CI | push | develop | 18:43:31 | cancelled
CI | push | develop | 18:27:20 | cancelled
```

Five consecutive develop pushes within ~35 minutes, each cancelling the previous. In a multi-agent workflow where agents merge PRs in quick succession, the develop branch CI almost never completes.

### For PRs: This Is Fine

For PR branches, `github.ref` is `refs/pull/{number}/merge`, so each PR gets its own concurrency group. A force-push to a PR branch cancels the old run and starts fresh -- this is correct behavior.

### For Develop: This Is Harmful

When multiple PRs merge to develop in quick succession, the CI for develop is repeatedly cancelled. The last push "wins" but by that time the intermediate commits were never validated.

### Recommendations

| Priority | Action | Impact |
|----------|--------|--------|
| **P0** | Split the concurrency into separate groups for push vs PR | Develop pushes stop cancelling each other |
| **P1** | Alternative: disable `cancel-in-progress` for the develop/main push event | Every merge gets validated |

**Recommended fix:**
```yaml
concurrency:
  group: ci-${{ github.event_name == 'pull_request' && github.head_ref || github.sha }}
  cancel-in-progress: ${{ github.event_name == 'pull_request' }}
```

This way:
- PR pushes: grouped by branch name, cancel-in-progress = true (desired)
- Push to develop/main: grouped by commit SHA (unique), cancel-in-progress = false (every commit validated)

---

## Issue 3: PR Trigger Not Firing

### Current Config

```yaml
on:
  push:
    branches: [develop, main]
  pull_request:
    branches: [develop, main]
```

### Analysis

The `pull_request` trigger defaults to `types: [opened, synchronize, reopened]` when no types are specified. This is correct.

There are **no path filters** (`paths:` or `paths-ignore:`), so every PR touching any file triggers CI. This is also correct.

**Recent PR-triggered runs show CI is triggering:**
```
CI | completed | success | chore/cleanup-dead-email-templates | 2026-03-30T19:32:58Z
CI | completed | failure | fix/typescript-types-consolidation  | 2026-03-30T19:11:38Z
CI | completed | failure | feat/api-endpoint-summaries         | 2026-03-30T18:56:09Z
```

### Possible Explanations for "No Checks" on Some PRs

1. **Branch protection misconfiguration**: If the required status check is configured as `Status check — all jobs green` but the workflow name or job name has changed, GitHub won't match the check.
2. **Workflow file not present on the PR branch**: If a PR branch was created before `ci.yml` existed (or from a fork), the workflow from that branch is used, not from `develop`.
3. **GitHub Actions quota**: Free tier allows 2000 minutes/month for private repos. If exhausted, runs are queued but not started.
4. **Concurrency cancellation**: If a PR push happens while another run for the same PR is in progress, the old run is cancelled. If the new run also gets cancelled (by another push), neither completes -- showing "no checks" on the PR.

### Recommendations

| Priority | Action | Impact |
|----------|--------|--------|
| **P1** | Verify branch protection rule references the exact job name `Status check — all jobs green` | Ensures GitHub reports the check |
| **P1** | Add `workflow_dispatch` to `ci.yml` for manual re-triggering | Escape hatch when CI doesn't trigger |
| **P2** | Check GitHub Actions usage/quota in repo Settings > Billing | Rule out quota exhaustion |

---

## Issue 4: Nightly Notify-Failure Job Skipped

### Current Config

```yaml
notify-failure:
  needs: playwright-e2e
  if: failure()
```

### Problem

When the Playwright job is **cancelled** (timeout), its conclusion is `cancelled`, not `failure`. The `failure()` function only matches `conclusion == failure`. As a result, the notification job is **skipped every night** and no GitHub issue is created.

### Evidence

```
Notify on failure | completed | skipped | started: 2026-03-30T03:35:40Z
```

### Recommendation

Change the condition to:
```yaml
if: ${{ failure() || cancelled() }}
```

Or more precisely:
```yaml
if: ${{ needs.playwright-e2e.result != 'success' }}
```

---

## Issue 5: Missing `workflow_dispatch` on CI

The `ci.yml` workflow has **no `workflow_dispatch` trigger**. This means there is no way to manually re-run CI without pushing a new commit. The `nightly.yml` and `deploy.yml` both already have `workflow_dispatch`.

### Recommendation

Add to `ci.yml`:
```yaml
on:
  push:
    branches: [develop, main]
  pull_request:
    branches: [develop, main]
  workflow_dispatch:  # Manual trigger for debugging
```

---

## Issue 6: Redundant Build Steps (Minor)

Both `backend-bdd` and `backend-integration` jobs depend on `backend-build` but each does its own `dotnet restore` and `dotnet build`. The build artifacts from `backend-build` are not shared via artifacts or cache. Each downstream job rebuilds from scratch.

### Recommendation (Low Priority)

Consider uploading the build output as an artifact from `backend-build` and downloading it in downstream jobs. This would save ~2-3 minutes per job. However, .NET build caching with NuGet restore is already in place, so the incremental benefit may be small.

---

## Issue 7: Node.js 20 Deprecation Warning

GitHub Actions logs show:
> Node.js 20 actions are deprecated. Actions will be forced to run with Node.js 24 starting June 2nd, 2026.

This affects `actions/checkout@v4`, `actions/setup-node@v4`, and `actions/upload-artifact@v4`. While not breaking today, these will stop working on 2026-09-16.

### Recommendation

Plan to upgrade to `@v5` versions of these actions when available, or set `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24=true` to test compatibility early.

---

## Summary of Recommendations

| # | Priority | Issue | Fix |
|---|----------|-------|-----|
| 1 | **P0** | Nightly always cancelled at 30min | Increase timeout to 60min; switch webServer to production build |
| 2 | **P0** | Develop CI cancelled on rapid merges | Use commit-SHA-based concurrency group for push events |
| 3 | **P1** | Nightly failure notification never fires | Change `if: failure()` to `if: needs.playwright-e2e.result != 'success'` |
| 4 | **P1** | No manual CI trigger | Add `workflow_dispatch` to `ci.yml` |
| 5 | **P1** | Verify branch protection status check name | Confirm it matches `Status check — all jobs green` exactly |
| 6 | **P1** | Playwright too slow for 30min | Increase workers from 1 to 2-3; use production server |
| 7 | **P2** | ConsentBanner hydration mismatch | Fix the React error to reduce test flakiness |
| 8 | **P2** | Node.js 20 deprecation | Plan upgrade to actions v5 before Sept 2026 |
| 9 | **P3** | Redundant builds in BDD/integration jobs | Share build artifacts from backend-build job |
