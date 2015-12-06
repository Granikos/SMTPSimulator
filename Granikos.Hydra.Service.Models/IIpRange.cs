using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Granikos.Hydra.Service.Models
{
    public interface IIpRange
    {
        IPAddress Start { get; set; }
        IPAddress End { get; set; }
    }
}
