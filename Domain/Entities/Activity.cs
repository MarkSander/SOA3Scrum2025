using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een Activity die bij een BacklogItem hoort. 
    /// Kan sub-activiteiten bevatten (Composite Pattern).
    /// </summary>
    public enum ActivityStatus
    {
        Todo,
        Doing,
        Done
    }

    // Composite Pattern: Activity implementeert IWorkItem en kan zelf ook sub-activities bevatten
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

        // Composite Pattern: Voeg sub-activity toe
        public void Add(Activity activity) => SubActivities.Add(activity);

        // Composite Pattern: Verwijder sub-activity
        public void Remove(Activity activity) => SubActivities.Remove(activity);

        // Composite Pattern: Status geeft "Done" alleen als alle sub-activities ook Done zijn
        public string GetStatus()
        {
            if (SubActivities.Any())
            {
                bool allSubActivitiesDone = SubActivities.All(a => a.Status == ActivityStatus.Done);

                if (Status == ActivityStatus.Done && !allSubActivitiesDone)
                {
                    return "Done (but not all sub-activities completed)";
                }
            }

            return Status.ToString();
        }

        // Composite Pattern: Bereken totale effort points inclusief alle sub-activities (recursief)
        public int GetEffortPoints()
        {
            int total = EffortPoints;

            // Recursief: tel effort points van alle sub-activities op
            foreach (var subActivity in SubActivities)
            {
                total += subActivity.GetEffortPoints();
            }

            return total;
        }

        // Business Rule: Activity kan alleen Done zijn als alle sub-activities Done zijn
        public bool CanMarkAsDone()
        {
            if (!SubActivities.Any())
                return true; // Geen sub-activities, dus mag Done

            return SubActivities.All(a => a.Status == ActivityStatus.Done);
        }

        // Helper method om status te zetten met validatie
        public void SetStatus(ActivityStatus newStatus)
        {
            if (newStatus == ActivityStatus.Done && !CanMarkAsDone())
            {
                throw new InvalidOperationException(
                    $"Cannot mark activity '{Name}' as Done: not all sub-activities are completed.");
            }

            Status = newStatus;
        }
    }
}