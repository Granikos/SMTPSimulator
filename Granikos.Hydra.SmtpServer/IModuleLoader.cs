using System;
using System.Collections.Generic;

namespace Granikos.NikosTwo.SmtpServer
{
    public interface IModuleLoader<T>
        where T : class
    {
        IEnumerable<Tuple<string, T>> GetModules();
    }
}