using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Granikos.NikosTwo.Service
{
    public static class PerformanceCounters
    {
        private static PerformanceCounter _sentLast5MinCounter;
        private static PerformanceCounter _sentLast60MinCounter;
        private static PerformanceCounter _receivedLast5MinCounter;
        private static PerformanceCounter _receivedLast60MinCounter;

        private static readonly Dictionary<string,PerformanceCounter> QueueLengthCounters = new Dictionary<string, PerformanceCounter>();

        static PerformanceCounters()
        {
            if (!PerformanceCounterCategory.Exists("nikos two Mail Service"))
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                CounterCreationData sentLast5Min = new CounterCreationData
                {
                    CounterName = "Emails sent (last 5 mins)",
                    CounterHelp = "​Emails sent during the last 5 minutes (total)",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
                counters.Add(sentLast5Min);

                CounterCreationData sentLast60Min = new CounterCreationData
                {
                    CounterName = "Emails sent (last 60 mins)",
                    CounterHelp = "​Emails sent during the last 60 minutes (total)",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
                counters.Add(sentLast60Min);

                CounterCreationData receivedLast5Min = new CounterCreationData
                {
                    CounterName = "Emails received (last 5 mins)",
                    CounterHelp = "​Emails received during the last 5 minutes (total)",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
                counters.Add(receivedLast5Min);

                CounterCreationData receivedLast60Min = new CounterCreationData
                {
                    CounterName = "Emails received (last 60 mins)",
                    CounterHelp = "​Emails received during the last 60 minutes (total)",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
                counters.Add(receivedLast60Min);

                CounterCreationData queueLength = new CounterCreationData
                {
                    CounterName = "​Queue length",
                    CounterHelp = "​Number of emails in queue to be sent",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };
                counters.Add(queueLength);

                PerformanceCounterCategory.Create("nikos two Mail Service",
                        "nikos two Mail Service", PerformanceCounterCategoryType.MultiInstance, counters);
            }
        }

        public static PerformanceCounter SentLast5Min
        {
            get
            {
                return _sentLast5MinCounter ??
                       (_sentLast5MinCounter =
                           new PerformanceCounter("nikos two Mail Service", "Emails sent (last 5 mins)", "Total", false));
            }
        }

        public static PerformanceCounter SentLast60Min
        {
            get
            {
                return _sentLast60MinCounter ??
                       (_sentLast60MinCounter =
                           new PerformanceCounter("nikos two Mail Service", "Emails sent (last 60 mins)", "Total", false));
            }
        }

        public static PerformanceCounter ReceivedLast5Min
        {
            get
            {
                return _receivedLast5MinCounter ??
                       (_receivedLast5MinCounter =
                           new PerformanceCounter("nikos two Mail Service", "Emails received (last 5 mins)", "Total", false));
            }
        }

        public static PerformanceCounter ReceivedLast60Min
        {
            get
            {
                return _receivedLast60MinCounter ??
                       (_receivedLast60MinCounter =
                           new PerformanceCounter("nikos two Mail Service", "Emails received (last 60 mins)", "Total", false));
            }
        }

        public static PerformanceCounter ForQueueLength(string connectorName)
        {
            if (!QueueLengthCounters.ContainsKey(connectorName))
            {
                QueueLengthCounters.Add(connectorName, new PerformanceCounter("nikos two Mail Service", "Emails received (last 60 mins)", connectorName, false));
            }

            return QueueLengthCounters[connectorName];
        }

        public static void TriggerSent()
        {
            SentLast5Min.Increment();
            SentLast60Min.Increment();
            new Timer(state => { SentLast5Min.Decrement(); }, null, TimeSpan.FromMinutes(5), TimeSpan.Zero);
            new Timer(state => { SentLast60Min.Decrement(); }, null, TimeSpan.FromMinutes(60), TimeSpan.Zero);
        }

        public static void TriggerReceived()
        {
            ReceivedLast5Min.Increment();
            ReceivedLast60Min.Increment();
            new Timer(state => { ReceivedLast5Min.Decrement(); }, null, TimeSpan.FromMinutes(5), TimeSpan.Zero);
            new Timer(state => { ReceivedLast60Min.Decrement(); }, null, TimeSpan.FromMinutes(60), TimeSpan.Zero);
        }
    }
}
