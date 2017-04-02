using System;
using System.Runtime.Serialization;

namespace TraceSpyService
{
    [DataContract]
    public class EtwRecord
    {
        [DataMember]
        public long Index { get; set; }

        [DataMember]
        public long Ticks { get; set; }

        [DataMember]
        public string ProcessName { get; set; }

        [DataMember]
        public string Text { get; set; }
    }
}
