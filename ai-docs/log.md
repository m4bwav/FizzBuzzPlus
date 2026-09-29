# Log

Append-only. One line per operation: `## [YYYY-MM-DD] op | title` where op is one of add, update, supersede, verify, verify-failed, prune, handoff, index. Newest at the bottom. Never edited, only appended; this is the history the entries themselves do not carry.

## [2026-09-29] init | scaffolded

## [2026-09-29] add | Phase 0 started: package-modernize, repository variant (kickoff package-modernization/prompts/2026-09-29-fizzbuzzplus-kickoff.md)
- Cloned m4bwav/FizzBuzzPlus (master d13d130, 6 commits, last push 2014-04-12) to labs/FizzBuzzPlus; branch v2 for all run work. everlast registered: mode repo, sync push.
- Survey: `survey-github.sh m4bwav/FizzBuzzPlus` saved as notes/2026-09-29-survey.txt. No issues, pull requests, forks, releases, tags, webhooks (`gh api .../hooks` returned `[]`, exit 0), secrets, variables, environments, workflows or rulesets; Dependabot alerts disabled (403), secret scanning and push protection off; default workflow permissions `write` with `can_approve_pull_request_reviews: true`; wiki on but `FizzBuzzPlus.wiki.git` not found; no licence. nuget.org: no package FizzBuzzPlus (search totalHits 0; flat container 404).
- Baseline, the solution as it is: `dotnet build FizzBuzzPlus.sln` on SDK 10.0.401 fails with MSB3644 in all three projects (no .NET Framework 4.5 reference assemblies; no Visual Studio on this machine).
- Baseline, the 2014 tests unchanged (skill L-079): a scratch SDK project on net48 and net10.0 with MSTest 4.4.1 compiling FizzBuzzProcessor.cs and both test files by link: 7 of 7 passed on each runtime; MSTEST0017 flags all 7 assertions as swapped (actual, expected).
- Frozen the 2014 sources byte for byte in tests/Golden/Original (git blob ids equal to d13d130's; table in its README).
- Golden capture tests/Golden/Capture (compiles Original/FizzBuzzProcessor.cs unchanged) and OriginalApp (the 2014 console app, rebuilt unchanged), run by tests/Golden/capture.sh: 108 cases on net48 (.NET Framework 4.8.9345.0, 64-bit) and on net10.0 (Windows). Two runs byte-identical on each runtime (sha256 8e4976d9130b910f... 49493 bytes, faaf35a9165c06a0... 49609 bytes); no CR bytes; ASCII only.
- net48 and net10.0 differ in 10 of 108 cases: the ArgumentNullException message wording and source assembly (System.Core against System.Linq), ObjectDisposedException's source assembly, the unhandled-exception line of the console app, and five cultures (sv-SE, nb-NO, he-IL, fa-IR, ar-SA) in which .NET 10 writes the minus sign as U+2212, or with U+200E or U+061C in front, where .NET Framework writes "-".
- The action pins in the throwaway golden-capture workflow were first written from memory (checkout v6, setup-dotnet v4, upload-artifact v4); replaced by the template's SHAs (checkout v7.0.1, setup-dotnet v6.0.0, upload-artifact v7.0.1) before any commit.

## [2026-09-29] add | Phase 0 finished: Linux and macOS recordings, architecture header, the Phase 0 commit
- `watch-run.sh` was first called with its arguments swapped (run id first); it exited 1 ("no run ... started"), but the call was piped to `tail`, which reported 0: the skill's L-129 `pipe-hides-status` repeated. Rerun without a pipe: exit 0.
- golden-capture run 36645300631 (scratch branch golden-capture, ubuntu-latest and macos-latest, capture twice and `cmp`): success. Linux matched the Windows net10.0 recording in 106 of 108 cases (the console app exits 134, SIGABRT, instead of -532462766); macOS differed from Linux in the four arithmetic-exception cases, whose source was System.Private.CoreLib instead of the library: macos-latest is arm64, where the runtime raises DivideByZeroException and OverflowException for `long %` from a helper.
- So the capture header now records the process architecture (a7e17ed; Windows recaptured twice, identical, 108 of 108 cases unchanged apart from the header). golden-capture rebased and pushed again: run 36645600812 success; downloaded recordings hash to the sums the runners logged (linux cb92ab2c..., macos c3e411b0...), architecture x64 and arm64, 108 of 108 cases equal to the first run.
- **The Phase 0 commit is 2835ec0** (Linux x64 and macOS arm64 recordings). From here on `git diff --exit-code 2835ec0 -- tests/Golden` must stay empty.
- check-readme-images.mjs README.md --registry nuget: 0 images, exit 0. History grep for credentials: only assembly PublicKeyToken values.

## [2026-09-29] add | Phase 1: plan, decision record, repository variant draft
- plans/2026-09-29-modernization-and-v2-release.md: D0 to D16, named exceptions E1 to E7 (28 of 108 net10.0 cases change on Windows: E1 2, E2 5, E3 3, E4 2, E5 9, E6 5, E7 2; plus the runtime rule E8), a GitHub Release instead of a registry (D13), MSTest 4 so the 2014 tests compile unchanged (D9).
- decisions/2026-09-29-modernize-as-2.0.0-with-a-github-release.md (status proposed); notes/2026-09-29-repository-variant.md (the draft of the skill's references/repository.md).
- Stop: the plan review.
## [2026-09-29] index | rebuilt (4 entries)
## [2026-09-29] index | rebuilt (4 entries)
