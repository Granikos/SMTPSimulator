using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract(Name = "{0}WithCount")]
    public class ValueWithCount<TValue>
    {
        private readonly TValue _value;
        private readonly int _count;

        public ValueWithCount(TValue value, int count)
        {
            _value = value;
            _count = count;
        }

        [DataMember]
        public TValue Value
        {
            get { return _value; }
            set { }
        }

        [DataMember]
        public int Count
        {
            get { return _count; }
            set { }
        }
    }
}