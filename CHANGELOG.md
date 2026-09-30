# Changelog

All notable changes to FizzBuzzPlus. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); versions follow [Semantic Versioning](https://semver.org/).

## [2.0.0] - Unreleased

**The promise:** every answer the 2014 code (1.0.0) gave stays exactly the same, byte for byte, except for the changes listed below. A recording of 108 cases, taken from the unchanged 2014 source on Windows, Linux and macOS, checks this on every build. The exceptions: a range ending at `long.MaxValue` now ends; numbers use the invariant culture; a rule of 0 and null arguments are refused when the processor is created; a rule of -1 no longer overflows; the rules are copied when the processor is created; and the program no longer waits for a key, and now takes arguments.

### Changed
- Runs on .NET 10 (was .NET Framework 4.5). The Visual Studio 2012 solution became SDK-style projects under `src/` and `tests/`, with `FizzBuzzPlus.slnx`.
- Numbers are written with the invariant culture. On .NET 5 and later, the 2014 code wrote the minus sign as U+2212 in sv-SE and nb-NO, and with a direction mark in front of it in he-IL, fa-IR and ar-SA. .NET Framework wrote "-" in every culture, and 2.0.0 does too.
- The rules are copied and sorted once, when the processor is created. In 1.0.0 the processor kept the caller's dictionary, so adding, removing or clearing rules later changed its output, and it sorted the rules again for every number.
- The constructors check their arguments. `ArgumentNullException` is thrown for a null dictionary or writer, and `ArgumentException` for a rule with the divisor 0. In 1.0.0 these failed only inside `Execute`, after "Current Number: 1 " had been written, with a NullReferenceException, an ArgumentNullException from LINQ or a DivideByZeroException.
- The console program is now `fizzbuzzplus`. With no arguments it still writes 15 to 30. It takes one number or a range, `--rule N=WORD` and `--help`, and it no longer calls `Console.ReadKey()`. That call threw InvalidOperationException whenever input was redirected, so the program failed in every pipeline and CI run.

### Fixed
- `Execute(long.MaxValue)` and any range ending at `long.MaxValue` no longer loop forever. The counter used to wrap to `long.MinValue`.
- A rule of -1 no longer throws OverflowException for `long.MinValue`: every number is a multiple of -1.

### Added
- Releases on GitHub: the program as one self-contained file for Windows, Linux and macOS on x64 and Arm64, with SHA-256 sums and build provenance attestations.
- Tests: the 7 tests of 2014, compiled unchanged; unit tests; a differential test against the 2014 code; the golden replay; CI on Linux, Windows and macOS.
- LICENSE (MIT), SECURITY.md.

### Kept on purpose
- Every line ends with a line feed ("\n") on every OS, whatever the writer's `NewLine`, and a number with no word keeps its trailing space. The 2014 tests assert both.
- Start greater than end writes nothing.

## [1.0.0] - 2014-04-12

The original code (tag `v1.0.0`, never released): `FizzBuzzProcessor` with the classic and custom rules, the FizzBuzzWithOutput console program and 7 MSTest tests, for .NET Framework 4.5 and Visual Studio 2012.

[2.0.0]: https://github.com/m4bwav/FizzBuzzPlus/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/m4bwav/FizzBuzzPlus/releases/tag/v1.0.0
