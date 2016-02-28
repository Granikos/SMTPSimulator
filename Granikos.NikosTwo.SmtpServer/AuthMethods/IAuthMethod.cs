namespace Granikos.NikosTwo.SmtpServer.AuthMethods
{
    public interface IAuthMethod
    {
        bool ProcessResponse(SMTPTransaction transaction, string response, out string challenge);
        void Abort(SMTPTransaction transaction);
    }
}