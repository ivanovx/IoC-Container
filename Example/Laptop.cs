using System;

namespace Example
{
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