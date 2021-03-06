﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZocBuild.Database.Exceptions
{
    /// <summary>
    /// An exception raised when an error occurs in an external process.
    /// </summary>
    public class ExternalProcessException : Exception
    {
        /// <summary>
        /// Creates an instance of the external process exception.
        /// </summary>
        /// <param name="executablePath">The path to the executable.</param>
        /// <param name="exitCode">The exit code generated by the process.</param>
        /// <param name="output">The content emitted to the standard output stream.</param>
        /// <param name="error">The content emitted to the standard error stream.</param>
        public ExternalProcessException(string executablePath, int exitCode, string output, string error)
            : base(string.Format("The executable located at '{0}' exited with code {1}. The error text follows:{2}", executablePath, exitCode, Environment.NewLine + (error ?? string.Empty)))
        {
            ExecutablePath = executablePath;
            ExitCode = exitCode;
            StandardOutput = output;
            StandardError = error;
        }

        /// <summary>
        /// Gets the path to the executable.
        /// </summary>
        public string ExecutablePath { get; private set; }

        /// <summary>
        /// Gets the exit code generated by the process.
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Gets the content emitted to the standard output stream.
        /// </summary>
        public string StandardOutput { get; private set; }

        /// <summary>
        /// Gets the content emitted to the standard error stream.
        /// </summary>
        public string StandardError { get; private set; }
    }
}
