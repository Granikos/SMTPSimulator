using System.ComponentModel.Composition;

namespace Granikos.NikosTwo.SmtpServer.AuthMethods
{
    [ExportMetadata("Name", "LOGIN")]
    [Export(typeof (IAuthMethod))]
    public class LoginAuthMethod : IAuthMethod
    {
        public bool ProcessResponse(SMTPTransaction transaction, string response, out string challenge)
        {
            if (response == null)
            {
                challenge = "Username:";
                return true;
            }

            if (!transaction.HasProperty("Username"))
            {
                transaction.SetProperty("Username", response, true);
                challenge = "Password:";
                return true;
            }

            transaction.SetProperty("Password", response, true);
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