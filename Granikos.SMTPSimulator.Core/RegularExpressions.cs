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
using System.Text.RegularExpressions;

namespace Granikos.SMTPSimulator.Core
{
    public static class RegularExpressions
    {
        public const string DomainNamePattern = @"(?i)([a-z](\-?[a-z0-9]+)*\.)+[a-z](\-?[a-z0-9]+)*(?-i)";
        public const string DomainPattern = @"(?:\[[^\]]+\]|" + DomainNamePattern + ")";
        public const string QuotedStringPattern = @"""(?:\\.|[^""\\]+)*""";
        public const string DotStringPattern = @"\w+(?:\.\w+)*";
        public const string AtDomainList = "@" + DomainPattern + "(,@" + DomainPattern + ")*";

        public const string MailboxPattern =
            "(?<LocalPart>" + DotStringPattern + "|" + QuotedStringPattern + ")@(?<Domain>" + DomainPattern + ")";

        public const string PathPattern = "<(?:(?<AtDomains>" + AtDomainList + "):)?" + MailboxPattern + ">";
        public static readonly Regex PathRegex = new Regex("^" + PathPattern + "$", RegexOptions.Compiled);
        public static readonly Regex DotStringRegex = new Regex("^" + DotStringPattern + "$", RegexOptions.Compiled);
    }
}