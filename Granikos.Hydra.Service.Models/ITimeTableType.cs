using System;
using System.Collections.Generic;

namespace Granikos.Hydra.Service.Models
{
    public interface ITimeTableType
    {
        IDictionary<string, string> Parameters { get; set; }

        IDictionary<string, string> Data { get; }

        DateTime GetNextMailTime();

        bool ValidateParameters(out string message);

        void Initialize();
    }
}