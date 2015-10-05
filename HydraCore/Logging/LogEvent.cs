using System.Net;

namespace HydraCore.Logging
{
    public class LogEvent
    {
        public string ConnectorId { get; set; }
        public string Session { get; set; }
        public int SequenceNumber { get; set; }
        public IPEndPoint LocalIP { get; set; }
        public IPEndPoint RemoteIP { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
    }
}