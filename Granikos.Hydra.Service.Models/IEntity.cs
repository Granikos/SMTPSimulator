using System;
using System.ComponentModel.DataAnnotations;

namespace Granikos.NikosTwo.Service.Models
{
    public interface IEntity<TKey>
        where TKey : IComparable<TKey>
    {
        [Key]
        TKey Id { get; set; }
    }
}