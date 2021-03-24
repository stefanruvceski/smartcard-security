using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SmartCardServiceLogger
    {
        #region Fields

        private string sourceName = "SmartCardService";                  
        private string logName = "SmartCardServiceLog";                 
        private EventLog eventLog;

        public EventLog EventLog { get => eventLog; set => eventLog = value; }
        #endregion

        #region Methods

        public SmartCardServiceLogger()
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
