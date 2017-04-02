using System;
using System.Runtime.Serialization;

namespace TraceSpyService
{
	[Serializable]
	public class TraceSpyServiceException: Exception
	{
		public TraceSpyServiceException()
			:base()
		{
		}

		public TraceSpyServiceException(string message)
			:base(message)
		{
		}

		public TraceSpyServiceException(string message, Exception innerException)
			:base(message, innerException)
		{
		}

		protected TraceSpyServiceException(SerializationInfo info, StreamingContext context)
			:base(info, context)
		{
		}
	}
}
