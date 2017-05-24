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
namespace Granikos.SMTPSimulator.Core
{
    public enum SMTPStatusCode
    {
        SyntaxError = 500,
        ParamError = 501,
        NotImplemented = 502,
        BadSequence = 503,
        ParamNotImplemented = 504,

        SystemStatus = 211,
        Help = 214,
        Ready = 220,
        Closing = 221,
        NotAvailiable = 421,

        Okay = 250,
        WillForward = 251,
        CannotVerify = 252,
        MailboxBusy = 450,
        MailboxUnavailiableError = 550,
        ProcessingError = 451,
        InsufficientStorage = 452,
        ExceededStorage = 552,
        MailboxNameNotAllowed = 553,
        StartMailInput = 354,
        TransactionFailed = 554,

        AuthContinue = 334,
        AuthTooWeak = 534,
        AuthEncryptionRequired = 538,
        AuthRequired = 530,
        AuthFailed = 535,
        AuthSuccess = 235,

        TLSNotAvailiable = 454
    }
}