using System;

namespace DependencyInjector
{
    [Flags]
    public enum ContainerOptions
    {
        None = 0,
        UseDefaultValue = 1
    }
}