using FullPotential.Api.Ioc;
using FullPotential.Api.Logging;

namespace FullPotential.Core.Logging
{
    public class AuditorFactory : IAuditorFactory
    {
        public IAuditor Create(object sender)
        {
            return CreateInternal(sender);
        }

        private IAuditor CreateInternal<T>(T sender)
        {
            var auditorType = typeof(Auditor<>).MakeGenericType(sender.GetType());

            return (IAuditor)DependenciesContext.Dependencies.CreateInstance(auditorType);
            //return (IAuditor)Activator.CreateInstance(auditorType);
        }
    }
}
