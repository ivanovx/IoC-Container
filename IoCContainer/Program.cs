using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VasContainer;

namespace IoCContainer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var ct = new Container(ContainerOptions.UseDefaultValue);
            ct.RegisterType<IPowerSource, Battery>();
            var laptop1 = ct.Resolve<Laptop>();
            laptop1.DisplayPower();
        }
    }

    public interface IPowerSource
    {
        int GetPower();
    }

    public class Battery : IPowerSource
    {
        public int GetPower()
        {
            return -1;
        }
    }

    public class PowerOutlet : IPowerSource
    {
        public int GetPower()
        {
            return 100000;
        }
    }

    public class Test : IPowerSource
    {
        public int test { get; set; }

        public Test(int test)
        {
            this.test = test;
        }

        public int GetPower()
        {
            return test;
        }
    }

    public class Laptop
    {
        private IPowerSource source;

        public Laptop(IPowerSource powerSource)
        {
            this.source = powerSource;
        }

        public void DisplayPower()
        {
            Console.WriteLine(this.source.GetPower());
        }
    }
}
