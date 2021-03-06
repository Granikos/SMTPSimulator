// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
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
            var oldActive = Database.Entry(dbEntity).OriginalValues.GetValue<bool>("Active");
            if (entity.Active != oldActive)
            {
                if (entity.Active)
                {
                    Database.Entry(dbEntity).Property(t => t.ActiveSince).CurrentValue = DateTime.Now;
                    Database.Entry(dbEntity).Property(t => t.ActiveUntil).IsModified = false;
                }
                else
                {
                    Database.Entry(dbEntity).Property(t => t.ActiveSince).IsModified = false;
                    Database.Entry(dbEntity).Property(t => t.ActiveUntil).CurrentValue = DateTime.Now;
                }
            }

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