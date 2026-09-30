using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FizzBuzzLibrary;
using Microsoft.Win32.SafeHandles;

namespace FizzBuzzWithOutput
{
    /// <summary>The fizzbuzzplus command: FizzBuzz lines for a number or a range, with the classic rules or custom ones.</summary>
    internal static class Program
    {
        internal const int Ok = 0;
        internal const int WriteError = 1;
        internal const int UsageError = 2;

        internal static readonly string[] Usage =
        {
            "Usage: fizzbuzzplus [START [END]] [--rule N=WORD]...",
            "",
            "Writes one line per number: \"Current Number: \", the number, a space, then the word of every rule",
            "whose divisor divides it, in ascending order of divisor.",
            "",
            "  (no numbers)     15 to 30, as the 2014 app did",
            "  START            that one number",
            "  START END        every number from START to END",
            "  --rule N=WORD    a rule: WORD for the multiples of N (not 0); repeat for more rules.",
            "                   Any --rule replaces the classic rules 3=Fizz and 5=Buzz.",
            "  -h, --help       this text",
            "",
            "Example: fizzbuzzplus 1 20 --rule 2=Even --rule 7=Lucky",
        };

        private static int Main(string[] args)
        {
            // Messages are collected first and written at the end, so a failing standard error cannot change the exit code.
            using var errors = new StringWriter(CultureInfo.InvariantCulture);
            int code;
            try
            {
                using var stdout = new StreamWriter(OpenStandardOutput(), new UTF8Encoding(false), 65536);
                code = Run(args, stdout, errors);
                stdout.Flush();
            }
            catch (IOException e) when (IsBrokenPipe(e))
            {
                // The reader went away (fizzbuzzplus 1 1000000 | head): stop quietly, as other command-line tools do.
                code = Ok;
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
            {
                // A full disk, a closed standard output: the lines are lost, so say so and fail.
                errors.WriteLine("fizzbuzzplus: cannot write the output: " + e.Message);
                code = WriteError;
            }

            try
            {
                Console.Error.Write(errors.ToString());
                Console.Error.Flush();
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException)
            {
                // Nowhere left to report it; the exit code still tells.
            }

            return code;
        }

        // The stream Console.OpenStandardOutput() returns ignores a closed pipe (EPIPE on Unix, ERROR_NO_DATA and
        // ERROR_BROKEN_PIPE on Windows), so `fizzbuzzplus 1 9223372036854775807 | head -1` would never end. A FileStream on
        // the standard output handle reports it as an IOException instead.
        private static FileStream OpenStandardOutput()
        {
            var handle = OperatingSystem.IsWindows() ? NativeMethods.GetStdHandle(NativeMethods.StdOutputHandle) : 1;
            return new FileStream(new SafeFileHandle(handle, ownsHandle: false), FileAccess.Write, 1);
        }

        private static bool IsBrokenPipe(IOException e)
        {
            var code = e.HResult & 0xFFFF;
            return OperatingSystem.IsWindows()
                ? code is 109 or 232 // ERROR_BROKEN_PIPE, ERROR_NO_DATA
                : code == 32; // EPIPE
        }

        /// <summary>Parses the arguments and writes the lines; returns the exit code.</summary>
        internal static int Run(IReadOnlyList<string> args, TextWriter output, TextWriter error)
        {
            var numbers = new List<long>();
            var rules = new Dictionary<long, string>();
            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if (arg is "-h" or "--help")
                {
                    WriteUsage(output);
                    return Ok;
                }

                if (arg == "--rule")
                {
                    if (i + 1 >= args.Count)
                    {
                        return Fail(error, "--rule needs a value such as 3=Fizz.");
                    }

                    arg = "--rule=" + args[++i];
                }

                if (arg.StartsWith("--rule=", StringComparison.Ordinal))
                {
                    var value = arg.Substring("--rule=".Length);
                    var eq = value.IndexOf('=', StringComparison.Ordinal);
                    if (eq <= 0 || !TryParse(value.Substring(0, eq), out var divisor))
                    {
                        return Fail(error, "--rule " + value + ": expected N=WORD, where N is a whole number.");
                    }

                    if (divisor == 0)
                    {
                        return Fail(error, "--rule " + value + ": the divisor cannot be 0.");
                    }

                    if (!rules.TryAdd(divisor, value.Substring(eq + 1)))
                    {
                        return Fail(error, "--rule " + value + ": the divisor " + divisor.ToString(CultureInfo.InvariantCulture) + " is given twice.");
                    }

                    continue;
                }

                if (!TryParse(arg, out var number))
                {
                    return Fail(error, "'" + arg + "' is not a whole number or an option.");
                }

                numbers.Add(number);
            }

            if (numbers.Count > 2)
            {
                return Fail(error, "at most two numbers, START and END.");
            }

            var processor = rules.Count == 0 ? new FizzBuzzProcessor(output) : new FizzBuzzProcessor(rules, output);
            switch (numbers.Count)
            {
                case 0:
                    processor.Execute(15, 30);
                    break;
                case 1:
                    processor.Execute(numbers[0]);
                    break;
                default:
                    processor.Execute(numbers[0], numbers[1]);
                    break;
            }

            return Ok;
        }

        private static bool TryParse(string text, out long value)
        {
            return long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
        }

        private static int Fail(TextWriter error, string message)
        {
            error.WriteLine("fizzbuzzplus: " + message);
            WriteUsage(error);
            return UsageError;
        }

        private static void WriteUsage(TextWriter writer)
        {
            foreach (var line in Usage)
            {
                writer.WriteLine(line);
            }
        }
    }
}
