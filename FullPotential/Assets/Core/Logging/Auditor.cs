using System;
using System.Text;

using FullPotential.Api.Logging;

namespace FullPotential.Core.Logging
{
    public class Auditor<T> : IAuditor
    {
        public bool IsEnabled(AuditLevel level)
        {
            return level >= AuditorFactory.Level;
        }

        public void Debug(string message, params object[] args)
        {
            Write(AuditLevel.Debug, message, null, args);
        }

        public void Info(string message, params object[] args)
        {
            Write(AuditLevel.Info, message, null, args);
        }

        public void Warn(string message, params object[] args)
        {
            Write(AuditLevel.Warn, message, null, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            Write(AuditLevel.Warn, message, exception, args);
        }

        public void Error(Exception exception)
        {
            Write(AuditLevel.Error, null, exception);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            Write(AuditLevel.Error, message, exception, args);
        }

        public void Error(string message, params object[] args)
        {
            Write(AuditLevel.Error, message, null, args);
        }

        private void Write(AuditLevel level, string message, Exception exception = null, params object[] args)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            if (args != null && args.Length > 0)
            {
                message = string.Format(message, args);
            }

            var sb = new StringBuilder(typeof(T).Name);
            sb.Append(": ");
            sb.Append(message);

            if (exception != null)
            {
                sb.Append(System.Environment.NewLine);
                sb.Append(exception);
            }

            switch (level)
            {
                case AuditLevel.Debug:
                    UnityEngine.Debug.Log(sb.ToString());
                    break;

                case AuditLevel.Info:
                    UnityEngine.Debug.Log(sb.ToString());
                    break;

                case AuditLevel.Warn:
                    UnityEngine.Debug.LogWarning(sb.ToString());
                    break;

                case AuditLevel.Error:
                    UnityEngine.Debug.LogError(sb.ToString());
                    break;
            }
        }
    }
}
