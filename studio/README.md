> **Studio V5:** This README documents the historical V4 deployment. For the active signed orchestration, QA handoffs, Dependabot, rollback and Windows runner procedures, see [Studio V5 Deployment](V5_DEPLOYMENT.md). V5 remains staging-only until explicitly enabled.

# The Nomad Studio V4 — Event-Driven Control Plane

**Status: STAGING. No unattended dispatch or AI execution enabled.**
\n**Deployment target: `The_Nomad/Nomad3D` (from the current `Nomad3D` source tree).**

## Sources of truth

- Game: `SIGAAMDAD/The_Nomad` branch **Nomad3D**, not legacy `main`.
- Framework: `SIGAAMDAD/NomadFramework` branch **develop**.
- Master Ledger: https://docs.google.com/spreadsheets/d/1avYu8wwu8KZMPcb36_9Pz0-6cJgkZCOWber4NNxx8NA/edit
- Existing Google Apps Script controller remains the authority for wave approval, task leases and final acceptance.
- GitHub Issues dispatch only owner-approved implementation tasks; GitHub PRs carry source changes.

## Design

```
Drive Studio Ledger -> Owner-approved brief -> GitHub issue
  -> [studio:approved + ai:ready + dept:<name>]
  -> GitHub Actions studio-dispatch
  -> Slack notification -> authorized Work/event worker
  -> isolated branch -> draft PR -> repository's own CI -> QA -> owner merge
```

The dispatcher is *one-way and staged*. An issue's contract must be valid before dispatch. It does not invoke a paid model API, trigger ChatGPT scheduled conversations, merge code, or override existing CI. Its work queue uses GitHub Issues, not repeated pushes to game branches.

### Issue contract (required lines in Issue body)

```text
STUDIO_TASK_ID: ITEM-0042
TARGET: game
TARGET_REPO: SIGAAMDAD/The_Nomad
TARGET_BRANCH: Nomad3D
BASE_SHA: <exact 40-hex commit SHA of the target branch>
DRIVE_BRIEF_URL: https://docs.google.com/document/d/<verified-ID>/edit
OWNER_APPROVED: YES
```

Issue title must start with `[ITEM-0042]`. Labels: `dept:gameplay`, `studio:approved`, `ai:ready`. The issue must match the already-approved ledger assignment and source baseline. This first dispatcher validates the declared contract; it does **not** independently verify a Drive ledger approval. Until that cross-check exists, only the authorized owner/controller should apply `studio:approved`; protect issue-label permissions accordingly.

### Configuration

Edit `studio/config.json` through a reviewed PR. `targets` lists repository and approved development branch, `departments` maps worker roles, `max_per_run` caps notifications, and `mode` gates dispatch. Both `config.mode=active` and repository variable `STUDIO_ENABLED=true` are required.

This PR is based on and **targets `SIGAAMDAD/The_Nomad:Nomad3D`**, not `main`. Merging it installs the staged controller and validation workflows in the active game source branch. It does not modify `NomadFramework/develop` or enable AI messages. GitHub issue, scheduled and repository-dispatch workflows are resolved from the repository's **default branch** (currently `main`), independent of this PR's target. The previous controller PR #2 was already merged into `main`; that existing default-branch workflow continues running its own staged/disabled copy. Changes made only to the `Nomad3D` dispatcher will not affect issue dispatch until the default-branch workflow is separately synchronized or the repository default branch is changed. A PR-targeted test run can validate this copy without activating dispatch.

### Activation (owner actions)

1. Review this PR and its Python contract tests. Merge the staging controller into **`SIGAAMDAD/The_Nomad:Nomad3D`**. Do not merge or retarget this PR to `main`.
2. Set up a Slack incoming webhook for a dedicated `#studio-dispatch` channel and store its full URL as GitHub Actions repository secret `STUDIO_SLACK_WEBHOOK`. Never commit this value.
3. Add the ChatGPT app to that Slack channel, connect it in ChatGPT Work, and manually create an authorized event-triggered worker task. Test receipt of **bot-originated Slack messages**; support is not guaranteed.
4. Configure repository/branch rules and the issue-label approval authority. Freeze source baselines in the ledger.
5. Before activation, decide which branch owns the live dispatcher: synchronize the default-branch workflow with this copy, or explicitly designate `Nomad3D` as default. Review a PR changing that branch's `studio/config.json` from `staging` to `active`. Set GitHub Actions repository variable `STUDIO_ENABLED=true` **only after** testing.
6. Create a non-production approved test issue in the repository. Verify webhook, label transition, and one worker report. Never treat dispatch as successful implementation.

To stop dispatch instantly, set `STUDIO_ENABLED=false`. This switch overrides config even if an issue event arrives. Existing scheduled ChatGPT workers are separate and unchanged.

### Windows runner (owner actions)

1. GitHub repository **Settings > Actions > Runners > New self-hosted runner**. Choose Windows x64, download and register using GitHub's current commands (registration token is temporary).
2. Install your pinned Godot fork, MSVC/VS Build Tools, Windows SDK, SCons, Python, .NET SDK and `godot-cpp`. Keep their paths under stable environment variables.
3. Assign a dedicated runner label like `nomad-windows-build`; do **not** expose this runner to untrusted fork PRs.
4. Add a separate trusted build workflow based on the actual verified engine project/build recipe; the dispatcher purposely includes no guessed build commands.
5. Add a GPU-specific label for Vulkan/D3D12 validation, and run representative benchmark scenes with stored baseline metrics.

### Existing Apps Script controller

The current Drive bundle `Code.gs` has its own five-minute scheduler and approval gates. Install/authorize it from the Apps Script editor only after code review. A downloadable companion bridge is included in the handoff; it creates GitHub issues on explicit approved intake and uses Script Properties for authentication.

**Discovered issue in existing source:** the original `Code.gs` currently has at least one invalid reference to `pass` in `nmSubmit_`, so do not deploy it unchanged. Fix and test it first. The V4 bridge is separate and does not alter it.

### Recovery and idempotence

The dispatcher scans open approved ready issues every 15 minutes to recover lost GitHub issue events. It sets `ai:dispatched`, comments with a receipt, and removes `ai:ready` after successful webhook delivery. If a worker does not respond, it is **not** automatically requeued by this first version: a human/controller checks the issue, commits and Slack receipt before resetting its status. Slack is an at-least-once delivery mechanism; worker prompt must de-duplicate by task ID and issue URL.

### Existing CI and merge controls

Framework `develop` already contains working CI workflow definitions; this package does not overwrite them. PRs must target the correct branch and retain owner approval requirements. No automated merges or paid agent invocation are included.

## Tests

```bash
python3 -m unittest discover -s studio/tests -p "test_*.py" -v
python3 -m py_compile studio/dispatcher.py
```
