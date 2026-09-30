using System;
using System.Globalization;
using System.IO;
using FizzBuzzWithOutput;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FizzBuzzLibrary.Tests
{
    [TestClass]
    public sealed class AppTests
    {
        private static (int Code, string Output, string Error) Run(params string[] args)
        {
            using var output = new StringWriter(CultureInfo.InvariantCulture);
            using var error = new StringWriter(CultureInfo.InvariantCulture);
            var code = Program.Run(args, output, error);
            return (code, output.ToString(), error.ToString());
        }

        [TestMethod]
        public void No_arguments_writes_15_to_30_as_the_2014_app_did()
        {
            var (code, output, error) = Run();
            Assert.AreEqual(0, code);
            Assert.StartsWith("Current Number: 15 FizzBuzz\nCurrent Number: 16 \n", output);
            Assert.EndsWith("Current Number: 30 FizzBuzz\n", output);
            Assert.AreEqual(string.Empty, error);
        }

        [TestMethod]
        public void One_number_writes_that_number()
        {
            Assert.AreEqual((0, "Current Number: 9 Fizz\n", string.Empty), Run("9"));
        }

        [TestMethod]
        public void Two_numbers_write_the_range_negative_numbers_included()
        {
            Assert.AreEqual((0, "Current Number: -1 \nCurrent Number: 0 FizzBuzz\nCurrent Number: 1 \n", string.Empty), Run("-1", "1"));
        }

        [TestMethod]
        public void Rules_replace_the_classic_rules_in_both_spellings()
        {
            Assert.AreEqual((0, "Current Number: 14 EvenLucky\n", string.Empty), Run("14", "--rule", "2=Even", "--rule=7=Lucky"));
        }

        [TestMethod]
        public void A_rule_may_have_an_empty_word_or_an_equals_sign_in_it()
        {
            Assert.AreEqual((0, "Current Number: 4 a=b\n", string.Empty), Run("4", "--rule", "2=", "--rule", "4=a=b"));
        }

        [TestMethod]
        public void Help_goes_to_standard_output_with_exit_code_0()
        {
            var (code, output, error) = Run("--help");
            Assert.AreEqual(0, code);
            Assert.StartsWith("Usage: fizzbuzzplus", output);
            Assert.AreEqual(string.Empty, error);
            Assert.AreEqual(output, Run("-h").Output);
        }

        [TestMethod]
        [DataRow(new[] { "1", "2", "3" }, "at most two numbers")]
        [DataRow(new[] { "x" }, "'x' is not a whole number or an option.")]
        [DataRow(new[] { "1.5" }, "'1.5' is not a whole number")]
        [DataRow(new[] { "99999999999999999999" }, "is not a whole number")]
        [DataRow(new[] { "--rule" }, "--rule needs a value")]
        [DataRow(new[] { "--rule", "Fizz" }, "expected N=WORD")]
        [DataRow(new[] { "--rule", "=Fizz" }, "expected N=WORD")]
        [DataRow(new[] { "--rule", "0=Zero" }, "the divisor cannot be 0")]
        [DataRow(new[] { "--rule", "3=Fizz", "--rule", "3=Again" }, "the divisor 3 is given twice")]
        [DataRow(new[] { "--verbose" }, "'--verbose' is not a whole number or an option.")]
        public void Bad_arguments_exit_2_with_a_message_and_the_usage_on_standard_error(string[] args, string message)
        {
            var (code, output, error) = Run(args);
            Assert.AreEqual(2, code);
            Assert.AreEqual(string.Empty, output);
            Assert.StartsWith("fizzbuzzplus: ", error);
            Assert.Contains(message, error);
            Assert.Contains("Usage: fizzbuzzplus", error);
        }

        [TestMethod]
        public void Start_after_end_writes_nothing_and_succeeds()
        {
            Assert.AreEqual((0, string.Empty, string.Empty), Run("5", "1"));
        }

        [TestMethod]
        public void Numbers_are_parsed_with_the_invariant_culture()
        {
            Assert.AreEqual(2, Run("1,000").Code);
            Assert.AreEqual(0, Run("+3").Code);
        }

        [TestMethod]
        public void The_usage_text_has_no_line_longer_than_110_characters()
        {
            foreach (var line in Program.Usage)
            {
                Assert.IsLessThanOrEqualTo(110, line.Length, line);
            }
        }

        [TestMethod]
        public void Main_is_the_entry_point_of_the_fizzbuzzplus_assembly()
        {
            Assert.AreEqual("fizzbuzzplus", typeof(Program).Assembly.GetName().Name);
            Assert.IsNotNull(typeof(Program).Assembly.EntryPoint);
            _ = Environment.NewLine;
        }
    }
}
