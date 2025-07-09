using System;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een gebruiker in het systeem.
    /// </summary>
    public abstract class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        protected User(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }

    public class Developer : User
    {
        public Developer(string name) : base(name) { }
    }

    public class ScrumMaster : User
    {
        public ScrumMaster(string name) : base(name) { }
    }

    public class ProductOwner : User
    {
        public ProductOwner(string name) : base(name) { }
    }
}