using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Granikos.Hydra.Service.Providers
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