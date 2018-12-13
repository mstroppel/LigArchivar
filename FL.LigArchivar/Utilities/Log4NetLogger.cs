using System;
using System.Globalization;
using Caliburn.Micro;

namespace FL.LigArchivar.GUI.Utilities
{
    /// <summary>
    /// <see cref="ILog"/> implementation with <see cref="log4net"/>.
    /// </summary>
    internal class Log4NetLogger : ILog
    {
        /// <summary>
        /// The inner logger.
        /// </summary>
        private readonly log4net.ILog _innerLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public Log4NetLogger(Type type)
        {
            _innerLogger = log4net.LogManager.GetLogger(type);
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="exception">The exception.</param>
        public void Error(Exception exception)
        {
            _innerLogger.Error(exception.Message, exception);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void Info(string format, params object[] args)
        {
            _innerLogger.InfoFormat(CultureInfo.DefaultThreadCurrentCulture, format, args);
        }

        /// <summary>
        /// Logs an warning message.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The arguments.</param>
        public void Warn(string format, params object[] args)
        {
            _innerLogger.WarnFormat(CultureInfo.DefaultThreadCurrentCulture, format, args);
        }
    }
}
