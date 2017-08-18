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
using System.Collections.Generic;
using System.Linq;
using Granikos.SMTPSimulator.Service.Models;
using Granikos.SMTPSimulator.Service.Models.Providers;

namespace Granikos.SMTPSimulator.Service.Database
{
    public class DefaultProvider<TEntity, TInterface> : DatabaseProvider<TEntity, int>, IDataProvider<TInterface,int>
        where TEntity : class, TInterface, new()
        where TInterface : IEntity<int>
    {
        private readonly Func<TInterface, TEntity> _converter;

        public DefaultProvider(Func<TInterface, TEntity> converter)
        {
            _converter = converter;
        }

        IEnumerable<TInterface> IDataProvider<TInterface, int>.All()
        {
            return All().Select(entity => (TInterface) entity);
        }

        IEnumerable<TInterface> IDataProvider<TInterface, int>.Paged(int page, int pageSize)
        {
            return Paged(page, pageSize).Select(entity => (TInterface)entity);
        }

        TInterface IDataProvider<TInterface, int>.Get(int id)
        {
            return Get(id);
        }

        TInterface IDataProvider<TInterface, int>.Add(TInterface entity)
        {
            return Add(_converter(entity));
        }

        TInterface IDataProvider<TInterface, int>.Update(TInterface entity)
        {
            return Update(_converter(entity));
        }

        bool IDataProvider<TInterface, int>.Validate(TInterface entity, out string message)
        {
            return Validate(_converter(entity), out message);
        }
    }
}