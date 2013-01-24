using System;

namespace TeamCityBuildChanges.Output
{
    public class LogEntry
    {
        DateTime Occurred { get; set; }
        Status Status { get; set; }
        string StatusText { get; set; }

        public LogEntry(DateTime occurred, Status status, string text)
        {
            Occurred = occurred;
            Status = status;
            StatusText = text;
        }
    }
}