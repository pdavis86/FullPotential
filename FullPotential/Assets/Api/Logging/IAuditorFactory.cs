namespace FullPotential.Api.Logging
{
    public interface IAuditorFactory
    {
        IAuditor Create(object sender);
    }
}
