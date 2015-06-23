using System;

namespace HydraService.Models
{
    public interface IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        TKey Id { get; set; }
    }
}