using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een Sprint binnen een project.
    /// </summary>
    public class Sprint
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<BacklogItem> BacklogItems { get; set; } = new List<BacklogItem>();
        public SprintStatus Status { get; set; }
        public Pipeline? Pipeline { get; set; }

        public Sprint(string name, DateTime startDate, DateTime endDate)
        {
            Id = Guid.NewGuid();
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            Status = SprintStatus.Planned;
            BacklogItems = new List<BacklogItem>();
        }
    }

    public enum SprintStatus
    {
        Planned,
        Active,
        Completed
    }
}