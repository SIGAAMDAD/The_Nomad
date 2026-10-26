# The Nomad Studio V5 — Deployment and Operations

**Status: STAGING. This repository is not an autonomous worker host.**
**Production branches:** game `SIGAAMDAD/The_Nomad:Nomad3D`; framework
`SIGAAMDAD/NomadFramework:develop`. Never silently use either repository's
historical `main` branch for gameplay/framework implementation.

## 1. What has actually been implemented

| Component | Code | Automation boundary |
| --- | --- | --- |
| Signed task contract | `studio/control.py` | HMAC covers ownership, prerequisites, branch and pinned baseline |
| Scheduler | `studio/control.py` | Priority P0/P1/P2, dependency DAG, worker capacity, path locks |
| Dispatch + recovery | `studio/orchestrator.py` | GitHub Issue outbox + Slack delivery; no blind retries |
| Worker acknowledgments | `studio/orchestrator.py` | Verified GitHub account comments update labels |
| Apps Script intake | `studio/apps-script/StudioV5Bridge.gs` | Needs installation, secrets, owner PIN, and enabled controller |
| GitHub Actions | `.github/workflows/studio-dispatch.yml` | Explicit release switches and secret requirements |
| Dependency automation | `.github/dependabot.yml` | Scheduled PR proposals, not automatic merge |
| Framework develop watcher | `studio/dependency_watch.py` | Review-only issue when pinned gitlink differs |
| CI failure triage | `studio/ci_triage.py` | Review-only regression issues; independent authorization required |
| Unit tests | `studio/tests/` | Executed on GitHub PR; not proof of Godot native build |

V5 is **not** a persistent ChatGPT runtime. No Work event trigger or paid API
execution is automatically installed by a YAML file.

## 2. Required GitHub deployment choice

GitHub resolves `issues`, `schedule`, `repository_dispatch`,
`workflow_run`, and Dependabot's `.github/dependabot.yml` from the
**repository default branch**, currently `main`. The V5 PR targets
`Nomad3D` by owner request. That means the new scheduled/event workflows
and Dependabot policy do **not** become active merely by merging the V5 PR.

Before activation, select ONE approved option:

1. Change `The_Nomad` default branch to `Nomad3D` in repository Settings,
   after confirming every existing GitHub workflow and tool that assumes
   `main` (including legacy PR #2) has been reconciled. OR
2. Maintain a minimal, reviewed dispatcher and `.github/dependabot.yml` on
   default `main` that explicitly fetches **V5 source from Nomad3D**, and
   targets all dependency/version-update PRs to `Nomad3D`. This requires
   a **separate authorized PR to main**. No such main change is made here.

The same default-branch rule applies independently to
`NomadFramework/develop` Dependabot configuration. Do not infer activation
from a green PR build.

## 3. Secrets, variables, and scopes

On `SIGAAMDAD/The_Nomad`, go to **Settings > Secrets and variables > Actions**.

| Name | Kind | Initial value | Use |
| --- | --- | --- | --- |
| `STUDIO_ENABLED` | Repository variable | `false` | Hard kill switch for dispatch |
| `STUDIO_DEPENDENCY_ISSUES` | Repository variable | `false` | Permit review issue for framework drift |
| `STUDIO_TRIAGE_ENABLED` | Repository variable | `false` | Permit review issues from failed CI |
| `STUDIO_APPROVAL_KEY` | Actions secret | 32+ random characters | Verify owner's signed task contract |
| `STUDIO_SLACK_WEBHOOK` | Actions secret | Slack incoming webhook URL | Deliver worker assignments |

Never put a credential in a repo file, GitHub Issue, workflow log, or Google
Sheet. A fine-grained token with Issues read/write for `The_Nomad` belongs
in Google Apps Script **Project Settings > Script Properties** as
`STUDIO_GITHUB_TOKEN`, not in the spreadsheet.

Install `STUDIO_APPROVAL_KEY` as an Apps Script property **with the same exact
value** as the GitHub Actions secret. Install `NOMAD_OWNER_PIN_SHA256` using
the existing owner approval controller. Set `STUDIO_V5_ENABLED=false`
initially. The existing Sheet `CONFIG.studio_mode` must also be `ACTIVE`
before Apps Script creates any issue. Each gate independently fails closed.

Security warning: the GitHub workflow has Issues write permission to
deliver receipts. Anyone able to change the default-branch workflow or
repository secrets is a trusted administrator. Protect those changes.

## 4. Google Apps Script installation

1. Open the existing [Studio Master Ledger](https://docs.google.com/spreadsheets/d/1avYu8wwu8KZMPcb36_9Pz0-6cJgkZCOWber4NNxx8NA/edit).
2. In Extensions > Apps Script, open the **bound studio controller**.
3. Add `StudioV5Bridge.gs` from this repository as a **new file**. Do not
   delete or replace the historical `Code.gs`, and do not enable the old
   V4 intake trigger in parallel with V5.
4. Review the code and create the script properties in section 3.
5. Run `studioV5InstallIntake()` from the editor. Google will request an
   authorization grant. This creates `STUDIO_INTAKE_V5` without overwriting
   existing `TASKS`, `WAVES`, or V4 intake.
6. Create one DRAFT row in the V5 intake tab, with an exact source SHA and
   approved Google Drive brief. For source-writing roles provide owned paths
   such as `Source/Game/Items/**`; use `-` for no dependencies or
   protected paths. Dependencies are comma-separated task IDs.
7. Approve that exact row by running
   `studioV5Approve("ITEM-0042", "<your PIN>")` in an authorized
   Apps Script invocation. The signature binds all relevant task fields.
8. To activate the five-minute bridge, first verify the existing controller
   and set the two independent switches, then run
   `studioV5InstallTrigger("<your PIN>")`.
9. If a record is edited after approval, **discard approval and resubmit**
   it as a new DRAFT. Do not copy the signature to another record.

Because the connected Drive controller's write permissions have not been
verified in this PR, these steps have not been executed remotely.

## 5. Dispatch and handoff semantics

Task Issue body must contain one line per canonical field:

```text
STUDIO_TASK_ID: ITEM-0042
TARGET: game
TARGET_REPO: SIGAAMDAD/The_Nomad
TARGET_BRANCH: Nomad3D
BASE_SHA: <40 hexadecimal SHA>
DRIVE_BRIEF_URL: https://docs.google.com/document/d/<id>/edit
OWNER_APPROVED: YES
DEPARTMENT: gameplay
DEPENDS_ON: -
OWNED_PATHS: Source/Game/Items/**
PROTECTED_PATHS: Source/NomadFramework/**
APPROVAL_SIG: <64 hexadecimal HMAC-SHA256>
```

Approval signature: lowercase HMAC-SHA256 of the UTF-8 JSON serialization of
the **eleven signed fields** (all above except `APPROVAL_SIG`) with keys
sorted alphabetically and no spaces around separators. Never sign unapproved
or mutable content.

Issue labels: `studio:approved`, `ai:ready`, exactly one
`dept:<department>`, and optionally `priority:p0`/`p1`/`p2`.

A dependency is satisfied **only** if a signed dependency Issue has both
`studio:accepted` and `state=closed`. Submitting a document or creating a
PR does not automatically mean accepted.

When a worker is eligible, the dispatcher reserves the assignment on GitHub,
writes a durable outbox receipt, sends the configured Slack notification, and
marks it dispatched. Workers must de-duplicate by
`(STUDIO_TASK_ID, lease_id)`.

A worker may acknowledge by posting a GitHub comment in the exact form:

```text
<!-- STUDIO_EVENT {"task_id":"ITEM-0042","lease_id":"<from Slack>","type":"CLAIMED"} -->
```

Supported types: `CLAIMED`, `RUNNING`, `CHECKPOINT`, `SUBMITTED`,
`BLOCKED`. Only Issue comments authored by configured
`worker_logins` are accepted. Progress should be accompanied by a linked
commit, artifact or PR. Source-writing workers never modify protected files.
QA passes cannot authorize source merge or lore/canon promotion.

If the notification may have been sent but no acknowledgment is recorded,
V5 places the issue into `ai:needs-attention`. **It does not automatically
start a second programmer**, because delivery is at-least-once and prior
execution might still be running.

## 6. Dependabot and dependency safety

The game config checks **GitHub Actions** and **NuGet** versions on scheduled
cadences with `target-branch: Nomad3D`. The framework config checks
**NuGet**, **GitHub Actions**, and its documentation git submodule with
`target-branch: develop`. These are *version update proposals*.

GitHub Dependabot **security update** settings are controlled separately and
typically target the **default branch**. Set up Dependabot Alerts and
Security Updates in GitHub repository settings; do not assume
`target-branch` redirects those security PRs to `Nomad3D`/`develop`.
Security alerts and version-update rules may need separate default-branch
strategy and policy.

`Source/NomadFramework` in `The_Nomad/Nomad3D` is pinned to a gitlink.
Ordinary `gitsubmodule` Dependabot updates may target the submodule's
*default branch*, not the desired framework `develop`; V5 intentionally
uses a custom review-only watcher instead.

Dependabot PR checks run without untrusted secrets. Do not run untrusted
Dependabot/fork PR source on a self-hosted runner. Dependabot-generated PR
workflows have read-only tokens and restricted secrets by design.

## 7. Windows and GPU validation

No self-hosted runner is registered by these files. To enable trusted
Windows/GPU CI you must register a runner from repository Settings > Actions >
Runners. Label it `nomad-windows-build`; install the owner-pinned MSVC,
Windows SDK, SCons, Python, .NET and custom Godot toolchains. Configure the
tested GDExtension build recipe and representative GPU scenes before enabling
hardware jobs. Keep external PRs off trusted runners.

The V5 PR tests exercise controller logic, **not** native Godot, C++,
C#, Vulkan/D3D12 or memory performance. Until a runner is verified, no
engine build may be marked `VERIFIED`.

## 8. Pilot, rollback and future changes

1. Merge the PR only when Python tests pass and file scope is reviewed.
2. Keep all variables `false` and config `mode=staging`.
3. Resolve the default-branch strategy with a separate owner-controlled
   configuration change. Verify GitHub actually recognizes the active
   Dependabot file on that branch.
4. Install the Apps Script V5 companion with a staging intake row.
5. Configure a dedicated private Slack channel and event-triggered Work
   task. Confirm that bot-originated Slack messages truly trigger the task.
6. Test a non-production Issue and confirm one signed dispatch, one
   acknowledgment, one checkpoint, one submission, and QA status.
7. Explicitly approve the transition of `studio/config.json` to
   `mode=active`, then set `STUDIO_ENABLED=true`.
8. Set `STUDIO_DEPENDENCY_ISSUES=true` to allow drift tickets after a
   read-only watch check; enable CI triage only after deduplication review.

**Emergency stop:** set `STUDIO_ENABLED=false` and
`STUDIO_V5_ENABLED=false` and lock `CONFIG.studio_mode`. This stops
**new dispatches**, not AI sessions already running. Disable the Work event
trigger separately. Do not delete existing GitHub issues or PRs to stop work.

All parameters—branches, role capacities, lease timeout, priority, and role
definitions—are versioned in `studio/config.json`. Change them through a PR
with corresponding contract tests. Never silently modify owner-approved
architectural baselines.
