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
using System.Net;

namespace Granikos.SMTPSimulator.Service.Models
{
    public static class ModelHelpers
    {
        public static void CopyTo(this IMailTemplate source, IMailTemplate target)
        {
            target.Id = source.Id;
            target.Behaviour = source.Behaviour;
            target.BodyEncoding = source.BodyEncoding;
            target.HeaderEncoding = source.HeaderEncoding;
            target.SubjectEncoding = source.SubjectEncoding;
            target.Html = source.Html;
            target.Subject = source.Subject;
            target.Text = source.Text;
            target.Title = source.Title;
        }

        public static T ConvertTo<T>(this IMailTemplate source)
            where T : IMailTemplate, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this IUserGroup source, IUserGroup target)
        {
            target.Id = source.Id;
            target.Name = source.Name;
            target.UserIds = source.UserIds;
        }

        public static T ConvertTo<T>(this IUserGroup source)
            where T : IUserGroup, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this IUser source, IUser target)
        {
            target.Id = source.Id;
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            target.Mailbox = source.Mailbox;
        }

        public static T ConvertTo<T>(this IUser source)
            where T : IUser, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this IReceiveConnector source, IReceiveConnector target)
        {
            target.Id = source.Id;
            target.Address = source.Address;
            target.AuthPassword = source.AuthPassword;
            target.AuthUsername = source.AuthUsername;
            target.Banner = source.Banner;
            target.Enabled = source.Enabled;
            target.GreylistingTime = source.GreylistingTime;
            target.Name = source.Name;
            target.Port = source.Port;
            target.RemoteIPRanges = source.RemoteIPRanges;
            target.RequireAuth = source.RequireAuth;
            target.TLSSettings = source.TLSSettings; // TODO
        }

        public static T ConvertTo<T>(this IReceiveConnector source)
            where T : IReceiveConnector, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this ISendConnector source, ISendConnector target)
        {
            target.Id = source.Id;
            target.Domains = source.Domains; // TODO
            target.LocalAddress = source.LocalAddress;
            target.RemoteAddress = source.RemoteAddress;
            target.RemotePort = source.RemotePort;
            target.RetryCount = source.RetryCount;
            target.RetryTime = source.RetryTime;
            target.Name = source.Name;
            target.UseSmarthost = source.UseSmarthost;
            target.Password = source.Password;
            target.Username = source.Username;
            target.UseAuth = source.UseAuth;
            target.TLSSettings = source.TLSSettings; // TODO
        }

        public static T ConvertTo<T>(this ISendConnector source)
            where T : ISendConnector, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this ITimeTable source, ITimeTable target)
        {
            target.Id = source.Id;
            target.Type = source.Type;
            target.Active = source.Active;
            target.AttachmentName = source.AttachmentName;
            target.AttachmentType = source.AttachmentType;
            target.MailTemplateId = source.MailTemplateId;
            target.MailsError = source.MailsError;
            target.MailsSuccess = source.MailsSuccess;
            target.Name = source.Name;
            target.MinRecipients = source.MinRecipients;
            target.MaxRecipients = source.MaxRecipients;
            target.Parameters = source.Parameters;
            target.SendEicarFile = source.SendEicarFile;
            target.ReportType = source.ReportType;
            target.ProtocolLevel = source.ProtocolLevel;
            target.ReportMailAddress = source.ReportMailAddress;
            target.RecipientGroupId = source.RecipientGroupId;
            target.SenderGroupId = source.SenderGroupId;
            target.SenderMailbox = source.SenderMailbox;
            target.RecipientMailbox = source.RecipientMailbox;
            target.StaticRecipient = source.StaticRecipient;
            target.StaticSender = source.StaticSender;
            target.ActiveSince = source.ActiveSince;
            target.ActiveUntil = source.ActiveUntil;
            target.ReportDay = source.ReportDay;
            target.ReportHour = source.ReportHour;
        }

        public static T ConvertTo<T>(this ITimeTable source)
            where T : ITimeTable, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static void CopyTo(this IAttachment source, IAttachment target)
        {
            target.Name = source.Name;
            target.Size = source.Size;
            target.Content = source.Content;
        }

        public static T ConvertTo<T>(this IAttachment source)
            where T : IAttachment, new()
        {
            var target = new T();

            source.CopyTo(target);

            return target;
        }

        public static bool Contains(this IIpRange range, IPAddress address)
        {
            if (address.AddressFamily != range.Start.AddressFamily)
            {
                return false;
            }

            var lowerBytes = range.Start.GetAddressBytes();
            var upperBytes = range.End.GetAddressBytes();
            var addressBytes = address.GetAddressBytes();

            bool lowerBoundary = true, upperBoundary = true;

            for (var i = 0;
                i < lowerBytes.Length &&
                (lowerBoundary || upperBoundary);
                i++)
            {
                if ((lowerBoundary && addressBytes[i] < lowerBytes[i]) ||
                    (upperBoundary && addressBytes[i] > upperBytes[i]))
                {
                    return false;
                }

                lowerBoundary &= (addressBytes[i] == lowerBytes[i]);
                upperBoundary &= (addressBytes[i] == upperBytes[i]);
            }

            return true;
        }
    }
}