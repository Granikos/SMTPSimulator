using System;

namespace Granikos.Hydra.Service.Models
{
    public interface IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        TKey Id { get; set; }
    }
}