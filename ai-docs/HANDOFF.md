# Handoff

<!-- Keep under 50 lines. Replace, never append. Written at the end of a work session so the next one starts without re-deriving state. -->

## Current state
- package-modernize run, repository variant (kickoff: package-modernization/prompts/2026-09-29-fizzbuzzplus-kickoff.md). Phases 0 to 4 done; Phase 5 (rehearsal) started 2026-09-29.
- Pull request #1 (v2) merged by m4bwav as merge commit 7c567fd; ci run 36653471093 green on master. Rulesets, security settings, the `release` environment and tag `v1.0.0` were done at the plan review.
- Pull request #2 (version 2.0.0-beta.1; release.yml now dispatches verify-release.yml, because a Release made with the GITHUB_TOKEN fires no `release` event) is merged: 0408825, a merge commit. ci run 36657270740 on the branch was green.
- Phase 0 commit 2835ec0: `git diff --exit-code 2835ec0 -- tests/Golden` must stay empty (a CI step).
- Skill lessons: package-modernize #25 open for review (lint-workflows.sh, bump-version-dotnet.sh, L-144 to L-146).

## In progress
- **Stopped before the tag `v2.0.0-beta.1`.** The agent's `gh pr merge 2 --squash` found #2 already merged; its read of who merged it was refused by the permission classifier ("Merge Without Review"). The maintainer confirms that the merge of #2 was his (or reviews 0408825) and says go.
- This docs update is on branch docs-beta1-stop (pull request for the maintainer to merge).

## Decisions made this session
- The Release job dispatches verify-release.yml (`actions: write` on the gated job only). See [notes/2026-09-29-repository-variant.md](notes/2026-09-29-repository-variant.md), Phases 5 and 6.

## Dead ends hit
- The old solution cannot build here (MSB3644); do not install a targeting pack.
- WSL Ubuntu has no .NET SDK, but runs the self-contained linux-x64 build: publish it, copy it to /tmp and run it with `MSYS_NO_PATHCONV=1 wsl.exe -d Ubuntu -- bash /mnt/c/.../script.sh`.
- check-workflow-shell.py takes the repository directory, not workflow files (given files it checked nothing and exited 0). Use the skill's `scripts/lint-workflows.sh REPO`.

## Next single action (a fresh session)
After the maintainer's go: `gh run list -R m4bwav/FizzBuzzPlus -w ci -b master -L 1` must show success on 0408825 (wait if not). Then on an up-to-date master: `git tag -a v2.0.0-beta.1 -m "2.0.0-beta.1" 0408825 && git push origin v2.0.0-beta.1`, and `bash <skill>/scripts/watch-run.sh m4bwav/FizzBuzzPlus release.yml`. The run waits at the `release` environment: **the maintainer approves in the browser** (agents never call pending_deployments). The last job dispatches verify-release.yml; watch it (`watch-run.sh m4bwav/FizzBuzzPlus verify-release.yml`); its runners `ubuntu-24.04-arm` and `windows-11-arm` are unproven. Phase 6: date the CHANGELOG heading and `bump-version-dotnet.sh 2.0.0` through a pull request, then the same tag ritual. Phase 7: ask for the wiki's first page at https://github.com/m4bwav/FizzBuzzPlus/wiki/_new at the start, run wikiwright, then propose ai-docs/notes/2026-09-29-repository-variant.md to the skill as references/repository.md, update the inventory row and the kickoff's corrections section, and name the next repository. Worth writing when beta.1 is out: a `verify-github-release.sh` in the skill (Release, sums and attestations in one call, like verify-registry-npm.sh).
