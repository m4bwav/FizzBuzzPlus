# FizzBuzzPlus

![A tall glass of sparkling soda fizzing with colorful bubbles while cheerful bumblebees buzz around it, playful and bright](https://raw.githubusercontent.com/m4bwav/FizzBuzzPlus/master/.github/images/banner.jpg)

[![ci](https://github.com/m4bwav/FizzBuzzPlus/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/m4bwav/FizzBuzzPlus/actions/workflows/ci.yml)
[![Latest release](https://img.shields.io/github/v/release/m4bwav/FizzBuzzPlus?sort=semver)](https://github.com/m4bwav/FizzBuzzPlus/releases/latest)

The classic FizzBuzz problem with one addition: your own rules. It comes as a small C# library, `FizzBuzzProcessor`, and a command-line program, `fizzbuzzplus`, for .NET 10. It was written in 2014 for .NET Framework 4.5 and modernized in 2026; see [CHANGELOG.md](CHANGELOG.md) for what changed.

## The command-line program

Download the archive for your system from the [latest release](https://github.com/m4bwav/FizzBuzzPlus/releases/latest). Archives exist for Windows, Linux and macOS, each on x64 and Arm64. Each holds a single self-contained file, so .NET does not need to be installed.

```
fizzbuzzplus                   # 15 to 30, as the 2014 program did
fizzbuzzplus 9                 # one number
fizzbuzzplus 1 100             # a range, both ends included
fizzbuzzplus 1 20 --rule 2=Even --rule 7=Lucky
fizzbuzzplus --help
```

```
$ fizzbuzzplus 13 15
Current Number: 13 
Current Number: 14 
Current Number: 15 FizzBuzz
```

Any `--rule N=WORD` replaces the classic rules 3=Fizz and 5=Buzz. Numbers may be negative. The exit code is 0, or 2 with a message and the usage on standard error when the arguments are wrong.

On macOS, a file downloaded by a browser is quarantined. Clear the quarantine with `xattr -d com.apple.quarantine fizzbuzzplus` before the first run. The binaries are not signed.

## The library

Build it from source (below) and reference `src/FizzBuzzLibrary`; it is not published as a package.

```csharp
using FizzBuzzLibrary;

var fizzBuzz = new FizzBuzzProcessor(Console.Out);           // 3 Fizz, 5 Buzz
fizzBuzz.Execute(1, 15);

var rules = new Dictionary<long, string> { { 2, "Even" }, { 6, "Scary" }, { 7, "Lucky" } };
new FizzBuzzProcessor(rules, Console.Out).Execute(42);        // Current Number: 42 EvenScaryLucky
```

| Member | What it does |
|---|---|
| `FizzBuzzProcessor(IDictionary<long, string> outputBreaks, TextWriter writer)` | Custom rules: each divisor and its word. The rules are copied and sorted by divisor when the processor is created. |
| `FizzBuzzProcessor(TextWriter writer)` | The classic rules, 3 "Fizz" and 5 "Buzz". |
| `Execute()` | 1 to 100. |
| `Execute(long singleValue)` | One number. |
| `Execute(long startValue, long endValue)` | Every number from start to end, both included. |

### Behaviour at the edges

- Each line is `Current Number: `, the number, a space, the words, then a line feed (U+000A) on every OS, whatever the writer's `NewLine`. A number no rule divides keeps its trailing space: `Current Number: 7 \n`.
- Words come out in ascending order of their divisor, whatever order the dictionary has.
- Numbers are written with the invariant culture: `-15`, never a culture's own minus sign.
- A negative divisor matches the multiples of its absolute value, and -1 matches every number. A null or empty word writes nothing.
- Nothing is written when start is greater than end. A range may start at `long.MinValue` or end at `long.MaxValue`. A range of 2^64 numbers is still 2^64 lines, so choose the range with care.
- The constructors throw `ArgumentNullException` for a null dictionary or writer, and `ArgumentException` for a rule with the divisor 0. `Execute` throws only what the writer throws.
- The processor writes the prefix, each word and the line feed as separate `Write(string)` calls, and never flushes the writer.

## Build and test

```
dotnet restore --locked-mode
dotnet build -c Release
dotnet test -c Release
dotnet publish src/FizzBuzzWithOutput -c Release -r linux-x64 -o out   # or win-x64, osx-arm64, ...
```

The tests include the 7 tests written in 2014, compiled unchanged. There is also a golden replay: every answer the 2014 code gave, recorded on Windows, Linux and macOS (`tests/Golden`), must still come out the same apart from the changes listed in the changelog.

## Licence

[MIT](LICENSE).
