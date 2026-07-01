# Introduction

This document outlines the process to triage GitHub issues and pull requests for MonoGame and provides an explanation of the labels used to assist with triage.

## Triage Process

When an issue or pull request is opened, maintainers perform an initial triage pass to make sure it is routed to the right area of the project and has enough information for future investigation.

Triage is not intended to fully diagnose or resolve a report.  Its purpose is to categorize incoming work, identify the affected areas of MonoGame, and determine whether additional information is needed.

### Issues

During issue triage, maintainers may:

- Apply labels to identify the type of issue and the affected areas of MonoGame.
- Determine whether the issue appears to be platform-specific, backend-specific, or broadly applicable.
- Request additional information, logs, sample projects, or reproduction steps when necessary.
- Mark issues that require further investigation, design discussion, or reproduction before work can proceed.
- Add contextual notes that may help future contributors and maintainers understand the report.

Not every issue is fully investigated during triage.  Some reports can be categorized quickly, while others may require discussion or additional information before their scope is understood.

### Pull Requests

During pull request triage, maintainers may:

- Apply labels describing the type and scope of the change.
- Identify whether the change introduces a breaking change, regression fix, or breaking change.
- Verify that the pull request contains enough information for review.
- Request clarification when the intent or impact of a change is unclear.
- Mark the pull request as ready for review once it reaches the appropriate state.

Triage does not replace code review.  Its purpose is to ensure a pull request is categorized correctly and has the context needed for reviewers to evaluate it.

### Good First Issues

Some issue maybe identified as suitable for new contributors and receive the `good first issue` label.

These issues are typically:

- Narrow in scope.
- Low risk.
- Self-contained.
- Well understood.
- Accompanied by enough context to help contributors get started.

Issue may also receive `help wanted` or `up-for-grabs` labels when maintainers are actively seeking community contributions.

### What Contributors Should Expect

After triage, an issue or pull request should have enough labeling and context to indicate:

- What type of work it represents.
- Which areas of MonoGame are affected.
- Whether additional information is needed.
- Whether it is waiting on design discussion, review, or further investigation.

Triage helps maintainers prioritize work, make  issues easier to discover, and provides contributors with a clearer understanding of where a report fits within the project.

## Labels

The MonoGame repository uses a consistent label set to keep issue and pull request triage simple and scalable across MonoGame's larger cross-platform surface area.

### Labeling rules (quick)

- Every issue and pull request should have **exactly one `Kind`** label.
- Add **0-2 `scope:`** labels (keep it focused).
- Add **`platform:`** only when the issue is OS, device, or platform specific.
- Add **`priority:`** only when you're intentionally ordering work.
- Use **`status:`** labels only when state is not obvious from the conversation or GitHub state.
- Use **impact/helper labels** like `breaking-change`, `regression`, or `behavior-change` only when they materially help triage.

---

### Kind (pick exactly one)

| Label         | Description                                                                                                                                                                                                                                                          |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `bug`         | Something is broken, crashes, regresses, or behaves unexpectedly. The behavior is observable and differs from documented or intended behavior.                                                                                                                       |
| `feature`     | A new user-facing capability that does not exist yet. Adds something new to the public surface: a new API, backend capability, tool behavior, content capability, or subsystem integration.                                                                          |
| `enhancement` | An improvement to an existing feature. Covers usability improvements, expanded support, better defaults, performance wins, compatibility work, and incremental platform/backend improvements.                                                                        |
| `refactor`    | Internal restructuring with no intended behavior change. If it might change user-observable behavior, prefer `bug` or `enhancement` instead.                                                                                                                         |
| `docs`        | Documentation-only work: README updates, guides, migration notes, XML doc comments, website/docs content, or contributor documentation. If code is also changed, prefer the relevant `Kind` label and add a `scope:` label instead of using `docs` as a second kind. |
| `question`    | Support, clarification, or discussion threads. These often close without code changes once answered.                                                                                                                                                                 |
| `security`    | A security concern or vulnerability. If sensitive, avoid posting details publicly; use private reporting where possible.                                                                                                                                             |

---

### Compatibility / Impact

| Label             | Description                                                                                                                                                                                                                                                                           |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `breaking-change` | The change requires user code changes or breaks source, binary, behavioral, packaging, or workflow compatibility. This includes public API removals or signature changes, tooling changes that require user action, or behavior changes that are expected to break existing projects. |
| `regression`      | Something previously working no longer works, or behavior has worsened between versions, commits, preview builds, or platform/backend updates. Usually paired with `bug`.                                                                                                             |
| `behavior-change` | The change intentionally alters runtime, tooling, pipeline, or platform behavior in a way users may notice even if the public API does not change.                                                                                                                                    |

---

### Scope (0-2 labels)

> Scopes describe **where** the work lands. Keep this focused; two labels max.

| Label                     | Description                                                                                                  |
| ------------------------- | ------------------------------------------------------------------------------------------------------------ |
| `scope: api`              | Use for changes to the public API surface and developer facing contracts.                                    |
| `scope: graphics`         | Use for core rendering and graphics-device behavior that is not specific to a single backend.                |
| `scope: directx`          | Use for DirectX specific work.                                                                               |
| `scope: opengl`           | Use for OpenGL, DesktopGL, or GLES specific work.                                                            |
| `scope: vulkan`           | Use for Vulkan- or DesktopVK-specific work.                                                                  |
| `scope: metal`            | Use for Metal-specific work.                                                                                 |
| `scope: shaders`          | Use for shader compilation, translation, compatibility, and runtime shader behavior.                         |
| `scope: native`           | Use for native runtime, interop, bindings, or unmanaged code related work.                                   |
| `scope: content-pipeline` | Use for content import, processing, and build pipeline behavior.                                             |
| `scope: content-builder`  | Use for the content builder tool itself and its user-facing behavior.                                        |
| `scope: mgcb`             | Use for the MGCB command line tool and task integration behavior.                                            |
| `scope: audio`            | Use for audio playback, audio content, codecs, or audio backend behavior.                                    |
| `scope: input`            | Use for keyboard, mouse, touch, gesture, and controller input behavior.                                      |
| `scope: gamepad`          | Use for controller specific behavior and compatibility.                                                      |
| `scope: touch`            | Use for touch and gesture specific behavior.                                                                 |
| `scope: windowing`        | Use for window creation, focus, resizing, fullscreen behavior, display handling, and OS window integration.  |
| `scope: templates`        | Use for project templates and template packaging or distribution.                                            |
| `scope: tooling`          | Use for developer tooling, scripts, diagnostics, and local development workflows outside the runtime itself. |
| `scope: build`            | Use for CI, packaging, dependency management, build scripts, and release automation.                         |
| `scope: tests`            | Use for unit tests, integration tests, regression tests, and test infrastructure.                            |
| `scope: performance`      | Use for profiling, allocations, throughput, memory pressure, and performance related improvements.           |
| `scope: docs`             | Use for documentation structure, guides, contributor docs, and API documentation work.                       |
| `scope: math`             | Use for numerical correctness, transforms, geometry, interpolation, and math related helpers.                |
| `scope: spritefont`       | Use for SpriteFont related authoring, runtime behavior, and text rendering concerns.                         |

---

### Priority (optional)

Only set priority when you're intentionally ordering work.

| Label              | Description                                                                             |
| ------------------ | --------------------------------------------------------------------------------------- |
| `priority: high`   | Important; should be addressed in the next cycle, milestone, or active planning window. |
| `priority: medium` | Default. Apply only if you want the priority to be explicit.                            |
| `priority: low`    | Nice to have; backlog item with no near-term urgency.                                   |

---

### Status (optional)

Use status labels to reflect workflow state *beyond* open/closed.

| Label                  | Description                                                                                                                                            |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `status: in-progress`  | Someone is actively working on it. Ideally link a branch, pull request, or comment.                                                                    |
| `status: blocked`      | Cannot proceed until something external changes or a decision is made. Add a short note explaining the blocker.                                        |
| `status: needs-info`   | Waiting on the reporter or a maintainer to provide logs, a repro, a sample project, platform details, or other clarifying information.                 |
| `status: needs-review` | Pull request is ready for review (prefer on pull requests, not issues).                                                                                |
| `status: needs-repro`  | The report may be valid, but a reliable reproduction is still needed before work can proceed confidently.                                              |
| `status: needs-design` | A feature or larger change needs maintainer agreement on approach, architecture, API shape, or project direction before implementation should proceed. |

---

### Platform (optional)

Use when the issue is target-platform-specific, OS-specific, device-specific, or reproducible only on specific platforms.

| Label                | Description                                                 |
| -------------------- | ----------------------------------------------------------- |
| `platform: windows`  | Windows                                                     |
| `platform: linux`    | Linux                                                       |
| `platform: macos`    | macOS                                                       |
| `platform: android`  | Android                                                     |
| `platform: ios`      | iOS                                                         |
| `platform: consoles` | Console targets or console oriented platform considerations |

> Prefer backend `scope:` labels such as `scope: directx`, `scope: opengl`, or `scope: vulkan` for graphics API/backend-specific issues. Use `platform:` for the OS/device/target environment itself.

---

### Community / Triage helpers

| Label              | Description                                                                                                                                                                                                                  |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `good first issue` | Beginner-friendly, well-scoped, and ready for an outside contributor. Should have clear reproduction steps or precise acceptance criteria, pointers to the relevant files/subsystems, and a low risk of architectural churn. |
| `help wanted`      | Maintainers are open to outside contribution. Ideally add pointers on where to start and what an acceptable solution looks like.                                                                                             |
| `up-for-grabs`     | A clearly available piece of work that contributors can self-select. Similar to `help wanted`, but useful if the project wants an explicitly community-facing pickup signal.                                                 |
| `duplicate`        | The issue already exists. Close with a link to the original.                                                                                                                                                                 |
| `invalid`          | Not actionable, cannot be reproduced, unsupported as reported, or not a bug/feature request for MonoGame itself. Explain why when applying.                                                                                  |
| `wontfix`          | Intentionally not addressing. Explain why and suggest alternatives where possible.                                                                                                                                           |
| `maintenance`      | Housekeeping work: cleanup, consistency fixes, repo hygiene, minor infrastructure upkeep, or non-feature maintenance work.                                                                                                   |
| `dependencies`     | Dependency bumps, SDK updates, third-party library changes, package version updates, or compatibility work caused by upstream dependency changes.                                                                            |

---

### Examples

| Scenario                                                    | Labels                                                    |
| ----------------------------------------------------------- | --------------------------------------------------------- |
| DesktopVK requesting an sRGB back buffer crashes            | `bug`, `scope: graphics`, `scope: vulkan`                 |
| RenderTarget2D created off the main thread crashes on GL    | `bug`, `scope: graphics`, `scope: opengl`                 |
| Add runtime and dynamic SpriteFont support                  | `feature`, `scope: spritefont`, `scope: content-pipeline` |
| Fix MGCB task failing after `dotnet tool restore` confusion | `bug`, `scope: mgcb`, `scope: tooling`                    |
| Update Windows runner image in CI                           | `enhancement`, `scope: build`                             |
| Trackpad gestures on macOS                                  | `feature`, `scope: input`, `platform: macos`              |
| Vulkan swapchain layout regression after backend change     | `bug`, `scope: vulkan`, `regression`                      |
| Add migration note for a behavior-changing template update  | `docs`, `scope: templates`, `behavior-change`             |
| Add regression test for GPU instancing                      | `enhancement`, `scope: tests`, `scope: graphics`          |
