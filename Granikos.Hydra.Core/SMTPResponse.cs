using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Granikos.Hydra.Core
{
    public sealed class SMTPResponse
    {
        public string[] Args;
        public SMTPStatusCode Code;

        public SMTPResponse(SMTPStatusCode code, params string[] args)
        {
            Contract.Requires<ArgumentNullException>(args != null);

            Code = code;
            Args = args;
        }

        public override string ToString()
        {
            var code = ((int) Code).ToString();
            if (Args.Length > 1)
            {
                var sep = string.Format("\r\n{0}", code);
                var response = code + "-" + string.Join(sep + "-", Args.Take(Args.Length - 1));

                response += sep + " " + Args.Last();

                return response;
            }

            return string.Format("{0} {1}", (int) Code, Args.Length > 0 ? Args[0] : Code.ToString());
        }
    }
}