namespace Granikos.Hydra.Service.Models.Providers
{
    public delegate void TimeTableChangeHandler<in TTimeTable>(TTimeTable timeTable)
        where TTimeTable : ITimeTable;

    public interface ITimeTableProvider<TTimeTable> : IDataProvider<TTimeTable, int>
        where TTimeTable : ITimeTable
    {
        event TimeTableChangeHandler<TTimeTable> OnAdd;

        event TimeTableChangeHandler<TTimeTable> OnRemove;

        void IncreaseErrorMailCount(int id);

        void IncreaseSuccessMailCount(int id);

        TTimeTable GetEmptyTimeTable();
    }


    public interface ITimeTableProvider : ITimeTableProvider<ITimeTable>
    {
    }
}