using System;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var mode = args[0];
                var input = args.Length > 1 ? args[1] : "input.txt";
                if (mode == "interpreter")
                {
                    new Interpreter(input).execute();
                }
                else if (mode == "compiler")
                {
                    var output = args.Length > 2 ? args[2] : "output.txt";
                    new Syntatic(input, output).analysis();
                }
            }
        }
    }
}
