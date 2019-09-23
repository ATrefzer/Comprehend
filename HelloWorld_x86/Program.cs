using System;

namespace HelloWorld_x86
{
    internal class Program
    {
        private static int Add(int a, int b)
        {
            return a+b;
        }

    private static int Mult(int a, int b)
        {
            int result = 0;
            for (int i = 0; i <a; i++)
            {
                result = Add(result, b);
            }
            return result;
            
        }


private static void Main(string[] args)
        {
            Console.WriteLine(Mult(2, 3));

            Console.ReadKey();
        }
    }
}