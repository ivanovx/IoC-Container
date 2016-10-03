using DependencyInjector;

namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Container container = new Container(ContainerOptions.UseDefaultValue);

            container.RegisterType<IPowerSource, Battery>();

            Laptop laptop = container.Resolve<Laptop>();

            laptop.DisplayPower();
        }
    }
}
