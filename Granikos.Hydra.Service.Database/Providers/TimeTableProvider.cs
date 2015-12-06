using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.Hydra.Service.Database.Models;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.Models.Providers;

namespace Granikos.Hydra.Service.Database.Providers
{
    [Export(typeof(ITimeTableProvider))]
    public class TimeTableProvider : DefaultProvider<TimeTable, ITimeTable>, ITimeTableProvider<TimeTable>, ITimeTableProvider
    {
        public override bool Validate(TimeTable entity, out string message)
        {
            // TODO
            message = null;

            return true;
        }

        public TimeTableProvider() : base(TimeTable.FromOther)
        {
        }

        public string ResultFileName { get; private set; }

#if DEBUG
        protected IEnumerable<TimeTable> Initializer()
        {
            yield return new TimeTable
            {
                Name = "Test",
                Active = true,
                MailTemplateId = 1,
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
                    {"staticType", "1h"}
                },
                ReportMailAddress = "report@test.de",
                SendEicarFile = false
            };
            yield return new TimeTable
            {
                Name = "Test 2",
                Active = false,
                MailTemplateId = 1
            };
        }
#endif

        public event TimeTableChangeHandler<TimeTable> OnAdd;

        event TimeTableChangeHandler<ITimeTable> ITimeTableProvider<ITimeTable>.OnRemove
        {
            add { OnRemove += value; }
            remove { OnRemove -= value; }
        }

        event TimeTableChangeHandler<ITimeTable> ITimeTableProvider<ITimeTable>.OnAdd
        {
            add { OnAdd += value; }
            remove { OnAdd -= value; }
        }

        public event TimeTableChangeHandler<TimeTable> OnRemove;

        public void IncreaseErrorMailCount(int id)
        {
            Get(id).MailsError++;

            Database.SaveChanges();
        }

        public void IncreaseSuccessMailCount(int id)
        {
            Get(id).MailsSuccess++;

            Database.SaveChanges();
        }

        ITimeTable ITimeTableProvider<ITimeTable>.GetEmptyTimeTable()
        {
            return GetEmptyTimeTable();
        }

        public TimeTable GetEmptyTimeTable()
        {
            return new TimeTable();
        }
    }
}