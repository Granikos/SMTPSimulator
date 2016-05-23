using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using S22.Pop3;

namespace Granikos.NikosTwo.Service
{
    public static class Pop3Tester
    {
        public static Pop3TestResult Test(string host, int port, bool ssl, string user, string password,
            AuthMethod authMethod)
        {
            var result = new Pop3TestResult
            {
                ConnectSuccess = false,
                LoginSuccess = !string.IsNullOrEmpty(user) ? false : (bool?) null
            };

            try
            {
                using (var client = new Pop3Client(host, port, ssl))
                {
                    result.ConnectSuccess = true;

                    if (!string.IsNullOrEmpty(user))
                    {
                        client.Login(user, password, authMethod);
                        result.LoginSuccess = client.Authed;
                    }

                    try
                    {
                        result.Capabilities = client.Capabilities().ToArray();
                    }
                    catch (NotSupportedException)
                    {
                    }

                    client.Logout();
                }
            }
            catch (SocketException e)
            {
                result.ErrorMessage =
                    string.Format("An error occured with the underlying socket (Code {1} {2}): {0}", e.Message,
                        e.ErrorCode, e.SocketErrorCode);
            }
            catch (IOException e)
            {
                result.ErrorMessage = string.Format("An IO error occured with the connection: {0}", e.Message);
            }
            catch (BadServerResponseException e)
            {
                result.ErrorMessage =
                    string.Format("The server gave a bad response (see log for details): {0}", e.Message);
            }
            catch (InvalidCredentialsException e)
            {
                result.ErrorMessage = string.Format("The login credentials were invalid: {0}", e.Message);
            }
            catch (Exception e)
            {
                result.ErrorMessage = string.Format("An unexpected error occured: {0}", e.Message);
            }

            return result;
        }
    }
}