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
using System.Linq;

namespace Granikos.SMTPSimulator.Core
{
    public sealed class SMTPResponse
    {
        public string[] Args;
        public SMTPStatusCode Code;

        public SMTPResponse(SMTPStatusCode code, params string[] args)
        {
            if (args == null) throw new ArgumentNullException();

            Code = code;
            Args = args;
        }

        public override string ToString()
        {
            var code = ((int) Code).ToString();
            if (Args.Length > 1)
            {
                var sep = string.Format("\r\n{0}", code);
                var response = code + "-" + string.Join(sep + "-", Args.Take(Args.Length - 1));

                response += sep + " " + Args.Last();

                return response;
            }

            return string.Format("{0} {1}", (int) Code, Args.Length > 0 ? Args[0] : Code.ToString());
        }
    }
}