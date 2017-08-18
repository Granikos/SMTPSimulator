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
using System.Xml.Serialization;

namespace Granikos.SMTPSimulator.Service.Models
{
    public interface IMailTemplate : IEntity<int>
    {
        string Title { get; set; }
        string Subject { get; set; }
        EncodingType HeaderEncoding { get; set; }
        EncodingType SubjectEncoding { get; set; }
        EncodingType BodyEncoding { get; set; }
        string Html { get; set; }
        string Text { get; set; }
        MailTemplateTypeBehaviour Behaviour { get; set; }
    }

    [Serializable]
    [XmlType(Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    public enum EncodingType
    {
        ASCII, UTF8, UTF32
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/MailTemplateSchema.xsd")]
    public enum MailTemplateTypeBehaviour
    {
        SMTPCompliant
    }
}