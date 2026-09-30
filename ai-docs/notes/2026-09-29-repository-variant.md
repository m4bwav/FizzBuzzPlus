---
title: The package-modernize repository variant (draft from the FizzBuzzPlus run)
kind: note
date: 2026-09-29
verified: 2026-09-29
stale_after: never
tags: [repository-variant, skill, package-modernize, golden, release]
summary: "the draft of references/repository.md for the package-modernize skill: which phases apply to a repository that was never published, what replaces the golden capture of a published version and the registry release, the modernize-archive-delete question; updated at every phase of the FizzBuzzPlus run"
---

# The repository variant (draft)

## Summary

For a repository that was never published: the survey is GitHub only; the frozen source of the last old commit replaces the published package as the golden reference; recordings per runtime, OS and CPU come from the maintainer's machine plus a throwaway runner workflow; the plan starts with modernize, archive or delete; and the release is none, a gated GitHub Release, or a first publish.

Drafted during the FizzBuzzPlus run (2026-09-29). It becomes references/repository.md in the package-modernize skill at the end of the run, when the run has proved it. Each section says which phase of the run it comes from. Marked *unproven* until the run reaches that phase.

## When it applies

A repository that was never published to a registry: an example site, a kata, a tool used from source, a library nobody packaged. The survey finds no package (the registry search, for example nuget.org's `packageid:` query, returns nothing). When the registry id is free, the plan can still choose to publish; that run then takes the package phases (5 and 6) as they are.

## Phase 0: survey and baseline (proven on FizzBuzzPlus)

- **Survey:** `scripts/survey-github.sh OWNER/REPO` only; there is no registry side. Check that the id is free on the registry the plan might publish to, and record the repository's visibility (a private repository runs CI on the maintainer's runner, the skill's references/private-repo-ci.md).
- **Baseline:** build as it is (old .NET projects fail with MSB3644 on an SDK-only machine), then the old tests unchanged against the old source in a scratch SDK project (the L-079 route, with the source in place of the published package).
- **The reference is the last old commit's source, frozen.** Copy each file the capture needs byte for byte with `git show <commit>:<path> > tests/Golden/Original/<file>`, check `git hash-object` against `git rev-parse <commit>:<path>`, write both ids in a README beside them, and mark the folder `-text` in `.gitattributes`. The capture script checks the blob ids before every run. This replaces "install the published version": the frozen files are what the capture compiles, and the rewrite can change the originals freely.
- **The capture compiles the frozen source unchanged**, beside a Cases file that touches only public names (the replay later compiles the same Cases file against the new code). An app in the repository is rebuilt from its frozen source as its own project, and the capture runs it as a process with redirected input, recording stdout, the exit code and the first stderr line.
- **One recording per runtime and OS a user can have,** each run twice and compared. The maintainer's machine gives Windows (net48 and net10.0). Linux and macOS come from a throwaway workflow on a scratch branch (two runs, compared with `cmp`, JSON uploaded as an artifact, the artifact's hash checked against the log). Record the process architecture in the header: macos-latest is arm64, and it raises arithmetic exceptions from a runtime helper, so an exception's source differs by CPU.
- **Output that never ends** is recorded by a writer guard that throws after a fixed number of lines. The recording says so ("stoppedByCaptureAfterLines"), and the replay of fixed code shows the difference as a named exception.

## Phase 1: plan (proven on FizzBuzzPlus)

- The first row, before the survey table: **modernize, archive or delete**, with what each costs and gives. Deleting needs the maintainer's OK.
- The compatibility promise compares the new build with the recording of the same runtime and OS. The old runtime's recording (net48 for a .NET Framework repository) is kept as history, with its differences reported, not replayed.
- Version: the old code's declared version (AssemblyVersion, package.json) names the recordings; the release is the next major when the promise has exceptions. Tag the last old commit with the old version (no release) so the changelog has something to compare with.
- The target floor drops to what the repository's own consumers need: with nothing published, only the current LTS (the overlay: netstandard2.0 only when published).
- Test framework: prefer the one the old tests compile under unchanged, so the old author's expectations stay part of the contract.
- The release question has three answers: **none** (source only; phases 5 and 6 skipped), **a GitHub Release** of built artifacts (below), or **publish** to a registry (the package phases, with a policy that allows new packages).

## Phase 2: rewrite (proven on FizzBuzzPlus, 2026-09-29)

- The replay compiles the capture's Cases file unchanged against the new code. It also drives the new app, through the same case functions, by pointing the capture's app path at the new build. The app project is referenced from the replay's test project, so its dll lands in the test output.
- The ruled exceptions live in one file beside the replay (`Exceptions/<version>.json`), one entry per case with its change id. The file is made from the replay's own difference list, written only when an environment variable asks for it, then reviewed case by case. Copying the replay's text keeps the escapes byte-exact. The replay fails on an unlisted difference, on a listed case that no longer differs, and on a change id the plan does not have. One file serves every OS when the new answers are the same everywhere.
- The old tests compile unchanged into the new test project by link, when the test framework allows it (MSTest here). The frozen old source compiles into an assembly of its own, referenced under an extern alias, for a differential test that runs both on random inputs. Keep the generator's ranges from wrapping: at the edges of `long`, a negative length turned into a range of 2^64 numbers.
- A program released per runtime identifier: list the RIDs in the program and in every project it references, or a locked-mode `dotnet publish -r RID` fails with NU1004 on the library's lock file. Then publish every RID in CI too (one per runner OS) and run the binary.
- Workflows for a repository without a registry: ci.yml without pack and consumers, and with a publish and smoke run; release.yml and verify-release.yml as described under Phases 5 and 6.

## Phases 3 and 4 (unchanged from the skill; *unproven* here until the run reaches them)

The independent review, CI, settings and cleanup run as for a package. The settings and rulesets can be applied at the plan review when the maintainer approves them there, which puts them in place before the pull request exists.

## Phases 5 and 6: a gated GitHub Release instead of a registry (*unproven*)

- `release.yml` on a `v*` tag: the tag must match the version in the project and sit on the default branch, and `ci` must be green on it. It builds and tests, publishes the app per runtime identifier (self-contained, single file), archives the builds and writes SHA256SUMS, attests every archive, then creates the Release from a job in a `release` environment with the maintainer as required reviewer. The environment is the human gate a registry's staging would otherwise give.
- Rehearsal: a prerelease tag (`vX.0.0-beta.1`), approved, then verified. `verify-release.yml` downloads the assets on fresh runners, checks the sums and `gh attestation verify`, and runs each binary its runner can run.
- **The Release job starts the verify workflow itself** (proven by reading, 2026-09-29; the run is still to come). A Release created with the GITHUB_TOKEN fires no `release` event for other workflows (only `workflow_dispatch` and `repository_dispatch` are exempt), so `on: release: published` alone never runs. The gated job has `actions: write` and ends with `gh workflow run verify-release.yml --ref master -f tag="$TAG"` (FizzBuzzPlus pull request #2; skill L-144 `token-release-fires-nothing`).
- The version goes in through a small pull request, because the master ruleset requires `ci`. In .NET, a test project's lock file records the referenced project's version, so the bump changes lock files too: `scripts/bump-version-dotnet.sh` in the skill does it in one call (L-146).
- A site's release is its deploy: the same environment gate in front of the deploy job, and a live check of the deployed site in place of `verify-release.yml` (*not proven by a C# library; see below*).

## Phase 7

The wiki with wikiwright. It verifies examples against the Release's binaries or the source at the tag, since there is no registry.

## What one small C# library can and cannot prove

It proves the frozen-source capture, the per-OS recordings from a scratch workflow, the plan's first question and the gated GitHub Release. It cannot prove anything about the example websites still on the inventory list:
- Their golden capture: recorded HTML or screenshots of a running site, which needs a way to run a 2014 ASP.NET MVC site at all.
- Their release: a deploy target, with hosting costs, and a decision the maintainer may prefer to answer with "archive".
- Their dependencies: front-end libraries from 2014 (jQuery, Angular 2 betas), with advisories to handle.

Related: see also [../plans/2026-09-29-modernization-and-v2-release.md](../plans/2026-09-29-modernization-and-v2-release.md), [2026-09-29-phase-0-findings.md](2026-09-29-phase-0-findings.md).
