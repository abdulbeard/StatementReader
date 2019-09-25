using StatementReader.Parsers;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new BoAStatementParser();
            var result = parser.Parse();
            //var mccs = StatementReader.Mcc.Mcc.VisaCodes;
            Console.WriteLine("Hello World!");
        }
    }
}
