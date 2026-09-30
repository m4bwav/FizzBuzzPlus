extern alias Original2014;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OldProcessor = Original2014::FizzBuzzLibrary.FizzBuzzProcessor;

namespace FizzBuzzLibrary.Tests
{
    /// <summary>
    /// The new FizzBuzzProcessor against the frozen 2014 one on seeded random rules and ranges, outside the inputs the plan
    /// changed on purpose (E1 to E6): no rule of 0, no rule of -1 with long.MinValue, no range ending at long.MaxValue, the
    /// rules not changed after construction, the invariant culture. Every output must be the same text (skill L-052).
    /// </summary>
    [TestClass]
    public sealed class DifferentialTests
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(20260929)]
        [DataRow(-7)]
        public void Random_rules_and_ranges_answer_as_the_2014_code(int seed)
        {
            var saved = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                var random = new Random(seed);
                for (var i = 0; i < 2000; i++)
                {
                    var rules = RandomRules(random);
                    var (start, end) = RandomRange(random);
                    Assert.AreEqual(Old(rules, start, end), New(rules, start, end), "seed " + seed + ", case " + i + ", range " + start + " to " + end + ", rules " + Describe(rules));
                }
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = saved;
            }
        }

        [TestMethod]
        public void The_default_rules_answer_as_the_2014_code()
        {
            using var oldWriter = new StringWriter(CultureInfo.InvariantCulture);
            using var newWriter = new StringWriter(CultureInfo.InvariantCulture);
            new OldProcessor(oldWriter).Execute(-500, 500);
            new FizzBuzzProcessor(newWriter).Execute(-500, 500);
            Assert.AreEqual(oldWriter.ToString(), newWriter.ToString());
        }

        private static Dictionary<long, string> RandomRules(Random random)
        {
            var rules = new Dictionary<long, string>();
            var count = random.Next(0, 5);
            string[] words = { "Fizz", "Buzz", "Even", string.Empty, "Lucky", "é", "A B" };
            while (rules.Count < count)
            {
                long divisor = random.Next(0, 10) == 0 ? random.NextInt64(long.MinValue, long.MaxValue) : random.Next(-30, 31);
                if (divisor == 0 || divisor == -1)
                {
                    continue;
                }

                rules[divisor] = random.Next(0, 12) == 0 ? null! : words[random.Next(words.Length)];
            }

            return rules;
        }

        private static (long Start, long End) RandomRange(Random random)
        {
            long start = random.Next(0, 4) switch
            {
                0 => random.Next(-100, 101),
                1 => random.NextInt64(long.MinValue, long.MaxValue - 100),
                2 => long.MinValue + random.Next(0, 50),
                _ => long.MaxValue - 100 - random.Next(0, 50),
            };
            // A negative length near long.MinValue would wrap round to a range of 2^64 numbers: keep those lengths positive.
            var length = random.Next(start < long.MinValue + 100 ? 0 : -2, 40);
            return (start, start + length);
        }

        private static string Old(Dictionary<long, string> rules, long start, long end)
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            new OldProcessor(new Dictionary<long, string>(rules), writer).Execute(start, end);
            return writer.ToString();
        }

        private static string New(Dictionary<long, string> rules, long start, long end)
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            new FizzBuzzProcessor(new Dictionary<long, string>(rules), writer).Execute(start, end);
            return writer.ToString();
        }

        private static string Describe(Dictionary<long, string> rules)
        {
            var parts = new List<string>();
            foreach (var rule in rules)
            {
                parts.Add(rule.Key.ToString(CultureInfo.InvariantCulture) + "=" + (rule.Value ?? "null"));
            }

            return string.Join(",", parts);
        }
    }
}
