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
using System.Linq;
using System.Text;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [ExportMetadata("Command", "DATA")]
    [Export(typeof (ICommandHandler))]
    public class DATAHandler : CommandHandlerBase
    {
        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var forwardPath = transaction.GetListProperty<MailPath>("ForwardPath");

            if (!transaction.GetProperty<bool>("MailInProgress") || forwardPath == null || !forwardPath.Any())
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            transaction.StartDataMode(DataLineHandler, data => DataHandler(transaction, data));

            return new SMTPResponse(SMTPStatusCode.StartMailInput);
        }

        public static bool DataLineHandler(string line, StringBuilder builder)
        {
            if (line.Equals(".")) return false;
            if (line.StartsWith(".")) line = line.Substring(1);

            if (builder.Length > 0) builder.Append("\r\n");

            builder.Append(line);

            return true;
        }

        public static SMTPResponse DataHandler(SMTPTransaction transaction, string data)
        {
            transaction.Server.TriggerNewMessage(transaction, transaction.GetProperty<MailPath>("ReversePath"),
                transaction.GetListProperty<MailPath>("ForwardPath").ToArray(), data);

            transaction.Reset();

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}