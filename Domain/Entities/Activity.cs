using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een Activity die bij een BacklogItem hoort. Kan sub-activiteiten bevatten (Composite Pattern).
    /// </summary>
    public enum ActivityStatus
    {
        Todo,
        Doing,
        Done
    }

    public class Activity : IWorkItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ActivityStatus Status { get; set; }
        public Developer? AssignedDeveloper { get; set; }
        public List<Activity> SubActivities { get; set; } = new List<Activity>();
        public int EffortPoints { get; set; }

        public Activity(string name, int effortPoints = 0)
        {
            Id = Guid.NewGuid();
            Name = name;
            Status = ActivityStatus.Todo;
            EffortPoints = effortPoints;
        }

        public void Add(Activity activity) => SubActivities.Add(activity);
        public void Remove(Activity activity) => SubActivities.Remove(activity);

        public string GetStatus() => Status.ToString();
        public int GetEffortPoints() => EffortPoints;
    }
}