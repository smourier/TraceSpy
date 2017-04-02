using System;

namespace TraceSpyService
{
	public class ConsoleControlEventArgs: EventArgs
	{
		public ConsoleControlEventArgs(ConsoleEventType eventType)
		{
			EventType = eventType;
		}

        public ConsoleEventType EventType { get; private set; }
	}
}
