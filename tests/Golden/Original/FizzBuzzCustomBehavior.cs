using System.Collections.Generic;
using System.IO;
using FizzBuzzLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FizzBuzzTest
{
    [TestClass]
    public class FizzBuzzCustomBehavior
    {
        private FizzBuzzProcessor _fizzBuzz;
        private TextWriter _writer;

        [TestInitialize]
        public void Setup()
        {
            _writer = new StringWriter();

            var pattern = new Dictionary<long, string>
            {
                {2, "Even"},
                {6, "Scary"},
                {7, "Lucky"}
            };

            _fizzBuzz = new FizzBuzzProcessor(pattern, _writer);
        }

        [TestMethod]
        public void ShouldOutputNotOutputExtrasOn3()
        {
            _fizzBuzz.Execute(3);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 3 \n");
        }

        [TestMethod]
        public void ShouldOutputLuckyFor7()
        {
            _fizzBuzz.Execute(7);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 7 Lucky\n");
        }

        [TestMethod]
        public void ShouldOutputEvenScaryFor6()
        {
            _fizzBuzz.Execute(6);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 6 EvenScary\n");
        }

        [TestMethod]
        public void ShouldOutputEvenScaryLuckyOn42()
        {
            _fizzBuzz.Execute(42);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 42 EvenScaryLucky\n");
        }
    }
}