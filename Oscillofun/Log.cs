using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Reflection;

namespace Oscillofun
{
    public static class Log
    {
        /// <summary>
        /// Enumeration of all the different tiers of urgency of a log message
        /// </summary>
        public enum Urgency
        {
            Debug,
            Error
        }

        /// <summary>
        /// Event arguments for when the log is written to
        /// </summary>
        public class AppendLogEventArgs : EventArgs
        {
            /// <summary>
            /// The text just appended to the log
            /// </summary>
            public string AppendedText { get; private set; }

            public AppendLogEventArgs(string text)
            {
                AppendedText = text;
            }
        }

        /// <summary>
        /// Called when the log is written to
        /// </summary>
        public static event EventHandler<AppendLogEventArgs> OnLogAppended;

        static string logFile;
        static object fileLock = new object();

        static Log()
        {
            bool enableLogging = (Environment.CommandLine.IndexOf("-debug", StringComparison.OrdinalIgnoreCase) == -1);
#if DEBUG
            enableLogging = true;
#endif
            //if there ain't no trees we can't log 
            if (!enableLogging) return;

            logFile = Path.Combine(Assembly.GetExecutingAssembly().Location, "log.log");

            //Delete the file if it already exists, we only want to store the log from the last session
            if (File.Exists(logFile))
                File.Delete(logFile);

            //Welcome message :)
            Append("Log started at {0}. Version {1}.", Urgency.Debug, DateTime.Now.ToString(), Assembly.GetEntryAssembly().GetName().Version);
        }

        public static void Append(string text, Urgency type = Urgency.Debug, params object[] args)
        {
            Append(string.Format(text, args), type);
        }

        public static void Append(string text, Urgency type = Urgency.Debug)
        {
            string line = string.Format("[{0} - {1}] {2}{3}", type, DateTime.Now.ToShortTimeString(), text, Environment.NewLine);

            //Write to the event
            if (OnLogAppended != null)
                OnLogAppended(null, new AppendLogEventArgs(line));

            //Append our log file
            if (!string.IsNullOrEmpty(logFile))
            {
                Monitor.Enter(fileLock);
                File.AppendAllText(logFile, line);
                Monitor.Exit(fileLock);
            }
        }
    }

    public class LogWriter : StringWriter
    {
        public delegate void FlushedEventHandler(object sender, EventArgs args);
        public event FlushedEventHandler Flushed;

        protected void OnFlush()
        {
            var flshHandler = Flushed;
            if (flshHandler != null)
                flshHandler(this, EventArgs.Empty);

            Log.Append(this.ToString());
        }

        public override void Flush()
        {
            base.Flush();
            OnFlush();
        }

        public override void Write(char value)
        {
            base.Write(value);
            Flush();
        }

        public override void Write(string value)
        {
            base.Write(value);
            Flush();
        }

        public override void Write(char[] buffer, int index, int count)
        {
            base.Write(buffer, index, count);
            Flush();
        }
    }
}
