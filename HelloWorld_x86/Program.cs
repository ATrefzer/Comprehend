
using System;
namespace HelloWorld_x86
{
    class App
    {
        public event EventHandler Initialized;
        public void Initialize()
        {
            Initialized?.Invoke(this, new EventArgs());
        }
        public void RunCycle()
        {
            CycleA(10);
        }
        public void CycleA(int counter)
        {
            if (counter <= 0)
            {
                return;
            }
            counter--;
            CycleB(counter);
        }
        private void CycleB(int counter)
        {
            counter--;
            CycleC(counter);
        }
        private void CycleC(int counter)
        {
            counter--;
            CycleA(counter);
        }
        public void RunRecursion()
        {
            Recursive(10);
        }
        private void Recursive(int counter)
        {
            if (counter <= 0)
            {
                return;
            }
            counter--;
            Recursive(counter);
        }
    }
    internal class Program
    {
        private static int Add(int a, int b)
        {
            return a + b;
        }
        private static int Mult(int a, int b)
        {
            var result = 0;
            for (var i = 0; i < a; i++)
            {
                result = Add(result, b);
            }
            return result;
        }

        private static void Main(string[] args)
        {
            Console.WriteLine(Mult(2, 3));
            var app = new App();
            app.Initialized += AppOnInitialized;
            app.Initialized += (sender, eventArgs) => Console.WriteLine("Initialized");
            app.Initialize();
            app.RunCycle();
            app.RunRecursion();
            //Console.ReadKey();
        }
        private static void AppOnInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Initialized (static)");
        }
    }
}