using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public class DbIPRange : IIpRange
    {
        [Key]
        public int Id { get; set; }

        public ReceiveConnector Connector { get; set; }
        private IPAddress _start;
        private IPAddress _end;

        protected DbIPRange()
        {
            
        }

        public DbIPRange(IPAddress start, IPAddress end)
        {
            if (start == null) throw new ArgumentNullException();
            if (end == null) throw new ArgumentNullException();
            if (!(start.AddressFamily == end.AddressFamily)) throw new ArgumentException();

            Start = start;
            End = end;
        }

        [NotMapped]
        public IPAddress Start
        {
            get { return _start; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(End == null || value.AddressFamily == End.AddressFamily)) throw new ArgumentException();

                _start = value;
            }
        }

        [NotMapped]
        public IPAddress End
        {
            get { return _end; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (!(Start == null || value.AddressFamily == Start.AddressFamily)) throw new ArgumentException();

                _end = value;
            }
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