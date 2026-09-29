using System;
using FizzBuzzLibrary;

namespace FizzBuzzWithOutput
{
    internal class Program
    {
        private static void Main()
        {
            var fizzBuzz = new FizzBuzzProcessor(Console.Out);

            fizzBuzz.Execute(15, 30);

            Console.ReadKey();
        }
    }
}