# Handoff

<!-- Keep under 50 lines. Replace, never append. Written at the end of a work session so the next one starts without re-deriving state. -->

## Current state
- package-modernize run, repository variant (kickoff: package-modernization/prompts/2026-09-29-fizzbuzzplus-kickoff.md). Phases 0 to 3 done on 2026-09-29. The maintainer ruled at the plan review that every recommendation stands, and the GitHub writes are done (log): rulesets 24218995 (master, required check `ci`) and 24218996 (tags, admins only), security settings, the `release` environment (required reviewer m4bwav, tags `v*`), tag `v1.0.0` on d13d130, scratch branch deleted.
- **Pull request https://github.com/m4bwav/FizzBuzzPlus/pull/1** (branch v2, head b901e34) waits for the maintainer's review and merge; ci run 36651132613 green on Linux, Windows, macOS; review summary posted as a comment.
- Phase 0 commit 2835ec0: `git diff --exit-code 2835ec0 -- tests/Golden` must stay empty (a CI step).
- Skill lessons: package-modernize #22 merged (L-136 to L-141); #23 open (L-142, the closed-pipe trap). Records: package-modernization #20 merged.

## In progress
- **Stopped at the pull request review** (Phase 3 stop). Nothing else is running.

## Decisions made this session
- [decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md](decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md) (accepted). The ruled changes E1 to E7 are in `tests/FizzBuzzLibrary.GoldenTests/Exceptions/2.0.0.json` (28 cases).
- The program writes to a FileStream on the standard output handle, because .NET's console stream swallows a closed pipe on Windows and Unix (review finding; skill L-142).

## Dead ends hit
- The old solution cannot build here (MSB3644); do not install a targeting pack.
- WSL Ubuntu has no .NET SDK, but runs the self-contained linux-x64 build: publish it, copy it to /tmp and run it with `MSYS_NO_PATHCONV=1 wsl.exe -d Ubuntu -- bash /mnt/c/.../script.sh`.
- Console.OpenStandardOutput() for a long-running writer: see above.

## Next single action (a fresh session)
After the maintainer merges #1: read the merge SHA and method back (`gh pr view 1 -R m4bwav/FizzBuzzPlus --json mergeCommit,mergedAt`), wait for `ci` green on master, then Phase 5. Put `<Version>2.0.0-beta.1</Version>` in Directory.Build.props through a small pull request (the ruleset requires ci), and after green on master, tag `v2.0.0-beta.1` and push the tag. release.yml waits at the `release` environment: **the maintainer approves in the browser** (agents never call pending_deployments). Then `verify-release.yml` runs on the published Release; check its run. Its runners `ubuntu-24.04-arm` and `windows-11-arm` are unproven. Phase 6 does the same for 2.0.0, with the CHANGELOG heading dated. Phase 7: ask for the wiki's first page at https://github.com/m4bwav/FizzBuzzPlus/wiki/_new at the start, run wikiwright, then propose ai-docs/notes/2026-09-29-repository-variant.md to the skill as references/repository.md, update the inventory row and the kickoff's corrections section, and name the next repository.
