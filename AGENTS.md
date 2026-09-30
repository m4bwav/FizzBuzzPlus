# AGENTS.md

Rules for any AI agent (Claude Code, Copilot, Cursor, Codex) working in this repository. `CLAUDE.md` and `.github/copilot-instructions.md` only point here.

## What this is

FizzBuzzPlus: the classic FizzBuzz with custom rules. The library `src/FizzBuzzLibrary` has one public class, `FizzBuzzProcessor`. The program `src/FizzBuzzWithOutput` builds as `fizzbuzzplus`. Both are on .NET 10. The code was written in 2014 for .NET Framework 4.5 (tag `v1.0.0`, never released) and modernized in 2026 as 2.0.0 with the package-modernize skill. This was the first run of the skill's variant for repositories that are not packages. Nothing is published to a registry: releases are GitHub Releases of the program. Start with `ai-docs/HANDOFF.md`; the plan is `ai-docs/plans/2026-09-29-modernization-and-v2-release.md`.

## Rules

- **The 2014 behaviour is the contract.** The original sources are frozen byte for byte in `tests/Golden/Original` (git blob ids of commit d13d130). `tests/Golden/Capture` recorded their answers on each runtime and OS, 108 cases per recording. The golden replay (`tests/FizzBuzzLibrary.GoldenTests`) runs the same cases against the new code. Only the ruled changes E1 to E7 in `Exceptions/2.0.0.json` may differ. Nothing under `tests/Golden` changes after the Phase 0 commit 2835ec0 (CI checks `git diff --exit-code 2835ec0 -- tests/Golden`). When the replay fails, fix the code, or get the maintainer's ruling and add the case to the exceptions file with its change id. Never re-record.
- **The 2014 tests stay unchanged.** `FizzBuzzLibrary.Tests` compiles the two frozen 2014 test files by link. That is why the tests use MSTest.
- **Names are kept.** The namespace `FizzBuzzLibrary`, the class and its members are what the capture and the 2014 tests compile against.
- **Nothing is published without the maintainer.** `release.yml` waits at the `release` environment for his approval before it creates the GitHub Release. No registry token exists anywhere.
- **Releases follow one ritual.** Update `CHANGELOG.md` (a release heading carries its date; a prerelease uses its base version's section), set `<Version>` in `Directory.Build.props`, merge, and wait for `ci` to be green on master. Then tag `v<version>` and push the tag. The maintainer approves the run, and `verify-release.yml` checks the Release on every platform. Tag only after green: the release run refuses a commit without a green `ci`.
- **Dependencies.** Lock files are committed, and CI restores with `--locked-mode`. The library and the program list the six release runtime identifiers so that `dotnet publish -r RID` also restores in locked mode. Actions are pinned to commit SHAs. Dependabot runs weekly with a seven-day cooldown.
- **Research beats recall.** SDK, package and action versions change; re-verify any version older than three months. Copy action pins from an existing workflow, never from memory.
- **Document for handoff.** Anything learned, decided or built goes into `ai-docs/` before you finish; rewrite `ai-docs/HANDOFF.md` when work is left unfinished.
- **No AI attribution anywhere.**
- **Line endings.** Files are LF (`.gitattributes`, `.editorconfig`). The frozen files under `tests/Golden` are `-text` and keep their 2014 byte order mark. `.editorconfig` switches formatting and analyzers off for that folder.

## Commands

```
dotnet restore --locked-mode
dotnet format --verify-no-changes
dotnet build -c Release
dotnet test -c Release                                   # Microsoft.Testing.Platform (global.json); 57 tests
dotnet restore -p:AuditPipeline=true --force             # fails on any NuGetAudit finding, as CI does
dotnet publish src/FizzBuzzWithOutput -c Release -r win-x64 -p:RestoreLockedMode=true -o publish
FIZZBUZZPLUS_WRITE_DIFFERENCES=diff.json dotnet test -c Release --project tests/FizzBuzzLibrary.GoldenTests   # lists every case that differs from the recording
bash tests/Golden/capture.sh /tmp/capture                # re-runs the 2014 capture elsewhere (never into tests/Golden)
```

## Layout and traps

- `src/FizzBuzzLibrary`, `src/FizzBuzzWithOutput` (assembly `fizzbuzzplus`, InvariantGlobalization, self-contained trimmed single file when published with a RID). The tests are `tests/FizzBuzzLibrary.Tests` (unit, app, differential against the 2014 code, the 2014 tests), `tests/FizzBuzzLibrary.GoldenTests` and `tests/FizzBuzzLibrary.Original2014` (the frozen source as its own assembly, extern alias `Original2014`).
- `tests/Golden` has empty `Directory.Build.props`, `.targets` and `Directory.Packages.props` files, so the capture never picks up the repository's settings.
- The recordings differ by CPU as well as by OS: on arm64 (macos-latest), arithmetic exceptions come from System.Private.CoreLib.
- `macos-latest` is arm64; osx-x64 has no free runner, so its archive is checked but never run.

## everlast (session knowledge, load on demand)

- `ai-docs/INDEX.md` lists what past sessions learned here (solutions with verified commands, decisions with reasons, plans). At the start of a task, scan it and open only the entries whose title or tags match; no line matches: `everlast.py search "<key terms>"` before concluding nothing was recorded. Read `ai-docs/HANDOFF.md` when continuing unfinished work (everlast-resume skill).
- Before acting on an entry marked `(recheck due)`, run `everlast.py recheck <entry>`, re-run its Verified-by command only when that is read-only or safe (a build, a test, a version query), then record `everlast.py verify <entry>` or `verify <entry> --failed "what broke"`; a fix that changed is superseded, never reused blindly.
- Before finishing a task that hit a dead end, verified a non-obvious command, made a design choice, or taught you something about the user, record it (everlast-capture skill, or `everlast.py note` / `handoff`); rewrite `HANDOFF.md` when work is left unfinished. Say "nothing to record" when that is true.
- Anything naming a person, an internal host or name, a credential, or an opinion about people goes to the private sidecar (`--private`), never here. Lessons about the user or this machine go to the user tier (`--user`).
- Rules go in this file, system layout in CODEMAP.md; the doc set holds only what could not be re-derived from the code in a minute.
- Link documents together with relative markdown links: every markdown folder is reachable from an index whose lines say when to read each file (`ai-docs/INDEX.md` is generated from frontmatter; give entries a one-line `summary`), and an entry links the entries it relates to on a typed `Related:` line (`supersedes`, `contradicts`, `builds on`, `see also`). The set then reads as a graph for people in Obsidian and for agents alike. No wikilinks in the repo.
