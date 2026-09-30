---
title: Modernize as 2.0.0, released as a GitHub Release, not a package
kind: decision
status: accepted
date: 2026-09-29
verified: 2026-09-29
stale_after: never
tags: [v2, decision, compatibility, golden, repository-variant, release]
summary: "accepted at the plan review (2026-09-29, every recommendation stands): modernize rather than archive; the frozen 2014 source is the contract, compared per OS on net10.0 with seven named exceptions; no registry, a gated GitHub Release of the console app for six runtime identifiers; MSTest 4 so the 2014 tests compile unchanged"
---

# Modernize as 2.0.0, released as a GitHub Release, not a package

## Context

FizzBuzzPlus was written in 2014 for .NET Framework 4.5 and never published. Its solution no longer builds on a machine with only the .NET 10 SDK. It is the first repository the package-modernize skill runs on that is not a package, so there is no published artifact to capture and no registry to release to.

## Decision (accepted 2026-09-29)

- **Modernize** (not archive, not delete), as **2.0.0**, with the 2014 commit tagged `v1.0.0`.
- **The contract is the frozen 2014 source.** Byte-exact copies of the four 2014 files live in `tests/Golden/Original` (git blob ids checked by the capture script). The capture compiles them unchanged, and the recordings are kept per runtime and OS. v2 on net10.0 is compared with the net10.0 recording of the OS it runs on. The seven named exceptions (E1 to E7 in the plan) are the only allowed differences. The net48 recording is kept as the 2014 runtime's answer and is not replayed.
- **Fixes go into the existing names** (there is only one class, no callers, and no package to break), each one a changelog line.
- **No registry.** The release is a GitHub Release of the console app for six runtime identifiers, attested, created by a job that waits for the maintainer's approval in a `release` environment, rehearsed as a prerelease, and verified from the Release on fresh runners.
- **MSTest 4** instead of the overlay's NUnit or xunit.v3, because the 2014 MSTest files then compile unchanged. The files' own tests are the author's expectations, and D1 promises them.

## Reasons

- The maintainer asked for a modernization, and the run exists to prove the skill's repository variant.
- With no package there is no other contract than the source; freezing it byte for byte makes the recording provable.
- Comparing per OS on net10.0 keeps the exception table to the real fixes; the runtime's own differences are reported once in the findings note.
- A GitHub Release can be deleted and re-made; a nuget.org package cannot be deleted.

## Alternatives

- Publish on nuget.org as the first brand-new package id (with the app as a dotnet tool). It would teach the new-package Trusted Publishing policy, but it leaves a permanent public package of a kata. nuget.org only unlists; it never deletes.
- Archive with a README note: no cost, and nothing learned.
- Compare v2 with the net48 recording everywhere: every runtime wording difference would then be an exception, for no gain.

## Consequences

The golden files, the capture and the frozen source are fixed from commit 2835ec0 on. A fix that changes a recorded answer needs a ruled exception. The repository variant of the skill gains its own Phase 0 (a frozen source instead of a published package) and its own release phases (a gated GitHub Release instead of a registry).

Related: builds on [../notes/2026-09-29-phase-0-findings.md](../notes/2026-09-29-phase-0-findings.md); see also [../plans/2026-09-29-modernization-and-v2-release.md](../plans/2026-09-29-modernization-and-v2-release.md).
