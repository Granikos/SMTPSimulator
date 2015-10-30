using System.Collections.Generic;
using Granikos.Hydra.Service.Models;
using Granikos.Hydra.Service.TimeTables;

namespace Granikos.Hydra.Service.Providers
{
    public delegate void TimeTableChangeHandler(TimeTable timeTable);

    public interface ITimeTableProvider : IDataProvider<TimeTable, int>
    {
        IEnumerable<TimeTableTypeInfo> GetTimeTableTypes();

        IDictionary<string,string> GetTimeTableTypeData(string type);

        event TimeTableChangeHandler OnAdd;

        event TimeTableChangeHandler OnRemove;

        void IncreaseErrorMailCount(int id);

        void IncreaseSuccessMailCount(int id);

        IEnumerable<MailTemplateType> GetMailTemplates();

    }
}