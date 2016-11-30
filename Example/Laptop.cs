using System;

namespace Example
{
    public class Laptop
    {
        private readonly IPowerSource source;

        public Laptop(IPowerSource source)
        {
            this.source = source;
        }

        public void DisplayPower()
        {
            Console.WriteLine(this.source.Power);
        }
    }
}