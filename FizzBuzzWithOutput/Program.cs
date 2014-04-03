using System;
using FizzBuzzLibrary;

namespace FizzBuzzWithOutput
{
    class Program
    {
        static void Main(string[] args)
        {
            var fizzBuzz = new FizzBuzzExecutor(Console.Out);

            fizzBuzz.Execute(15, 30);

            Console.ReadKey();
        }
    }
}
