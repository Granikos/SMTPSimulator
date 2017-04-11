using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;
using Granikos.SMTPSimulator.Service.Models;

namespace Granikos.SMTPSimulator.Service.Database.Models
{
    public abstract class Entity : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        private ObjectContext GetObjectContextFromEntity()
        {
            var field = GetType().GetField("_entityWrapper");

            if (field == null)
                return null;

            var wrapper = field.GetValue(this);
            var property = wrapper.GetType().GetProperty("Context");
            var context = (ObjectContext)property.GetValue(wrapper, null);

            return context;
        }
    }
}