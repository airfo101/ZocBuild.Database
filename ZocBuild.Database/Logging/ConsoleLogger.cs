﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Logging
{
    public class ConsoleLogger : ILogger
    {
        private readonly SeverityLevel minSeverity;
        private readonly string header;

        public ConsoleLogger(SeverityLevel minSeverity)
        {
            this.header = string.Empty;
            this.minSeverity = minSeverity;
        }

        public ConsoleLogger(string header, SeverityLevel minSeverity)
        {
            this.header = header + ": ";
            this.minSeverity = minSeverity;
        }

        public async Task LogMessageAsync(string message, SeverityLevel severity)
        {
            if (severity < minSeverity)
            {
                return;
            }
            await Console.Out.WriteAsync(header);
            await Console.Out.WriteAsync(GetSeverityString(severity));
            await Console.Out.WriteLineAsync(message);
        }

        private string GetSeverityString(SeverityLevel severity)
        {
            switch (severity)
            {
                case SeverityLevel.Critical:
                    return "[CRITICAL] ";
                case SeverityLevel.Error:
                    return "[ERROR] ";
                case SeverityLevel.Warning:
                    return "[WARNING] ";
                case SeverityLevel.Information:
                    return "[INFO] ";
                case SeverityLevel.Verbose:
                    return "[DEBUG] ";
                default:
                    throw new NotSupportedException("Unable to log severities of type " + severity + ".");
            }
        }
    }
}