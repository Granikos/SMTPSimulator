using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Granikos.NikosTwo.Service.ConfigurationService.Models;
using S22.Imap;

namespace Granikos.NikosTwo.Service
{
    public static class ImapTester
    {
        private static readonly StringBuilderTraceListener Listener = new StringBuilderTraceListener();

        private static readonly object ListenerLock = new object();

        static ImapTester()
        {
            var field = typeof(ImapClient).GetField("ts", BindingFlags.NonPublic | BindingFlags.Static);

            if (field != null)
            {
                var ts = (TraceSource) field.GetValue(null);

                ts.Listeners.Add(Listener);
                ts.Switch.Level = SourceLevels.All;
            }
        }

        public static ImapTestResult Test(string host, int port, bool ssl, string user, string password,
            AuthMethod authMethod)
        {
            lock (ListenerLock)
            {
                Listener.Clear();

                var result = new ImapTestResult
                {
                    ConnectSuccess = false,
                    LoginSuccess = !string.IsNullOrEmpty(user)? false : (bool?) null
                };

                try
                {
                    using (var client = new ImapClient(host, port, ssl))
                    {
                        result.ConnectSuccess = true;
                        result.PreAuthCapabilities = client.Capabilities().ToArray();

                        if (!string.IsNullOrEmpty(user))
                        {
                            client.Login(user, password, authMethod);
                            result.LoginSuccess = client.Authed;

                            if (client.Authed)
                            {
                                result.PostAuthCapabilities = client.Capabilities().ToArray();
                            }
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

                result.ProtocolLog = Listener.GetLog();

                return result;
            }
        }

        private class StringBuilderTraceListener : TraceListener
        {
            private readonly StringBuilder _sb = new StringBuilder();

            public override void Write(string message)
            {
            }

            public override void WriteLine(string message)
            {
                _sb.AppendLine(message.Replace("S -> ", "> ").Replace("C -> ", "< "));
            }

            public string GetLog()
            {
                return _sb.ToString();
            }

            public void Clear()
            {
                _sb.Clear();
            }
        }
    }
}