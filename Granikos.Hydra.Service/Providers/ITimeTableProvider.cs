using System.Collections.Generic;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Providers
{
    public interface ITimeTableProvider : IDataProvider<TimeTable, int>
    {
        IEnumerable<TimeTableTypeInfo> GetTimeTableTypes();
    }
}