using System;
using System.Collections.Generic;

namespace HydraCore
{
    public interface IModuleLoader<T>
        where T : class 
    {
        IEnumerable<Tuple<string, T>> GetModules();
    }
}