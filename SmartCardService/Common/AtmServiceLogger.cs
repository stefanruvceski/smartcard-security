using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class AtmServiceLogger
    {
        #region Fields
        
        private string sourceName = "AtmService";               // Application ako hocu da bude sa ostaim Windows Event Log-ovima 
        private string logName = "AtmServiceLog";               // Application
        private EventLog eventLog;

        public EventLog EventLog { get => eventLog; set => eventLog = value; }
        #endregion

        #region Methods

        public AtmServiceLogger()
        {
            if (!EventLog.SourceExists(sourceName))
            {
                EventLog.CreateEventSource(sourceName, logName);
            }

            this.EventLog = new EventLog(logName, Environment.MachineName, sourceName);
        }

        public void WriteInformation(string message)
        {
            using (this.EventLog = new EventLog(logName))
            {
                this.EventLog.Source = sourceName;
                this.EventLog.WriteEntry(message, EventLogEntryType.Information, 1001, 1);
            }
        }

        public void WriteError(string message)
        {
            using (this.EventLog = new EventLog(logName))
            {
                this.EventLog.Source = sourceName;
                this.EventLog.WriteEntry(message, EventLogEntryType.Error, 1001, 1);
            }
        }

        #endregion
    }
}
