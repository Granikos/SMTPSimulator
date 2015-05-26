using System;
using System.ComponentModel.Composition;

namespace HydraCore.AuthMethods
{
    [ExportMetadata("Name", "PLAIN")]
    [Export(typeof (IAuthMethod))]
    public class PlainAuthMethod : IAuthMethod
    {
        public bool ProcessResponse(SMTPTransaction transaction, string response, out string challenge)
        {
            if (String.IsNullOrEmpty(response))
            {
                challenge = null;
                return false;
            }

            var parts = response.Split('\0');

            if (parts.Length != 3)
            {
                challenge = null;
                return false;
            }

            var username = parts[1];
            var password = parts[2];

            transaction.SetProperty("Username", username, true);
            transaction.SetProperty("Password", password, true);

            challenge = null;

            // TODO
            // var password = response;
            // var username = transaction.GetProperty<string>("Username");

            return true;
        }

        public void Abort(SMTPTransaction transaction)
        {
            transaction.SetProperty("Username", null, true);
            transaction.SetProperty("Password", null, true);
        }
    }
}