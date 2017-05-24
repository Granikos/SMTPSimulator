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
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Granikos.SMTPSimulator.Service.Retention
{
    public class DirectoryRetentionConfigCollection : ConfigurationElementCollection
    {
        protected override string ElementName
        {
            get { return "Directory"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public DirectoryRetentionConfigElement this[int index]
        {
            get
            {
                return BaseGet(index) as DirectoryRetentionConfigElement;
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public new DirectoryRetentionConfigElement this[string dir]
        {
            get { return (DirectoryRetentionConfigElement)BaseGet(dir); }
            set
            {
                if (BaseGet(dir) != null)
                {
                    BaseRemoveAt(BaseIndexOf(BaseGet(dir)));
                }
                BaseAdd(value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DirectoryRetentionConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DirectoryRetentionConfigElement)element).Directory;
        }
    }
}