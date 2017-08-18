// MIT License
// 
// Copyright (c) 2017 Granikos GmbH & Co. KG (https://www.granikos.eu)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System.ComponentModel.Composition;

namespace Granikos.SMTPSimulator.SmtpServer.AuthMethods
{
    [ExportMetadata("Name", "PLAIN")]
    [Export(typeof (IAuthMethod))]
    public class PlainAuthMethod : IAuthMethod
    {
        public bool ProcessResponse(SMTPTransaction transaction, string response, out string challenge)
        {
            if (string.IsNullOrEmpty(response))
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