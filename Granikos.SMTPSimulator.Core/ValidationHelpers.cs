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
using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Granikos.SMTPSimulator.Core
{
    public static class ValidationHelpers
    {
        private static readonly Regex DomainRegex = new Regex(@"^([a-z](\-?[a-z0-9]+)*\.)+[a-z](\-?[a-z0-9]+)*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidDomainName(this string domain)
        {
            if (domain == null) throw new ArgumentNullException();
            return DomainRegex.IsMatch(domain);
        }

        public static bool IsValidAddressLiteral(this string address)
        {
            if (address == null) throw new ArgumentNullException();
            IPAddress ip;
            if (address.StartsWith("IPv6:"))
            {
                address = address.Substring(5);

                if (!IPAddress.TryParse(address, out ip)) return false;

                return ip.GetAddressBytes().Length > 4;
            }

            if (!IPAddress.TryParse(address, out ip)) return false;

            return ip.GetAddressBytes().Length == 4;
        }

        public static bool IsValidDomain(this string domain)
        {
            if (domain == null) throw new ArgumentNullException();
            if (IsValidDomainName(domain)) return true;

            if (domain.StartsWith("[") && domain.EndsWith("]"))
            {
                return IsValidAddressLiteral(domain.Substring(1, domain.Length - 2));
            }

            return false;
        }
    }
}