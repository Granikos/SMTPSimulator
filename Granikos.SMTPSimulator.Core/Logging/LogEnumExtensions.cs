namespace Granikos.SMTPSimulator.Core.Logging
{
    public static class LogEnumExtensions
    {
        public static string GetSymbol(this LogPartType part)
        {
            switch (part)
            {
                case LogPartType.Client:
                    return "C";
                case LogPartType.Server:
                    return "S";
                case LogPartType.Other:
                    return "O";
                default:
                    return null;
            }
        }

        public static string GetSymbol(this LogEventType type)
        {
            switch (type)
            {
                case LogEventType.Connect:
                    return "+";
                case LogEventType.Disconnect:
                    return "-";
                case LogEventType.Incoming:
                    return "<";
                case LogEventType.Outgoing:
                    return ">";
                case LogEventType.Certificate:
                    return "#";
                case LogEventType.Other:
                    return "*";
                default:
                    return null;
            }
        }

        public static bool IsConnectionEvent(this LogEventType type)
        {
            switch (type)
            {
                case LogEventType.Connect:
                    return true;
                case LogEventType.Disconnect:
                    return true;
                default:
                    return false;
            }
        }
    }
}