using System;
using System.Threading.Tasks;

namespace HelloWorld_x86
{
    class AsyncObj
    {
        public bool IsBusy
        {
            set => CallAsync3().Wait();
        }

        public async Task CallAsync()
        {
            IsBusy = true;
            await CallAsync2();
            IsBusy = false;
        }

        public async Task CallAsync2()
        {
            await Task.Delay(1);
            Foo();
        }

        public async Task CallAsync3()
        {
            await Task.Delay(1);
         
        }

        private void Foo()
        {

        }
    }
    internal class App
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

        public void RunRecursion()
        {
            Recursive(10);
        }

        internal void CycleC(int counter)
        {
            counter--;
            CycleA(counter);
        }

        private void CycleB(int counter)
        {
            counter--;
            CycleC(counter);
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
        public static async Task Main(string[] args)
        {
            //var obj = new AsyncObj();
            //await obj.CallAsync().ConfigureAwait(true);

            //Following code runs in different thread and is no longer in sequence diagram


            Mult(2, 3);
            Mult(2, 3);
            Add(1, 2);

            Console.WriteLine(Mult(2, 3));
            var app = new App();
            app.Initialized += AppOnInitialized;
            app.Initialized += (sender, eventArgs) => Console.WriteLine("Initialized");
            app.Initialize();
            app.RunCycle();
            app.RunRecursion();
            Poly(2);
            Poly();

            //Console.ReadKey();

        }

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

        private static void Poly(int i)
        {
            Console.WriteLine("Poly: int");
            Poly();
        }

        private static void Poly()
        {
            Console.WriteLine("Poly");
        }

        private static void AppOnInitialized(object sender, EventArgs e)
        {
            Console.WriteLine("Initialized (static)");
        }
    }
}