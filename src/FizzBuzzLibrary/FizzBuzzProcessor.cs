using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FizzBuzzLibrary
{
    /// <summary>
    /// Writes FizzBuzz lines to a <see cref="TextWriter"/>: for each number, "Current Number: " and the number, a space,
    /// then the word of every rule whose divisor divides the number, in ascending order of divisor, then a line feed.
    /// </summary>
    /// <remarks>
    /// Every line ends with a line feed ("\n", U+000A) whatever the writer's <see cref="TextWriter.NewLine"/>, and a number
    /// that no rule divides keeps its trailing space ("Current Number: 1 \n"), as in 1.0.0. Numbers are written with the
    /// invariant culture, so a negative number reads "-15" on every machine.
    /// </remarks>
    public class FizzBuzzProcessor
    {
        private const string Prefix = "Current Number: ";
        private const string LineEnd = "\n";

        private readonly KeyValuePair<long, string>[] _rules;
        private readonly TextWriter _writer;

        /// <summary>Creates a processor with custom rules.</summary>
        /// <param name="outputBreaks">
        /// The rules: each divisor and the word written for a number it divides. The rules are copied and sorted by divisor
        /// here, so later changes to the dictionary have no effect. A negative divisor matches the multiples of its absolute
        /// value. A null or empty word writes nothing.
        /// </param>
        /// <param name="writer">Where the lines are written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="outputBreaks"/> or <paramref name="writer"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="outputBreaks"/> has a rule with the divisor 0.</exception>
        public FizzBuzzProcessor(IDictionary<long, string> outputBreaks, TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(outputBreaks);
            ArgumentNullException.ThrowIfNull(writer);

            _rules = outputBreaks.OrderBy(rule => rule.Key).ToArray();
            if (_rules.Any(rule => rule.Key == 0))
            {
                throw new ArgumentException("A rule cannot have the divisor 0: no number is a multiple of 0.", nameof(outputBreaks));
            }

            _writer = writer;
        }

        /// <summary>Creates a processor with the classic rules: 3 writes "Fizz" and 5 writes "Buzz".</summary>
        /// <param name="writer">Where the lines are written.</param>
        /// <exception cref="ArgumentNullException"><paramref name="writer"/> is null.</exception>
        public FizzBuzzProcessor(TextWriter writer)
            : this(new Dictionary<long, string> { { 3, "Fizz" }, { 5, "Buzz" } }, writer)
        {
        }

        /// <summary>Writes the lines for 1 to 100.</summary>
        public void Execute()
        {
            Execute(1, 100);
        }

        /// <summary>Writes the line for one number.</summary>
        /// <param name="singleValue">The number.</param>
        public void Execute(long singleValue)
        {
            Execute(singleValue, singleValue);
        }

        /// <summary>
        /// Writes the lines for every number from <paramref name="startValue"/> to <paramref name="endValue"/>, both included.
        /// Writes nothing when <paramref name="startValue"/> is greater than <paramref name="endValue"/>.
        /// </summary>
        /// <param name="startValue">The first number.</param>
        /// <param name="endValue">The last number; the range may end at <see cref="long.MaxValue"/>.</param>
        public void Execute(long startValue, long endValue)
        {
            if (startValue > endValue)
            {
                return;
            }

            // Stop on the last number instead of testing number <= endValue: at long.MaxValue that test is always true,
            // and the counter wrapped around to long.MinValue in 1.0.0.
            for (var number = startValue; ; number++)
            {
                WriteLine(number);
                if (number == endValue)
                {
                    return;
                }
            }
        }

        private void WriteLine(long number)
        {
            _writer.Write(Prefix + number.ToString(CultureInfo.InvariantCulture) + " ");
            foreach (var rule in _rules)
            {
                if (IsMultiple(number, rule.Key))
                {
                    _writer.Write(rule.Value);
                }
            }

            _writer.Write(LineEnd);
        }

        // Every number is a multiple of -1, but long.MinValue % -1 overflows, so -1 is answered without dividing.
        private static bool IsMultiple(long number, long divisor)
        {
            return divisor == -1 || number % divisor == 0;
        }
    }
}
