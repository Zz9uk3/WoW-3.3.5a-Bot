using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AmeisenLogging
{
    public enum LogLevel
    {
        VERBOSE,
        DEBUG,
        WARNING,
        ERROR
    }

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

        public void StopLogging() { loggingActive = false; }

        public static AmeisenLogger GetInstance()
        {
            if (i == null)
                i = new AmeisenLogger();
            return i;
        }

        public void SetActiveLogLevel(LogLevel logLevel) { activeLogLevel = logLevel; }

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
    }
}
