using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    [Export(typeof (ITimeTableProvider))]
    public class TimeTableProvider : DefaultProvider<TimeTable>, ITimeTableProvider
    {
        public TimeTableProvider()
            : base("TimeTables")
        {
        }

#if DEBUG
        protected override IEnumerable<TimeTable> Initializer()
        {
            yield return new TimeTable
            {
                Name = "Test",
                Active = true,
                MailContent = "Test Mail Content",
                MailType = "default",
                MinRecipients = 1,
                MaxRecipients = 2,
                ProtocolLevel = ProtocolLevel.On,
                ReportType = ReportType.Daily,
                Type = "static",
                StaticRecipient = true,
                RecipientMailbox = "fu@bar.de",
                StaticSender = true,
                SenderMailbox = "bernd@test.de",
                AttachmentType = AttachmentType.Off,
                Parameters = new Dictionary<string, string>
                {
                    {"type", "1h"}
                },
                TimeData = Enumerable.Repeat("0", 24*7).ToArray(),
                ReportMailAddress = "report@test.de",
                SendEicarFile = false
            };
            yield return new TimeTable
            {
                Name = "Test 2",
                Active = false,
                MailContent = "Test 2 Mail Content"
            };
        }
#endif
    }
}