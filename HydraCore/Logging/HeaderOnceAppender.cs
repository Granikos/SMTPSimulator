using log4net.Appender;

namespace HydraCore.Logging
{
    public class HeaderOnceAppender : FileAppender
    {
        protected override void WriteHeader()
        {
            if (LockingModel.AcquireLock().Length == 0)
            {
                base.WriteHeader();
            }
        }
    }
}