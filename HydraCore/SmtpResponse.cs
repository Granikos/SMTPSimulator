using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace HydraCore
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
                var sep = String.Format("\r\n{0}", code);
                var response = code + "-" + String.Join(sep + "-", Args.Take(Args.Length - 1));

                response += sep + " " + Args.Last();

                return response;
            }

            return String.Format("{0} {1}", (int) Code, Args.Length > 0 ? Args[0] : Code.ToString());
        }
    }
}