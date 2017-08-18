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
using System.Data.Entity;
using System.Linq;
using Granikos.SMTPSimulator.Service.Database.Models;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database.Providers
{
    [Export(typeof (IReceiveConnectorProvider))]
    public class ReceiveConnectorProvider : DefaultProvider<ReceiveConnector,IReceiveConnector>, IReceiveConnectorProvider<ReceiveConnector>, IReceiveConnectorProvider
    {
        public ReceiveConnectorProvider() : base(ReceiveConnector.FromOther)
        {
        }

        public ReceiveConnector GetEmptyConnector()
        {
            return new ReceiveConnector();
        }

        IReceiveConnector IReceiveConnectorProvider<IReceiveConnector>.GetEmptyConnector()
        {
            return GetEmptyConnector();
        }

        protected override void OnUpdate(ReceiveConnector entity, ReceiveConnector dbEntity)
        {
            foreach (var range in dbEntity.RemoteIPRanges.ToArray())
            {
                Database.Entry(range).State = EntityState.Deleted;
            }

            dbEntity.RemoteIPRanges.Clear();

            foreach (var range in entity.RemoteIPRanges)
            {
                dbEntity.RemoteIPRanges.Add(new DbIPRange(range.Start, range.End));
            }
        }
    }
}