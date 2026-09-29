using System;

namespace FullPotential.Api.Logging
{
    public interface IAuditor
    {
        bool IsEnabled(AuditLevel level);

        void Debug(string message, params object[] args);

        void Info(string message, params object[] args);

        void Warn(string message, params object[] args);

        void Warn(Exception exception, string message, params object[] args);

        void Error(Exception exception);

        void Error(Exception exception, string message, params object[] args);

        void Error(string message, params object[] args);
    }
}
