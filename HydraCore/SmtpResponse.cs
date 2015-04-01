using System;
using System.Linq;

namespace HydraCore
{
    public enum SMTPStatusCode
    {
        SyntaxError = 500,
        ParamError = 501,
        NotImplemented = 502,
        BadSequence = 503,
        ParamNotImplemented = 504,

        SystemStatus = 211,
        Help = 214,
        Ready = 220,
        Closing = 221,
        NotAvailiable = 421,

        Okay = 250,
        WillForward = 251,
        CannotVerify = 252,
        MailboxBusy = 450,
        MailboxUnavailiableError = 550,
        ProcessingError = 451,
        InsufficientStorage = 452,
        ExceededStorage = 552,
        MailboxNameNotAllowed = 553,
        StartMailInput = 354,
        TransactionFailed = 554
    }


    public sealed class SMTPResponse
    {
        public SMTPResponse(SMTPStatusCode code, params string[] args)
        {
            Code = code;
            Args = args ?? new string[0];
        }

        public SMTPStatusCode Code;
        public string[] Args;

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

            return String.Format("{0} {1}", (int)Code, Args.Length > 0 ? Args[0] : Code.ToString());
        }
    }
}
