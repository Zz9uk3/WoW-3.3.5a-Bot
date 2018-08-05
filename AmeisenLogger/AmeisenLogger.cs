﻿using AmeisenUtilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AmeisenLogging
{
    /// <summary>
    /// Class to store a log entry within
    /// </summary>
    public class AmeisenLogEntry
    {
        public LogLevel loglevel;
        public int id;
        public string timestamp;
        public string msg;
        public object originClass;
        public string functionName;

        public override string ToString()
        {
            return "[" + id + "][" + timestamp + "]\t[" + loglevel.ToString() + "][" + originClass.ToString() + ":" + functionName + "] - " + msg;
        }
    }

    public class AmeisenLogger
    {
        private readonly string logPath = AppDomain.CurrentDomain.BaseDirectory + "/logs/";
        private readonly string logName;

        private LogLevel activeLogLevel;
        private int logcount = 0;
        private bool loggingActive;
        private ConcurrentQueue<AmeisenLogEntry> entries;
        private Thread loggingThread;

        private static AmeisenLogger i;

        private AmeisenLogger()
        {
            activeLogLevel = LogLevel.WARNING; // Default to avoid spam
            loggingActive = true;
            entries = new ConcurrentQueue<AmeisenLogEntry>();
            loggingThread = new Thread(new ThreadStart(WorkOnQueue));
            loggingThread.Start();
            logName = DateTime.Now.ToString("dd-MM-yyyy") + "_" + DateTime.Now.ToString("HH-mm") + ".txt";
        }

        /// <summary>
        /// Initialize/Get the instance of our singleton
        /// </summary>
        /// <returns>AmeisenLogger instance</returns>
        public static AmeisenLogger GetInstance()
        {
            if (i == null)
                i = new AmeisenLogger();
            return i;
        }

        /// <summary>
        /// Set the LogLevel that is going to be saved in the logs
        /// </summary>
        /// <param name="logLevel">LogLevel to save to the logfile</param>
        public void SetActiveLogLevel(LogLevel logLevel) { activeLogLevel = logLevel; }

        /// <summary>
        /// Stop the logging thread, dont forget it!
        /// </summary>
        public void StopLogging() { loggingActive = false; }

        /// <summary>
        /// Add an entry to the log
        /// </summary>
        /// <param name="loglevel">LogLevel of this Log-Message</param>
        /// <param name="msg">...</param>
        /// <param name="self">Class that calls it / string with class name</param>
        /// <param name="functionName">Function name that this is called by, no need to set this manually</param>
        /// <returns>The AmeisenLogEntry</returns>
        public AmeisenLogEntry Log(LogLevel loglevel, string msg, object self, [CallerMemberName]string functionName = "")
        {
            AmeisenLogEntry logEntry = new AmeisenLogEntry
            {
                loglevel = loglevel,
                id = logcount,
                timestamp = DateTime.Now.ToLongTimeString(),
                msg = msg,
                originClass = self,
                functionName = functionName
            };

            if (loglevel >= activeLogLevel)
            {
                logcount++;
                entries.Enqueue(logEntry);
            }
            return logEntry;
        }

        private void WorkOnQueue()
        {
            while (loggingActive)
            {
                if (!entries.IsEmpty)
                {
                    if (entries.TryDequeue(out AmeisenLogEntry currentEntry))
                    {
                        SaveLogToFile(currentEntry);
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void SaveLogToFile(AmeisenLogEntry entry)
        {
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            File.AppendAllText(logPath + logName, entry.ToString() + Environment.NewLine);
        }
    }
}