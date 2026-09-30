---
title: Phase 0 findings for FizzBuzzPlus (the 2014 source)
kind: note
date: 2026-09-29
verified: 2026-09-29
stale_after: never
tags: [survey, phase-0, golden, repository-variant, dotnet]
summary: "what the 2014 repository holds, what the survey and the golden capture proved (claims confirmed and refuted), the runtime and culture differences, and the baseline; read before re-surveying or planning"
---

# Phase 0 findings: FizzBuzzPlus (2014 source, commit d13d130)

## Summary

The 2014 solution no longer builds, but its 7 tests pass unchanged against the unchanged source on net48 and net10.0. The golden capture of the frozen source (108 cases, four recordings) confirms every bug the kickoff listed. It adds two the kickoff missed: `Execute(long.MaxValue)` never ends, and a rule of -1 overflows at `long.MinValue`. It also shows that the same source prints a culture-dependent minus sign on .NET 10.

Evidence: [2026-09-29-survey.txt](2026-09-29-survey.txt) (GitHub side), `tests/Golden/1.0.0.*.json` (the recordings), [../log.md](../log.md) (commands and results).

## What is there

- A Visual Studio 2012 solution with three old-style projects (ToolsVersion 4.0, .NET Framework 4.5, AssemblyInfo.cs declaring version 1.0.0.0, copyright 2014, no company). `FizzBuzzLibrary` holds one public class, `FizzBuzzProcessor`. `FizzBuzzWithOutput` is a console app that runs `Execute(15, 30)` on `Console.Out`, then `Console.ReadKey()`. `FizzBuzzTest` has 7 MSTest v1 tests, and its csproj references the Coded UI assemblies for Visual Studio 2010.
- Every source file starts with a UTF-8 byte order mark and uses LF line endings. The `.gitignore` lists 44 build outputs one file at a time.
- Nothing from NuGet. There are no issues, pull requests, forks, tags, releases, webhooks, secrets, workflows or rulesets, and no licence. Dependabot alerts, secret scanning and push protection are off. Default workflow permissions are `write` (and Actions may approve pull requests). The wiki is switched on, but it has no repository. The NuGet id FizzBuzzPlus is free.

## Baseline

- `dotnet build FizzBuzzPlus.sln` (SDK 10.0.401): MSB3644 in all three projects. This machine has no .NET Framework 4.5 reference assemblies and no Visual Studio.
- The 7 tests of 2014, unchanged, compiled by link with the unchanged library source in a scratch SDK project (MSTest 4.4.1): 7 of 7 pass on net48 and on net10.0. MSTest's analyzer MSTEST0017 flags all 7 assertions as swapped.

## The golden capture

The repository never published a package, so the reference is the source itself. The four 2014 files are frozen byte for byte in `tests/Golden/Original`, with git blob ids equal to commit d13d130's. `tests/Golden/Capture` compiles the library file unchanged, and `tests/Golden/OriginalApp` rebuilds the console app unchanged. `tests/Golden/capture.sh` checks the blob ids and records 108 cases per runtime: the API surface, the default and custom rules, single numbers and ranges, the edges of `long`, rule shapes (0, 1, negative, `long.MinValue`, `long.MaxValue`, null and empty words, control characters, surrogates, sorted and read-only dictionaries), null arguments, the caller's dictionary changed at four moments, the writer's calls, a failing and a disposed writer, eight cultures, two ranges of 100 000, and the console app with redirected input. A writer guard stops output that never ends, after a fixed number of lines, and the recording says so. Two runs are byte-identical on net48 and on net10.0 (Windows). Linux and macOS come from a throwaway workflow on the scratch branch `golden-capture`.

## The kickoff's claims, checked against the recording

| Claim | Result |
|---|---|
| `Execute(n, long.MaxValue)` never ends: the counter wraps to `long.MinValue` | Confirmed (edge cases). **Also true of `Execute(long.MaxValue)`**, the one-argument form, which the kickoff did not list. |
| A divisor of 0 throws DivideByZeroException in the middle of the output | Confirmed: "Current Number: 1 " is written first. With rules 0 and 3, 0 sorts first, so Fizz is never reached. With an empty range, nothing is thrown. |
| A null dictionary throws ArgumentNullException from OrderBy after "Current Number: 1 " | Confirmed (parameter `source`; from System.Core on net48, System.Linq on net10.0). With an empty range, nothing is thrown. |
| A null writer throws NullReferenceException only when Execute runs | Confirmed. Construction succeeds; an empty range throws nothing. |
| Every line ends with a hard-coded "\n"; a number with no word keeps a trailing space | Confirmed. The writer's `NewLine` (set to CR LF) is ignored. |
| The rules are sorted again for every number | Confirmed by reading; it is not observable in the output. |
| The caller's dictionary is kept by reference | Confirmed at four moments: a rule added after construction is used, one removed between calls is gone, clearing empties the output, and a rule the writer adds after the first line applies from the second number on. |
| startValue > endValue writes nothing, silently | Confirmed, including `Execute(long.MaxValue, long.MinValue)`. |
| The console app's Console.ReadKey() throws when input is redirected | Confirmed. The 16 lines for 15 to 30 are printed first, then InvalidOperationException, exit code -532462766 (0xE0434352) on Windows. Arguments are ignored. |
| The tests call Assert.AreEqual(actual, expected) | Confirmed by MSTEST0017 on all 7. |

## Found by the capture, not in the kickoff

1. **A rule of -1 throws OverflowException at `long.MinValue`.** `long.MinValue % -1` overflows in .NET, after "Current Number: -9223372036854775808 " has been written.
2. **The same source prints differently on .NET 10.** `"Current Number: " + n` formats `n` with the current culture. On .NET Framework every culture recorded writes "-". On .NET 10 (ICU), sv-SE and nb-NO write U+2212 MINUS SIGN; he-IL writes U+200E LEFT-TO-RIGHT MARK then "-"; fa-IR writes U+200E then U+2212; ar-SA writes U+061C ARABIC LETTER MARK then "-". A negative number's line therefore depends on the machine's culture and runtime.
3. **Write granularity.** The library calls `TextWriter.Write(string)` three or more times per line (the prefix, each word, the "\n"), passes `null` through for a null word, and never flushes.
4. **A rule added by the writer while words are being written** takes effect from the next number and throws nothing, because `OrderBy` has already buffered the dictionary.
5. **Sorting ignores the dictionary's own order and comparer.** A SortedDictionary with a descending comparer still yields Fizz before Buzz.
6. **Text in a word goes out as it is.** LF, CR LF, surrogate pairs and a lone surrogate are written unchanged. A null word and an empty word both give "Current Number: 3 \n".

## Differences between net48 and net10.0 (same source)

10 of 108 cases differ: the ArgumentNullException message ("Value cannot be null.\r\nParameter name: source" against "Value cannot be null. (Parameter 'source')") and its source assembly, ObjectDisposedException's source assembly, the console app's unhandled-exception line, and the five cultures above. The other 98 cases are identical.

Related: see also [2026-09-29-survey.txt](2026-09-29-survey.txt), [../../tests/Golden/Original/README.md](../../tests/Golden/Original/README.md).
