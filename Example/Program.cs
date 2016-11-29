using DependencyInjector;

namespace Example
{
    internal static class Program
    {
        internal static void Main(string[] args)
        {
            Container container = new Container();

            container.RegisterType<IPowerSource, Battery>();

            Laptop laptop = container.Resolve<Laptop>();

            laptop.DisplayPower();
        }
    }
}
