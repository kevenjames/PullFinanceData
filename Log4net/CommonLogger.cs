namespace PullFinanceData.Log4net
{
    public class CommonLogger:BaseLogger
    {
        public CommonLogger()
            : base("CommonLogger", "CommonLog")
        {
        }

        public void LogMessage(string eventDescription, EventStatus eventStatus, EventType eventType)
        {
            //hexadecimal value 0x00, will cause write to event missing content after \0.
            eventDescription = eventDescription.Replace("\0", "");
            var eventSeverity = EventSeverity.medium;
            if (eventStatus == EventStatus.failure)
            {
                eventSeverity = EventSeverity.critical;
            }
            Log(eventDescription, eventSeverity, eventStatus, eventType);
        }

        public void LogMessage(string requestId, string eventDescription, EventStatus eventStatus, EventType eventType)
        {
            this.request_id = requestId;
            LogMessage(eventDescription, eventStatus, eventType);
        }
    }
}