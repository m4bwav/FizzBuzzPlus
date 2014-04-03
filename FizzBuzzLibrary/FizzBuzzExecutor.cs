using System;
using System.IO;

namespace FizzBuzzLibrary
{
    public class FizzBuzzExecutor
    {
        private readonly TextWriter _writer;

        public FizzBuzzExecutor(TextWriter writer)
        {
            _writer = writer;
        }

        public void Execute()
        {
            Execute(1, 100);
        }

        public void Execute(int singleValue)
        {
            Execute(singleValue, singleValue);
        }

        public void Execute(int startValue, int endValue)
        {
            for (int iii = startValue; iii <= endValue; iii++)
            {
                _writer.Write("Current Number: " + iii + " ");

                if (iii % 3 == 0)
                    _writer.Write("Fizz");

                if (iii % 5 == 0)
                    _writer.Write("Buzz");

                _writer.Write("\n");
            }
        }
    }

}
