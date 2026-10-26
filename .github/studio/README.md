# The Nomad automated studio - staging hooks
Status: PREPARED / NOT DEPLOYED
Owner-approved game source: SIGAAMDAD/The_Nomad branch `Nomad3D`
Pinned head at intake (2026-10-26): `2e4be6aa626a83a198b54122ded4ce6a7382327c`.
Legacy branch: `main`; preserve history, do not dispatch development to it.

This branch contains **inert, manual-only GitHub Actions scaffolding**. There are no outbound webhooks, API tokens, AI task launches, automatic edits, or branch promotions. The hook is activated only after the owner approves its PR, and its `workflow_dispatch` still requires a manual launch. GitHub default branch remains main until changed in repository settings.

## Proposed job exchange
1. Studio Director selects a READY approved job in Google Drive.
2. The specialist chat receives its exact brief, source ref, owned paths and acceptance criteria.
3. Worker returns a PR or artifact URL with status IMPLEMENTED or PARTIAL.
4. Manual CI handoff validates provenance and reports actual checks. It never pretends to compile without a configured toolchain.
5. Independent QA verifies output; Director updates the shared status report.

## Planned hooks (not active)
- `pull_request` event: eventually enqueue review/QA metadata after explicit approval.
- `workflow_run` or CI result: eventually return build/test status to the Director.
- `workflow_dispatch`: manually inspect a submitted job.
- `repository_dispatch`: reserved for future authenticated external controller.

No automatic coding implementation via Codex or other paid agents is authorized.
Do not introduce automated Github writes or merge policies until the owner explicitly requests activation.

## Role registry
The stored `AAA_Studio_Skill_Library_v2.zip` manifest currently contains 174 specialist roles plus one Studio Director = 175 functions. Roles are assignable entries, not 175 simultaneously active ChatGPT Scheduled Tasks. The existing 10-active-task cap means the scheduler multiplexes execution slots and uses Drive to share job state.

## GOAP source
Recommendation: split a reusable pure C++17 GOAP planning/runtime core into a dedicated versioned repository only after freezing its standalone interfaces and tests. Keep Godot/GDExtension adapter and C#/Godot registration code inside the game extension repository, with an exact dependency pin.
