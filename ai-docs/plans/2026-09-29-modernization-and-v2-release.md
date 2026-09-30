---
title: Modernization and v2 release
kind: plan
status: active
date: 2026-09-29
verified: 2026-09-29
stale_after: never
tags: [v2, plan, repository-variant, dotnet, github-actions, tests, release]
summary: "the living plan for FizzBuzzPlus 2.0.0, the first repository run of package-modernize: modernize rather than archive, survey, what the 2014 code gets wrong, decisions D0-D16, named golden exceptions E1-E7 and the runtime rule E8, API, test strategy, phases 0-7 with a GitHub Release instead of a registry, security, verification checklist"
---

# Modernization and v2.0.0 release plan: FizzBuzzPlus

This plan takes FizzBuzzPlus from its 2014 state (Visual Studio 2012, .NET Framework 4.5, never published) to 2.0.0 on current .NET, released as a GitHub Release of the console app. It is the first run of the package-modernize skill on a repository that is not a package: phases 0 to 4 and 7 follow the skill (SKILL.md, references/nuget.md), and phases 5 and 6 are replaced by a gated GitHub Release (D13). The variant is drafted as the run goes in [../notes/2026-09-29-repository-variant.md](../notes/2026-09-29-repository-variant.md). Evidence goes to [../log.md](../log.md) as it lands; this file's checkboxes and [../HANDOFF.md](../HANDOFF.md) are current at every stop. The Phase 0 facts are in [../notes/2026-09-29-phase-0-findings.md](../notes/2026-09-29-phase-0-findings.md).

## Status

Active, stopped at the pull request review (2026-09-29): pull request #1 green on three OSes, the independent review's 7 findings fixed or answered (b901e34, comment on the pull request). Next: the maintainer's merge, then Phase 5. Rulings at the plan review, 2026-09-29: every recommendation D0 to D16 and E1 to E7 stands, and the GitHub writes were approved ("do all the recommendations and tags"); the settings, rulesets, `release` environment and `v1.0.0` tag are in place (log).

## Goal

- One class, `FizzBuzzProcessor`, with the same three constructors-and-methods surface, on .NET 10, answering every question the 2014 code answered the way it answered it, apart from the named exceptions E1 to E7, each a changelog line.
- No endless loops, no half-written line before an error, the same text on every machine and culture.
- The 7 tests of 2014 compile unchanged against the new library and pass, beside a real test suite.
- A console app that runs anywhere (Windows, Linux, macOS; x64 and Arm64) without .NET installed, released as a GitHub Release built, attested and gated by the maintainer's approval, verified from the Release on every OS.
- The repository tidy and protected (licence, settings, rulesets, secret scanning), and a wiki.

## Modernize, archive or delete (the first question for every repository)

| Option | What it costs | What it gives |
|---|---|---|
| **Modernize (recommended; the maintainer asked for it)** | One run; about 150 lines of code change hands | A working, tested, released example, and the skill's repository variant proven on something small enough to see every step |
| Archive | A README line and `gh repo archive` | Nothing learned; the repository keeps failing to build |
| Delete | The maintainer's OK; history lost | Nothing; the repository costs nothing to keep |

## Where it stands (survey 2026-09-29)

| Fact | Value | Evidence |
|---|---|---|
| Published version, downloads, dependents | Never published; the NuGet id FizzBuzzPlus is free; no dependents; 0 stars, 0 forks | survey note, log |
| Source, build, tests, language | VS 2012 solution, three ToolsVersion 4.0 projects on .NET Framework 4.5, AssemblyInfo.cs version 1.0.0.0; C# 5; 7 MSTest v1 tests; the test project references the VS 2010 Coded UI assemblies | findings note |
| Entry points and how the README says to use it | `FizzBuzzProcessor` (two constructors, three `Execute` overloads) and the `FizzBuzzWithOutput` console app; the README says only "the classic fizz buzz problem with some addtional features" | README.md |
| Runtime dependencies | None beyond the framework | csproj files |
| Issues, pull requests, forks, branches | None, none, none; `master` only (plus this run's scratch branch `golden-capture`) | survey |
| Alerts, webhooks, secrets, security features | Dependabot alerts disabled; 0 webhooks; 0 secrets; secret scanning, push protection and security updates off; default workflow permissions write, Actions may approve pull requests | survey |
| Dead services | None (no CI, badges or config files) | survey |
| README images and badges | 0 images (`check-readme-images.mjs README.md --registry nuget` exit 0) | log |
| Leaked credentials | None (the history holds only assembly public-key tokens) | log |
| Licence | None | survey |
| Baseline | `dotnet build FizzBuzzPlus.sln` fails with MSB3644 (no .NET Framework 4.5 reference assemblies); the 7 tests of 2014, unchanged, pass on net48 and net10.0 against the unchanged source | log |
| Golden capture | 108 cases, recorded from the frozen 2014 source on net48 and net10.0 (Windows, x64) and on net10.0 (Linux x64, macOS arm64), each twice byte-identical | tests/Golden, log |

## What the 2014 code gets wrong, confirmed, and what v2 does

Case names are the recording's `group | name`. "Changes" counts the net10.0 cases whose recorded answer differs from v2's; each is listed in the golden test's exception table.

1. **Endless output at `long.MaxValue`** (`edge | Execute(long.MaxValue)`, `edge | Execute(long.MaxValue - 2, long.MaxValue)`): the counter wraps to `long.MinValue`, and the loop never ends. The one-argument form is endless too. Fix (E1): stop after `long.MaxValue`. Changes 2 cases; `Execute(long.MinValue, long.MaxValue)` is still 2^64 numbers long and still stopped by the capture's guard at the same 3 lines. Changelog: "Execute no longer loops forever when the range ends at long.MaxValue."
2. **The text depends on the machine's culture on .NET** (`culture | sv-SE`, `nb-NO`, `he-IL`, `fa-IR`, `ar-SA`): the number is formatted with the current culture, and .NET 10's ICU data writes U+2212 or a direction mark before the digits, where .NET Framework, the 2014 runtime, wrote "-". Fix (E2): format with the invariant culture. v2 then answers what the 2014 code answered on its own runtime. Changes 5 cases on each OS. Changelog: "Numbers are written with the invariant culture: a negative number is written with '-' on every machine."
3. **A rule of 0 throws DivideByZeroException in the middle of the output** (`rules | divisor 0: Execute(1, 2)` and two more). Fix (E3): the constructor refuses it with ArgumentException (parameter `outputBreaks`), so the error comes before any output. Changes 3 cases. Changelog: "A rule of 0 is refused by the constructor."
4. **A rule of -1 throws OverflowException at `long.MinValue`** (`rules | divisor -1: Execute(long.MinValue)`, `rules | divisor 2 and -1: Execute(long.MinValue)`): `long.MinValue % -1` overflows. Every number is divisible by -1, so v2 writes the word (E4). Changes 2 cases. Negative rules otherwise keep their recorded, mathematical meaning (-3 matches the multiples of 3).
5. **A null rules dictionary or writer fails late** (every `null` case): ArgumentNullException from inside LINQ, or NullReferenceException, only when `Execute` runs and after "Current Number: 1 " is written, and not at all for an empty range. Fix (E5): the constructors throw ArgumentNullException for `outputBreaks` and `writer`, rules first. Changes 9 cases.
6. **The caller's dictionary is kept by reference, and the rules are sorted again for every number** (every `aliasing` case). Fix (E6): the constructor copies and sorts the rules once, so later changes to the caller's dictionary have no effect. Changes 5 cases. Changelog: "The rules are copied when the processor is created."
7. **The console app cannot run in a pipeline or CI** (`app | ...`): it prints 15 to 30, then `Console.ReadKey()` throws InvalidOperationException when input is redirected (exit code 0xE0434352 on Windows, 134 on Linux and macOS), and it ignores its arguments. Fix (E7, D12): no ReadKey; arguments for the range and the rules; exit code 0. Changes both app cases on each OS.
8. **Runtime wording** (E8, not a code change): on each OS, v2 is compared with the net10.0 recording of the same OS. The net48 recording is kept as the 2014 runtime's answer. Its differences from net10.0 (the ArgumentNullException wording, source assemblies, the console app's crash line, the five cultures) are the runtime's, and are reported, not replayed.

Kept, and documented in the README: every line ends with "\n" whatever the writer's `NewLine` (the old tests assert it, and Environment.NewLine would make the output depend on the OS); a number with no word keeps its trailing space; start greater than end writes nothing; words are written in ascending order of their divisor, whatever the dictionary's own order; the writer sees the same calls (the prefix, each word, "\n", `null` for a null word; no Flush); words go out unchanged (control characters, surrogates).

## Decisions (recommendation first; the maintainer rules in the plan review, silence means the recommendations stand)

| # | Question | Recommendation | Why | Alternative |
|---|---|---|---|---|
| D0 | Modernize, archive or delete | Modernize | The maintainer asked for it; the run proves the repository variant | Archive with a README line |
| D1 | The compatibility promise | Every recorded case keeps its answer on each OS (net10.0 against that OS's recording), apart from E1 to E7; the 7 tests of 2014 compile unchanged and pass; the golden files, the capture and the frozen source never change after the Phase 0 commit | The source is the only contract there is, and the recording makes it exact | Compare with the net48 recording and name every runtime difference too (more exceptions, no gain) |
| D2 | Shape: namespace, class, project names | Keep namespace `FizzBuzzLibrary`, class `FizzBuzzProcessor`, projects `FizzBuzzLibrary` and `FizzBuzzWithOutput`, moved under `src/`; tests under `tests/` | The capture's Cases.cs and the 2014 tests compile unchanged only against the same names | Rename to `FizzBuzzPlus` everywhere (breaks the unchanged compile; needs a type forward) |
| D3 | Behaviour at the edges | E1 to E7 as listed above; everything else kept | Each fix removes a hang, a half-written line or a machine-dependent answer | Keep the throw-late behaviour and only document it |
| D4 | Version | 2.0.0; tag the 2014 commit d13d130 as `v1.0.0` (the AssemblyVersion it declared), with no release | E1 to E7 change answers the 1.0.0 code gave; a 1.0.0 tag gives the recordings (`1.0.0.*.json`) and the changelog a name to compare with | Call the first release 1.0.0, since nothing was released before |
| D5 | Runtime dependencies | None | The library needs only the base class library | - |
| D6 | Names added | None. Considered and left out: a method that returns the lines instead of writing them (a StringWriter already does it), a CancellationToken for long ranges (the writer can throw; E1 removes the one hang that no writer could stop), sealing the class (a break for any subclass, for no gain) | "Do not gold-plate"; each addition is upkeep | Add `GetLines(long start, long end)` returning a sequence computed as it is read, if an example of it is wanted |
| D7 | Errors | Constructors validate (E3, E5): ArgumentNullException for a null dictionary or writer, ArgumentException for a rule of 0; `Execute` throws only what the writer throws | Errors before any output, at the call that caused them | Never throw: skip a rule of 0 silently (hides a caller's bug) |
| D8 | Target frameworks and CI matrix | Library, app and tests on `net10.0` only; CI on Ubuntu, Windows and macOS | Nothing is published, so no caller needs netstandard2.0 or net48 (the overlay: netstandard2.0 only when published); the net48 recording stays as history | `netstandard2.0;net10.0` plus net48 tests, if D13 turns to publishing |
| D9 | Tooling | The skill's .NET defaults from the templates: SDK-style projects, `.slnx`, `Directory.Build.props` (warnings as errors, analyzers latest-recommended, code style in build, nullable, deterministic, lock files), `global.json` 10.0.100 with latestFeature, `dotnet format` in CI, `.editorconfig` and `.gitattributes` for LF. Tests: **MSTest 4** (4.4.1) on Microsoft.Testing.Platform | MSTest keeps the 2014 test files compiling unchanged (same namespace and attributes), which is the D1 promise; NUnit and xunit.v3 would need a port | NUnit 4.6.1 (5.0.0 is inside the three-day cooldown until 2026-09-30) or xunit.v3 4.0.1 with the old tests ported by hand |
| D10 | Lock files and Dependabot | `packages.lock.json` committed, `--locked-mode` in CI; Dependabot for nuget, github-actions and dotnet-sdk, weekly, seven-day cooldown | Template defaults | - |
| D11 | README, badges, licence, repository metadata | New README: what it does, the app's usage, the library's API and behaviour at the edges (including the kept "\n" and trailing space), how to build and test, the upgrade notes from 1.0.0; two badges: CI and the latest release. LICENSE: MIT, "Copyright (c) 2014-2026 Mark Rogers". Description fixed ("additional"), topics `fizzbuzz csharp dotnet kata console-app`, homepage the Releases page | There is no package page, so no version or download badge from a registry; the overlay's licence is MIT | Add a downloads badge for the Release assets |
| D12 | The console app | Keep it as the released program: no ReadKey; `fizzbuzzplus` prints 15 to 30 as in 2014; `fizzbuzzplus N` one number; `fizzbuzzplus START END` a range; `--rule N=WORD` (repeatable) replaces the default rules; `--help`; exit 0, or 2 with the usage on stderr for bad arguments. Assembly name `fizzbuzzplus` | A command-line program that waits for a key is wrong in a terminal and in pipelines; Visual Studio keeps its own console open since 2019; the rules option is the only way to show the "Plus" | Keep ReadKey when input is not redirected (the 2014 F5 habit); a dotnet tool instead (needs nuget.org, D13) |
| D13 | Release | **No registry.** A GitHub Release of the app, self-contained single-file for win-x64, win-arm64, linux-x64, linux-arm64, osx-x64 and osx-arm64 (trimmed, invariant globalization), zipped per OS, with SHA256SUMS and build-provenance attestations, from `release.yml` on a `v*` tag; the Release is created by a job gated by a `release` environment with the maintainer as required reviewer; rehearsal `v2.0.0-beta.1` as a prerelease first; `verify-release.yml` downloads the assets on fresh runners of every OS, checks sums and attestations, and runs the app | Proves the repository variant's release (the example sites need the same gated deploy); a toy FizzBuzz on nuget.org cannot be deleted there, only unlisted | Publish FizzBuzzPlus 1.0.0 (or 2.0.0) on nuget.org and the app as a dotnet tool: phases 5 and 6 as for a package, a Trusted Publishing policy for new packages before the first push, netstandard2.0 added. Teaches the first brand-new package id and the tool policy; costs a permanent public package |
| D14 | Default branch and extras | Keep `master`; no benchmark project (nothing hot); no icon (no registry page shows one) | Template defaults, nothing to measure | - |
| D15 | Repository settings and security | Secret scanning, push protection, private vulnerability reporting, Dependabot alerts and security updates on; default workflow permissions read, Actions may not approve pull requests; delete branch on merge; ruleset on `master` (no deletion, no force push, required check `ci`, admin bypass) before the merge; tag ruleset (admins only); SECURITY.md | The skill's standing settings; applied when the pull request opens so the merge is gated (L-077) | - |
| D16 | Wiki | Phase 7 with wikiwright after the release is verified; needs the maintainer's first-page click | The overlay: every public repository gets one | Skip the wiki for a repository this small |

## Proposed public API (v2)

Unchanged surface (the `api` case of the recording must still hold every line):

- `public class FizzBuzzProcessor`
  - `FizzBuzzProcessor(IDictionary<long, string> outputBreaks, TextWriter writer)`: copies and sorts the rules; throws ArgumentNullException for a null `outputBreaks` or `writer`, ArgumentException (`outputBreaks`) for a rule of 0.
  - `FizzBuzzProcessor(TextWriter writer)`: the rules 3 "Fizz" and 5 "Buzz"; throws ArgumentNullException for a null `writer`.
  - `void Execute()`: 1 to 100.
  - `void Execute(long singleValue)`: one number.
  - `void Execute(long startValue, long endValue)`: every number from start to end inclusive, ending after `long.MaxValue`; nothing when start is greater than end.
- Nullable annotations: `IDictionary<long, string?>` stays `IDictionary<long, string>` in the signature (a null word is still accepted and writes nothing), XML docs on every member.

## Build and layout

- src/FizzBuzzLibrary/FizzBuzzLibrary.csproj (net10.0, not packable), src/FizzBuzzWithOutput/FizzBuzzWithOutput.csproj (net10.0, Exe, AssemblyName `fizzbuzzplus`, InvariantGlobalization, publish settings for the six runtime identifiers).
- tests/FizzBuzzLibrary.Tests/ (MSTest 4: new unit tests with the arguments in the right order, the app's tests, and the four frozen 2014 test files compiled by link from tests/Golden/Original), tests/FizzBuzzLibrary.GoldenTests/ (the replay: compiles tests/Golden/Capture's Cases.cs and Json.cs by link, runs them against the new library and the new app, compares each case with the recording of the current OS, one exception table for E1 to E7).
- FizzBuzzPlus.slnx, Directory.Build.props, global.json, .editorconfig, `.gitattributes`, `.gitignore` (the Visual Studio template, replacing the 44-line list), .github/workflows/ci.yml, release.yml, verify-release.yml, .github/dependabot.yml.
- Removed: FizzBuzzPlus.sln, the three old csproj files, the AssemblyInfo.cs files, App.config, the FizzBuzzTest folder (its two files live on, frozen, in tests/Golden/Original).

## Phases

### Phase 0: survey and baseline (2026-09-29, no code changed)
- [x] Cloned to labs/FizzBuzzPlus; survey in notes/2026-09-29-survey.txt; findings note
- [x] Old build as it is: MSB3644; the 7 tests of 2014 unchanged: 7 of 7 on net48 and net10.0
- [x] Golden capture of the frozen 2014 source: tests/Golden (Windows net48 and net10.0 in 7c455b9 and a7e17ed; Linux x64 and macOS arm64 from golden-capture run 36645600812 in 2835ec0, **the Phase 0 commit**); from here on the golden JSON, the capture, OriginalApp and Original never change
- [x] everlast registered (mode repo, sync push); AGENTS.md, CLAUDE.md with the AGENTS.md import line, Copilot pointer
### Phase 1: plan
- [x] This plan and the decision record [../decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md](../decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md). **Stop**: the maintainer rules on the tables; the questions below.
### Phase 2: rewrite on branch v2
- [x] Remove the old projects; add the templates (adapted for net10.0 only, no pack); LICENSE; `.gitignore`
- [x] Golden test first, green on the first build apart from the E1 to E7 table; canary: a planted line in src/ turns it red, reverted, green (both runs logged); golden files unchanged since the Phase 0 commit (`git diff --exit-code`)
- [x] The library (E1 to E6), the app (E7, D12), the unit tests, the 2014 tests by link, README, CHANGELOG (first paragraph: the promise and E1 to E7), SECURITY.md, AGENTS.md
- [x] Verified locally and from a fresh clone with the workflow's exact commands; `dotnet publish` for one runtime identifier run and checked
- [x] Workflows and Dependabot from the templates, actions pinned to SHAs, actionlint with shellcheck, zizmor, `check-workflow-shell.py` clean
- [x] Pushed; pull request opened with a "For review" list
### Phase 3: review
- [x] Independent read-only review (prompts/review-subagent.md, with a differential fuzz against the frozen 2014 source); findings fixed or answered; summary on the pull request; D15 settings and rulesets applied. **Stop: pull request review.**
### Phase 4: CI, settings, merge, cleanup
- [x] CI green (run 36653471093 on master 7c567fd; #1 merged by m4bwav as a merge commit); merge after the maintainer's review (read the SHA and method back); `v1.0.0` tag on d13d130; the golden-capture branch deleted (with the OK); repository metadata
### Phase 5: release rehearsal (replaces the registry rehearsal)
- [ ] The maintainer creates nothing on a registry; the `release` environment exists with him as required reviewer (the run creates it; he checks it). `v2.0.0-beta.1` tagged after `ci` is green on master; the Release job waits; **stop** for the approval; `verify-release.yml` green (run id)
### Phase 6: release
- [ ] Changelog dated; `v2.0.0` tagged after green; **stop** for the approval; `verify-release.yml` green; `gh attestation verify` on every asset
### Phase 7: wrap-up
- [ ] Wiki with wikiwright (the first-page click asked for at the start of the phase); HANDOFF.md around standing work; inventory row moved; the repository variant proposed to the skill; lessons and changelog entries in the skill; the kickoff's corrections section; the next repository

## Test strategy

| Layer | What it proves | How | Runs where |
|---|---|---|---|
| Golden replay | Every 2014 answer kept, apart from E1 to E7 | Cases.cs of the capture, compiled unchanged, against the new library and app; compared with the current OS's recording as JSON text | CI on Ubuntu, Windows, macOS |
| The 2014 tests | The 2014 author's own expectations still hold | The frozen files compiled by link into the test project | Everywhere |
| Unit tests | E1 to E7 and the kept behaviour, with the arguments in the right order | MSTest 4 | Everywhere |
| Differential | The new library against the 2014 source on generated inputs | A test compiling Original/FizzBuzzProcessor.cs under another namespace and comparing outputs for seeded random ranges and rule sets, apart from the E1 to E6 inputs | Everywhere |
| App | Arguments, exit codes, output bytes, usage text | Tests starting the built app as a process | Everywhere |
| Published app | The released binaries run on their OS without .NET | `verify-release.yml` on fresh runners for each runtime identifier the runners offer (x64 on all three; arm64 on macOS and Linux) | After each release |

## Pull requests, issues and forks: disposition

None exist. The branch `golden-capture` (this run's throwaway workflow, commit on top of v2) is deleted in Phase 4, with the maintainer's OK.

## Security

No credentials in files or history; no webhooks; no apps (the maintainer checks github.com/settings/installations, which the API cannot read). D15's settings; workflows with contents set to read by default, id-token and attestations set to write only in the attest job, contents set to write only in the gated Release job; `persist-credentials: false`; actions pinned to SHAs; zizmor and actionlint clean; a three-day cooldown on new dependency versions (Dependabot seven days). The library: no I/O but the writer it is given, no reflection, no unbounded work other than the range the caller asks for. SECURITY.md with private vulnerability reporting.

## Badges and images: disposition

| Image or badge | What it shows now | Decision | New URL or reason |
|---|---|---|---|
| (none in the 2014 README) | - | - | - |
| New: CI | - | Add | the ci.yml workflow badge |
| New: latest release | - | Add | shields.io GitHub release badge for m4bwav/FizzBuzzPlus |

## Verification checklist

| Claim | Command or place | Expected |
|---|---|---|
| Restores clean, locked | `dotnet restore --locked-mode` in a fresh clone | No NU1004, no audit warnings |
| Format and analyzers | `dotnet format --verify-no-changes`; `dotnet build -c Release` | No changes, no warnings |
| Old behaviour kept | the golden replay in `dotnet test` on three OSes | 108 cases, only E1 to E7 differ |
| Golden files untouched | `git diff --exit-code <phase-0 commit> -- tests/Golden` | Empty |
| The 2014 tests | `dotnet test` | 7 pass, unchanged files |
| Release assets | `gh release view v2.0.0`; SHA256SUMS; `gh attestation verify --format json` per asset | Six archives, sums match, attestations name release.yml |
| Runs without .NET | `verify-release.yml` | Green on every OS |
| Repository tidy | `gh api` settings, rulesets, hooks | Scanning on, 0 webhooks, rulesets active |
| Wiki live | wikiwright's live check | Every page answers |

## Risks and open points

- Trimming and single-file on osx-x64 and win-arm64 are built on runners but only run where a runner of that kind exists; osx-x64 has no free runner after macos-13's retirement, so it is verified only by its checksum and attestation. Unsigned macOS binaries are quarantined by Gatekeeper when downloaded by a browser; the README says how to clear it.
- `Execute(long.MinValue, long.MaxValue)` is still 2^64 numbers long by the caller's own choice; the README says so.

## Questions for the plan review

1. The rulings on D0 to D16 and E1 to E7 (silence: the recommendations stand).
2. GitHub writes this run will make, for one go now: create the `v1.0.0` tag on d13d130 (D4); apply the D15 settings and the two rulesets; create the `release` environment with you as required reviewer; set description, topics and homepage; delete the scratch branch `golden-capture` after this plan is ruled on.

## Next single action

The maintainer's rulings; then Phase 2 on branch v2.
