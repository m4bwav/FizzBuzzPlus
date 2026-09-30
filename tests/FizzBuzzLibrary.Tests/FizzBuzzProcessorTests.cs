using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FizzBuzzLibrary.Tests
{
    [TestClass]
    public sealed class FizzBuzzProcessorTests
    {
        private static readonly string[] WritesFor15 = { "Current Number: 15 ", "Fizz", "Buzz", "\n" };

        private static string Run(Action<FizzBuzzProcessor> act, IDictionary<long, string>? rules = null)
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            var processor = rules == null ? new FizzBuzzProcessor(writer) : new FizzBuzzProcessor(rules, writer);
            act(processor);
            return writer.ToString();
        }

        [TestMethod]
        public void Default_rules_write_Fizz_Buzz_and_FizzBuzz()
        {
            Assert.AreEqual("Current Number: 15 FizzBuzz\n", Run(p => p.Execute(15)));
            Assert.AreEqual("Current Number: 18 Fizz\n", Run(p => p.Execute(18)));
            Assert.AreEqual("Current Number: 20 Buzz\n", Run(p => p.Execute(20)));
        }

        [TestMethod]
        public void A_number_with_no_word_keeps_its_trailing_space()
        {
            Assert.AreEqual("Current Number: 7 \n", Run(p => p.Execute(7)));
        }

        [TestMethod]
        public void Execute_without_arguments_writes_1_to_100()
        {
            var lines = Run(p => p.Execute()).Split('\n');
            Assert.HasCount(101, lines);
            Assert.AreEqual("Current Number: 1 ", lines[0]);
            Assert.AreEqual("Current Number: 100 Buzz", lines[99]);
            Assert.AreEqual(string.Empty, lines[100]);
        }

        [TestMethod]
        public void Custom_rules_are_written_in_ascending_order_of_divisor()
        {
            var rules = new Dictionary<long, string> { { 7, "Lucky" }, { 6, "Scary" }, { 2, "Even" } };
            Assert.AreEqual("Current Number: 42 EvenScaryLucky\n", Run(p => p.Execute(42), rules));
        }

        [TestMethod]
        public void A_descending_dictionary_is_still_written_in_ascending_order()
        {
            var rules = new SortedDictionary<long, string>(Comparer<long>.Create((a, b) => b.CompareTo(a))) { { 3, "Fizz" }, { 5, "Buzz" } };
            Assert.AreEqual("Current Number: 15 FizzBuzz\n", Run(p => p.Execute(15), rules));
        }

        [TestMethod]
        public void Every_line_ends_with_a_line_feed_whatever_the_writers_NewLine()
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture) { NewLine = "\r\n" };
            new FizzBuzzProcessor(writer).Execute(1, 2);
            Assert.AreEqual("Current Number: 1 \nCurrent Number: 2 \n", writer.ToString());
        }

        [TestMethod]
        public void Start_after_end_writes_nothing()
        {
            Assert.AreEqual(string.Empty, Run(p => p.Execute(10, 1)));
            Assert.AreEqual(string.Empty, Run(p => p.Execute(long.MaxValue, long.MinValue)));
        }

        [TestMethod]
        public void A_null_or_empty_word_writes_nothing()
        {
            var rules = new Dictionary<long, string> { { 3, null! }, { 5, string.Empty } };
            Assert.AreEqual("Current Number: 15 \n", Run(p => p.Execute(15), rules));
        }

        [TestMethod]
        public void E1_a_range_that_ends_at_long_MaxValue_stops_there()
        {
            Assert.AreEqual("Current Number: 9223372036854775807 \n", Run(p => p.Execute(long.MaxValue)));
            var lines = Run(p => p.Execute(long.MaxValue - 1, long.MaxValue)).Split('\n');
            CollectionAssert.AreEqual(new[] { "Current Number: 9223372036854775806 Fizz", "Current Number: 9223372036854775807 ", string.Empty }, lines);
        }

        [TestMethod]
        public void E1_a_range_that_starts_at_long_MinValue_works()
        {
            Assert.AreEqual("Current Number: -9223372036854775808 \nCurrent Number: -9223372036854775807 \n", Run(p => p.Execute(long.MinValue, long.MinValue + 1)));
        }

        [TestMethod]
        [DataRow("sv-SE")]
        [DataRow("nb-NO")]
        [DataRow("he-IL")]
        [DataRow("fa-IR")]
        [DataRow("ar-SA")]
        [DataRow("en-US")]
        public void E2_negative_numbers_use_the_ASCII_minus_in_every_culture(string culture)
        {
            var saved = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                Assert.AreEqual("Current Number: -15 FizzBuzz\n", Run(p => p.Execute(-15)));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = saved;
            }
        }

        [TestMethod]
        public void E3_a_rule_of_zero_is_refused_before_any_output()
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            var e = Assert.ThrowsExactly<ArgumentException>(() => new FizzBuzzProcessor(new Dictionary<long, string> { { 3, "Fizz" }, { 0, "Zero" } }, writer));
            Assert.AreEqual("outputBreaks", e.ParamName);
            Assert.AreEqual(string.Empty, writer.ToString());
        }

        [TestMethod]
        public void E4_every_number_is_a_multiple_of_minus_one()
        {
            var rules = new Dictionary<long, string> { { -1, "MinusOne" } };
            Assert.AreEqual("Current Number: -9223372036854775808 MinusOne\n", Run(p => p.Execute(long.MinValue), rules));
            Assert.AreEqual("Current Number: 7 MinusOne\n", Run(p => p.Execute(7), rules));
        }

        [TestMethod]
        public void Negative_divisors_match_the_multiples_of_their_absolute_value()
        {
            var rules = new Dictionary<long, string> { { -3, "Neg" }, { 3, "Pos" } };
            Assert.AreEqual("Current Number: -6 NegPos\nCurrent Number: -5 \n", Run(p => p.Execute(-6, -5), rules));
        }

        [TestMethod]
        public void E5_null_arguments_are_refused_by_the_constructors()
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            Assert.AreEqual("outputBreaks", Assert.ThrowsExactly<ArgumentNullException>(() => new FizzBuzzProcessor(null!, writer)).ParamName);
            Assert.AreEqual("writer", Assert.ThrowsExactly<ArgumentNullException>(() => new FizzBuzzProcessor(new Dictionary<long, string>(), null!)).ParamName);
            Assert.AreEqual("writer", Assert.ThrowsExactly<ArgumentNullException>(() => new FizzBuzzProcessor(null!)).ParamName);
            Assert.AreEqual("outputBreaks", Assert.ThrowsExactly<ArgumentNullException>(() => new FizzBuzzProcessor(null!, null!)).ParamName);
        }

        [TestMethod]
        public void E6_the_rules_are_copied_when_the_processor_is_created()
        {
            var rules = new Dictionary<long, string> { { 3, "Fizz" } };
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            var processor = new FizzBuzzProcessor(rules, writer);
            rules.Add(5, "Buzz");
            rules.Remove(3);
            processor.Execute(15);
            Assert.AreEqual("Current Number: 15 Fizz\n", writer.ToString());
        }

        [TestMethod]
        public void The_writer_sees_the_prefix_each_word_and_the_line_feed_as_separate_writes()
        {
            var writer = new CallLog();
            new FizzBuzzProcessor(writer).Execute(15);
            CollectionAssert.AreEqual(WritesFor15, writer.Writes);
            Assert.AreEqual(0, writer.Flushes);
        }

        [TestMethod]
        public void A_range_of_a_million_numbers_ends_with_the_right_line()
        {
            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            new FizzBuzzProcessor(writer).Execute(1, 1_000_000);
            var text = writer.ToString();
            Assert.AreEqual(1_000_000, text.Count(c => c == '\n'));
            Assert.EndsWith("Current Number: 1000000 Buzz\n", text);
        }

        private sealed class CallLog : StringWriter
        {
            public CallLog()
                : base(CultureInfo.InvariantCulture)
            {
            }

            public List<string> Writes { get; } = new();

            public int Flushes { get; private set; }

            public override void Write(string? value)
            {
                Writes.Add(value ?? "(null)");
                base.Write(value);
            }

            public override void Flush()
            {
                Flushes++;
                base.Flush();
            }
        }
    }
}
