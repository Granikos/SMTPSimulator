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
namespace Granikos.SMTPSimulator.Core.Logging
{
    public static class LogEnumExtensions
    {
        public static string GetSymbol(this LogPartType part)
        {
            switch (part)
            {
                case LogPartType.Client:
                    return "C";
                case LogPartType.Server:
                    return "S";
                case LogPartType.Other:
                    return "O";
                default:
                    return null;
            }
        }

        public static string GetSymbol(this LogEventType type)
        {
            switch (type)
            {
                case LogEventType.Connect:
                    return "+";
                case LogEventType.Disconnect:
                    return "-";
                case LogEventType.Incoming:
                    return "<";
                case LogEventType.Outgoing:
                    return ">";
                case LogEventType.Certificate:
                    return "#";
                case LogEventType.Other:
                    return "*";
                default:
                    return null;
            }
        }

        public static bool IsConnectionEvent(this LogEventType type)
        {
            switch (type)
            {
                case LogEventType.Connect:
                    return true;
                case LogEventType.Disconnect:
                    return true;
                default:
                    return false;
            }
        }
    }
}