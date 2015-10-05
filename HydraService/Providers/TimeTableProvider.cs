using System.Collections.Generic;
using System.ComponentModel.Composition;
using HydraService.Models;

namespace HydraService.Providers
{
    [Export(typeof(ITimeTableProvider))]
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
                MailContent = "Test Mail Content"
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