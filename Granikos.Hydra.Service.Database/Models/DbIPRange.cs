using System.Net;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Database.Models
{
    public class DbIPRange : IPRange
    {
        public DbIPRange(IPAddress start, IPAddress end) : base(start, end)
        {
        }

        public string StartString
        {
            get { return Start.ToString(); }
            set { Start = IPAddress.Parse(value); }
        }

        public string EndString
        {
            get { return End.ToString(); }
            set { End = IPAddress.Parse(value); }
        }

        public static DbIPRange FromOther(IIpRange range)
        {
            return new DbIPRange(range.Start, range.End);
        }
    }
}