using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.NikosTwo.Service.Database.Models;
using Granikos.NikosTwo.Service.Models;
using Granikos.NikosTwo.Service.Models.Providers;

namespace Granikos.NikosTwo.Service.Database.Providers
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

        protected override void OnUpdate(TimeTable entity, TimeTable dbEntity)
        {
            Database.Entry(dbEntity).Property(t => t.MailsError).IsModified = false;
            Database.Entry(dbEntity).Property(t => t.MailsSuccess).IsModified = false;
        }

        protected override void OnAdded(TimeTable entity)
        {
            if (OnTimeTableAdd != null) OnTimeTableAdd(entity);
        }

        protected override void OnUpdated(TimeTable entity)
        {
            if (OnTimeTableRemove != null) OnTimeTableRemove(entity);
            if (OnTimeTableAdd != null) OnTimeTableAdd(entity);
        }


        protected override void OnDeleted(TimeTable entity)
        {
            if (OnTimeTableRemove != null) OnTimeTableRemove(entity);
        }

#if DEBUG
        protected IEnumerable<TimeTable> Initializer()
        {
            yield return new TimeTable
            {
                Name = "Test",
                Active = true,
                MailTemplateId = 1,
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

        public event TimeTableChangeHandler<TimeTable> OnTimeTableAdd;

        event TimeTableChangeHandler<ITimeTable> ITimeTableProvider<ITimeTable>.OnTimeTableRemove
        {
            add { OnTimeTableRemove += value; }
            remove { OnTimeTableRemove -= value; }
        }

        event TimeTableChangeHandler<ITimeTable> ITimeTableProvider<ITimeTable>.OnTimeTableAdd
        {
            add { OnTimeTableAdd += value; }
            remove { OnTimeTableAdd -= value; }
        }

        public event TimeTableChangeHandler<TimeTable> OnTimeTableRemove;

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