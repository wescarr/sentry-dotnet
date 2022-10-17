using System;
using System.Diagnostics.Tracing;

namespace Sentry.Internal.Etw
{
    internal class SqlEventListener : EventListener
    {
        protected override void OnEventSourceCreated(EventSource source)
        {
            // Only enable events from SqlClientEventSource.
            if (source.Name.Equals("Microsoft.Data.SqlClient.EventSource"))
            {
                // Use EventKeyWord 2 to capture basic application flow events.
                EnableEvents(source, EventLevel.Informational, (EventKeywords)2);
            }
        }

        // This callback runs whenever an event is written by SqlClientEventSource.
        // Event data is accessed through the EventWrittenEventArgs parameter.
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // Print event data.
            var o = eventData.Payload![0];
            Console.WriteLine(o);
        }
    }
}
