using System.IO;
using FizzBuzzLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FizzBuzzTest
{
    [TestClass]
    public class FizzBuzzBehavior
    {
        private FizzBuzzExecutor _fizzBuzz;
        private TextWriter _writer;

        [TestInitialize]
        public void Setup()
        {
            _writer = new StringWriter();

            _fizzBuzz = new FizzBuzzExecutor(_writer);
        }

        [TestMethod]
        public void ShouldOutputFizzBuzzOn15()
        {
            _fizzBuzz.Execute(15);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 15 FizzBuzz\n");
        }

        [TestMethod]
        public void ShouldOutputFizzOn18()
        {
            _fizzBuzz.Execute(18);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 18 Fizz\n");
        }

        [TestMethod]
        public void ShouldOutputBuzzOn20()
        {
            _fizzBuzz.Execute(20);

            var output = _writer.ToString();

            Assert.AreEqual(output, "Current Number: 20 Buzz\n");
        }
    }
}