using FullPotential.Api.Ioc;
using Moq;
using NUnit.Framework;

namespace FullPotential.Api.IntegrationTests.TestHelpers
{
    public class MonoBehaviourTestsBase
    {
        [SetUp]
        public void MonoBehaviourTestsBaseSetup()
        {
            DependenciesContext.Dependencies.ResetForTesting();
        }

        protected void SetupSingletonServiceMock<T>(Mock<T> mock) where T : class
        {
            DependenciesContext.Dependencies.Register(new Dependency
            {
                Type = typeof(T),
                Factory = () => mock.Object,
                IsSingleton = true
            });
        }
    }
}
