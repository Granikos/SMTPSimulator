using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Granikos.NikosTwo.Service.Models
{
    public interface ITimeTableType
    {
        IDictionary<string, string> Parameters { get; set; }

        IDictionary<string, string> Data { get; }

        DateTime GetNextMailTime();

        bool ValidateParameters(out string message);

        void Initialize();

        ReadOnlyDictionary<string, string> InitialParameters { get; }
    }
}