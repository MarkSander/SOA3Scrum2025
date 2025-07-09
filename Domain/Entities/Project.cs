using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een Scrum project.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Unieke identificatie van het project.
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Naam van het project.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Eigenaar van het project.
        /// </summary>
        public ProductOwner ProductOwner { get; set; }
        /// <summary>
        /// Sprints die bij dit project horen.
        /// </summary>
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();

        public Project(string name, ProductOwner productOwner)
        {
            Id = Guid.NewGuid();
            Name = name;
            ProductOwner = productOwner;
            Sprints = new List<Sprint>();
        }
    }
}