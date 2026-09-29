# Handoff

<!-- Keep under 50 lines. Replace, never append. Written at the end of a work session so the next one starts without re-deriving state. -->

## Current state
- package-modernize run, repository variant (kickoff: package-modernization/prompts/2026-09-29-fizzbuzzplus-kickoff.md). Branch `v2`, pushed. Phases 0 and 1 done on 2026-09-29.
- Golden capture of the frozen 2014 source: tests/Golden (108 cases; net48 and net10.0 on Windows x64, net10.0 on Linux x64 and macOS arm64). **Phase 0 commit 2835ec0**: `git diff --exit-code 2835ec0 -- tests/Golden` must stay empty.
- Plan: [plans/2026-09-29-modernization-and-v2-release.md](plans/2026-09-29-modernization-and-v2-release.md) (D0 to D16, E1 to E7, questions at the end). Findings: [notes/2026-09-29-phase-0-findings.md](notes/2026-09-29-phase-0-findings.md). Variant draft: [notes/2026-09-29-repository-variant.md](notes/2026-09-29-repository-variant.md).
- The scratch branch `golden-capture` (throwaway workflow) is on origin; deleting it waits for the maintainer's OK.

## In progress
- **Stopped at the plan review.** Waiting for the maintainer's rulings (silence: the recommendations stand) and one go for the GitHub writes listed under "Questions for the plan review" in the plan.

## Decisions made this session
- Proposed, not ruled: [decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md](decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md).
- Taken as run mechanics: v2 is compared per OS with the net10.0 recording of that OS; net48 is history. The capture header records the CPU architecture, because ARM64 raises arithmetic exceptions from a runtime helper.

## Dead ends hit
- The old solution cannot build here (MSB3644); do not install a targeting pack. The Microsoft.NETFramework.ReferenceAssemblies package builds net48 instead.
- WSL Ubuntu has no .NET SDK; Linux and macOS recordings come from the golden-capture workflow, not from WSL.

## Next single action
Take the maintainer's rulings first-hand (one question if a new session: skill L-022), then Phase 2 on `v2`: remove the old projects, add the templates, write the golden replay first.
