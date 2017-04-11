using log4net.Appender;

namespace Granikos.SMTPSimulator.Core.Logging
{
    public class HeaderOnceAppender : RollingFileAppender
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