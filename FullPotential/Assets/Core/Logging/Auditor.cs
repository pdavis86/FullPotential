using System;
using System.Text;

using FullPotential.Api.Logging;

namespace FullPotential.Core.Logging
{
    public class Auditor<T> : IAuditor
    {
        private AuditLevel _logLevel = AuditLevel.Info;

        public void SetLogLevel(AuditLevel level)
        {
            _logLevel = level;
        }

        public bool IsEnabled(AuditLevel level)
        {
            return level >= _logLevel;
        }

        public void Debug(string message)
        {
            Write(AuditLevel.Debug, message);
        }

        public void Info(string message)
        {
            Write(AuditLevel.Info, message);
        }

        public void Warn(string message, Exception exception = null)
        {
            Write(AuditLevel.Warn, message);
        }

        public void Error(Exception exception)
        {
            Write(AuditLevel.Error, null, exception);
        }

        public void Error(string message, Exception exception = null)
        {
            Write(AuditLevel.Error, message);
        }

        private void Write(AuditLevel level, string message, Exception exception = null)
        {
            if (!IsEnabled(level))
            {
                return;
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
