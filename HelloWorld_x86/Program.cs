using System;

namespace HelloWorld_x86
{
    class App
    {
        public event EventHandler Initialized;
        public void Initialize()
        {
            Console.WriteLine(nameof(Initialized));
            Initialized?.Invoke(this, new EventArgs());

            var a = new Action(() => Inner());
            a.Invoke();

            Recursive(3);

            CycleA(3);
        }

        private void CycleA(int v)
        {
            if (v <= 0) return;
            CycleB(v - 1);
        }

        private void CycleB(int v)
        {
            CycleA(v - 1);
        }

        private void Recursive(int v)
        {
            if (v == 0) return;
            Recursive(--v);
        }

        void Inner()
        {
            Console.WriteLine("Inner");
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
            var a = new App();
            a.Initialized += AppIsInitialized;
            a.Initialized += (sender, arguments) => { Console.WriteLine("event handled"); };
            a.Initialize();
            Console.WriteLine(Mult(2, 3));

            Console.ReadKey();
        }

        private static void AppIsInitialized(object sender, EventArgs e)
        {
            var a = new App();
            a.Initialize();
        }
    }
}