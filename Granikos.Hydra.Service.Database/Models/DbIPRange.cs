using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Net;
using Granikos.Hydra.Service.Models;

namespace Granikos.Hydra.Service.Database.Models
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
            Contract.Requires<ArgumentNullException>(start != null);
            Contract.Requires<ArgumentNullException>(end != null);
            Contract.Requires<ArgumentException>(start.AddressFamily == end.AddressFamily);

            Start = start;
            End = end;
        }

        [NotMapped]
        public IPAddress Start
        {
            get { return _start; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(End == null || value.AddressFamily == End.AddressFamily);

                _start = value;
            }
        }

        [NotMapped]
        public IPAddress End
        {
            get { return _end; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null, "value");
                Contract.Requires<ArgumentException>(Start == null || value.AddressFamily == Start.AddressFamily);

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