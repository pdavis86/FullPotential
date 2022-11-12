using System;

namespace FullPotential.Api.Ioc
{
    public struct Dependency
    {
        public Type Type { get; set; }
        public Func<object> Factory { get; set; }
        public bool IsSingleton { get; set; }
    }
}
