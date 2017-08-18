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
using System.Text.RegularExpressions;
using Granikos.SMTPSimulator.Core;

namespace Granikos.SMTPSimulator.SmtpServer.CommandHandlers
{
    [RequiresAuth]
    [ExportMetadata("Command", "MAIL")]
    [Export(typeof (ICommandHandler))]
    public class MAILHandler : CommandHandlerBase
    {
        private static readonly Regex FromRegex =
            new Regex("^FROM:(" + RegularExpressions.PathPattern + "|<>)(?: (?<Params>.*))?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override SMTPResponse DoExecute(SMTPTransaction transaction, string parameters)
        {
            if (transaction.GetProperty<bool>("MailInProgress"))
            {
                return new SMTPResponse(SMTPStatusCode.BadSequence);
            }

            var match = FromRegex.Match(parameters);

            if (!match.Success)
            {
                return new SMTPResponse(SMTPStatusCode.SyntaxError);
            }

            var path = match.Groups[1].Value.Equals("<>") ? MailPath.Empty : MailPath.FromMatch(match);

            transaction.SetProperty("ReversePath", path);
            transaction.SetProperty("MailInProgress", true);

            return new SMTPResponse(SMTPStatusCode.Okay);
        }
    }
}