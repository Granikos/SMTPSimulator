using System.Runtime.Serialization;

namespace Granikos.Hydra.Service.Models
{
    [DataContract(Name = "{0}WithCount")]
    public class ValueWithCount<TValue>
    {
        public ValueWithCount(TValue value, int count)
        {
            Value = value;
            Count = count;
        }

        [DataMember]
        public TValue Value { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}