using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Granikos.NikosTwo.Service.Retention
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