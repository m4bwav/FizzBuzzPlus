using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FizzBuzzLibrary
{
    public class FizzBuzzProcessor
    {
        private readonly IDictionary<long, string> _breaks;
        private readonly TextWriter _writer;

        public FizzBuzzProcessor(IDictionary<long, string> outputBreaks, TextWriter writer)
        {
            _breaks = outputBreaks;
            _writer = writer;
        }

        public FizzBuzzProcessor(TextWriter writer)
        {
            _writer = writer;
            _breaks = new Dictionary<long, string>
            {
                {3, "Fizz"},
                {5, "Buzz"}
            };
        }

        private IEnumerable<KeyValuePair<long, string>> OrderedBreakPoints
        {
            get { return _breaks.OrderBy(x => x.Key); }
        }

        public void Execute()
        {
            Execute(1, 100);
        }

        public void Execute(long singleValue)
        {
            Execute(singleValue, singleValue);
        }

        public void Execute(long startValue, long endValue)
        {
            for (var iii = startValue; iii <= endValue; iii++)
            {
                _writer.Write("Current Number: " + iii + " ");

                CheckForBreak(iii);

                _writer.Write("\n");
            }
        }

        private void CheckForBreak(long currentValue)
        {
            foreach (var breakPoint in OrderedBreakPoints)
            {
                if (currentValue%breakPoint.Key == 0)
                    _writer.Write(breakPoint.Value);
            }
        }
    }
}