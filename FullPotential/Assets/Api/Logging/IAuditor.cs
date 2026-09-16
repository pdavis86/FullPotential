using System;

namespace FullPotential.Api.Logging
{
    public interface IAuditor
    {
        void SetLogLevel(AuditLevel level);

        bool IsEnabled(AuditLevel level);

        void Debug(string message);

        void Info(string message);

        void Warn(string message, Exception exception = null);

        void Error(Exception exception);

        void Error(string message, Exception exception = null);
    }
}
