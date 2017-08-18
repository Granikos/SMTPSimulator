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
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Security;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<ServiceDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ServiceDbContext context)
        {
            if (!context.SendConnectors.Any())
            {
                context.SendConnectors.Add(new SendConnector
                {
                    Name = "Default",
                    UseSmarthost = false,
                    RetryTime = TimeSpan.FromMinutes(5),
                    RetryCount = 3,
                    Default = true
                });
            }

            if (!context.ReceiveConnectors.Any())
            {
                context.ReceiveConnectors.Add(new ReceiveConnector
                {
                    Name = "Default",
                    Enabled = true,
                    Address = IPAddress.Parse("0.0.0.0"),
                    Port = 25,
                    Banner = "nikos two ready DEFAULT",
                    TLSSettings = new TLSSettings
                    {
                        EncryptionPolicy = EncryptionPolicy.NoEncryption
                    },
                    GreylistingTime = TimeSpan.FromMinutes(15)
                });
            }
        }
    }
}