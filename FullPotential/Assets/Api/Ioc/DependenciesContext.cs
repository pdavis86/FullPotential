
//Inspiration from https://moderncsharpinunity.github.io/post/dependency-injection-on-unity/

namespace FullPotential.Api.Ioc
{
    public static class DependenciesContext
    {
        public static DependenciesCollection Dependencies { get; } = new DependenciesCollection();
    }
}
