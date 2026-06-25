using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entities
{
    // Composite Pattern: BacklogItem implementeert IWorkItem en kan Activities bevatten
    // Een BacklogItem is dus zowel een composite (bevat Activities) als een leaf (wanneer geen Activities)
    public class BacklogItem : IWorkItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Developer? AssignedDeveloper { get; set; }
        public List<Activity> Activities { get; set; }
        public DiscussionThread? Discussion { get; set; }
        public IBacklogItemState State { get; private set; }
        private readonly Notifier _notifier = new Notifier();
        public List<IWorkItem> WorkItems { get; set; }
        public int EffortPoints { get; set; }

        public BacklogItem(string title, string description, int effortPoints = 0)
        {
            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            Activities = new List<Activity>();
            WorkItems = new List<IWorkItem>();
            State = new TodoState();
            EffortPoints = effortPoints;
        }

        // Composite Pattern: Voeg een work item (Activity) toe
        public void AddWorkItem(IWorkItem item)
        {
            WorkItems.Add(item);
            if (item is Activity activity)
            {
                Activities.Add(activity);
            }
        }

        // Composite Pattern: Verwijder een work item
        public void RemoveWorkItem(IWorkItem item)
        {
            WorkItems.Remove(item);
            if (item is Activity activity)
            {
                Activities.Remove(activity);
            }
        }

        //Observer Pattern: Op basis van een subscriber wordt er een notificatie verstuurd.
        public void Subscribe(INotificationSubscriber subscriber) => _notifier.Subscribe(subscriber);
        public void Unsubscribe(INotificationSubscriber subscriber) => _notifier.Unsubscribe(subscriber);

        // State Pattern: De state veranderen bijvoorbeeld van todo naar doing.
        // Dit kan alleen als de CanTransitionTo method true teruggeeft.
        public void ChangeState(IBacklogItemState newState)
        {
            if (!State.CanTransitionTo(newState))
            {
                throw new InvalidOperationException($"Transition from {State.Name} to {newState.Name} is not allowed.");
            }

            // Business Rule (casus): een backlog item mag pas naar Done als alle
            // onderliggende activities Done zijn. Hier wordt de regel hard afgedwongen.
            if (newState is DoneState && !CanMarkAsDone())
            {
                throw new InvalidOperationException(
                    $"BacklogItem '{Title}' cannot move to Done: not all underlying activities are completed.");
            }

            var oldState = State;
            State = newState;
            NotifyOnTransition(oldState, newState);
        }

        // Observer Pattern: stuurt gerichte notificaties op basis van de transitie,
        // conform de casus (testers, scrum master en (lead) developer).
        private void NotifyOnTransition(IBacklogItemState from, IBacklogItemState to)
        {
            if (to is ReadyForTestingState)
            {
                // Item klaar voor test -> testers krijgen een notificatie.
                _notifier.NotifyAll(this, $"Status changed to {to.Name}: testers are notified.");
            }
            else if (to is TodoState)
            {
                // Tester constateerde dat het item toch niet klaar was: het gaat terug
                // naar Todo en de scrum master wordt geïnformeerd.
                _notifier.NotifyAll(this, $"Status changed from {from.Name} to {to.Name}: scrum master is notified.");
            }
            else if (to is DoneState)
            {
                // (Lead) developer heeft het via de definition of done goedgekeurd.
                _notifier.NotifyAll(this, $"Status changed to {to.Name}: lead developer is notified.");
            }
        }

        // Composite Pattern: Status is "Done" alleen als alle onderliggende Activities ook Done zijn
        public string GetStatus()
        {
            // Als er activities zijn, check of ze allemaal Done zijn
            if (Activities.Any())
            {
                bool allActivitiesDone = Activities.All(a => a.Status == ActivityStatus.Done);

                // BacklogItem kan alleen Done zijn als alle activities Done zijn
                if (State.Name == "Done" && !allActivitiesDone)
                {
                    return "Done (but not all activities completed)";
                }
            }

            return State?.Name ?? "Unknown";
        }

        // Composite Pattern: Bereken totale effort points inclusief alle child work items
        public int GetEffortPoints()
        {
            int total = EffortPoints;

            // Tel effort points van alle child work items op (recursief via composite pattern)
            foreach (var item in WorkItems)
            {
                total += item.GetEffortPoints();
            }

            return total;
        }

        // Business Rule: BacklogItem kan alleen naar Done als alle Activities Done zijn
        public bool CanMarkAsDone()
        {
            if (!Activities.Any())
                return true; // Geen activities, dus mag Done

            return Activities.All(a => a.Status == ActivityStatus.Done);
        }
    }

    // State design pattern. Dit is een voorbeeld van het State Pattern. Op basis van de huidige state van het BacklogItem wordt er een nieuwe state geselecteerd.
    public interface IBacklogItemState
    {
        string Name { get; }
        bool CanTransitionTo(IBacklogItemState newState);
    }

    public class TodoState : IBacklogItemState
    {
        public string Name => "Todo";
        // Aan het begin van de sprint start een item in Todo; werk starten -> Doing.
        public bool CanTransitionTo(IBacklogItemState newState) => newState is DoingState;
    }

    public class DoingState : IBacklogItemState
    {
        public string Name => "Doing";
        // Developer levert op voor test. Casus: 'terug naar doing kan niet', dus geen
        // enkele andere state mag terug naar Doing (zie de overige states).
        public bool CanTransitionTo(IBacklogItemState newState) => newState is ReadyForTestingState;
    }

    public class ReadyForTestingState : IBacklogItemState
    {
        public string Name => "ReadyForTesting";
        // Tester pakt het item op (Testing), of constateert dat het toch niet klaar is
        // en stuurt het terug naar Todo (niet naar Doing!) -> scrum master notificatie.
        public bool CanTransitionTo(IBacklogItemState newState) => newState is TestingState || newState is TodoState;
    }

    public class TestingState : IBacklogItemState
    {
        public string Name => "Testing";
        // Test slaagt -> Tested. Gaat er iets mis -> terug naar Todo (werk voor developer).
        public bool CanTransitionTo(IBacklogItemState newState) => newState is TestedState || newState is TodoState;
    }

    public class TestedState : IBacklogItemState
    {
        public string Name => "Tested";
        // (Lead) developer controleert de definition of done: akkoord -> Done,
        // niet akkoord -> terug naar ReadyForTesting voor een nieuwe testronde.
        public bool CanTransitionTo(IBacklogItemState newState) => newState is DoneState || newState is ReadyForTestingState;
    }

    public class DoneState : IBacklogItemState
    {
        public string Name => "Done";
        // Done is een eindtoestand binnen de sprintuitvoering.
        public bool CanTransitionTo(IBacklogItemState newState) => false;
    }

    public interface INotificationSubscriber
    {
        void Notify(BacklogItem item, string message);
    }

    public class Notifier
    {
        private readonly List<INotificationSubscriber> _subscribers = new List<INotificationSubscriber>();

        public void Subscribe(INotificationSubscriber subscriber)
        {
            if (!_subscribers.Contains(subscriber))
                _subscribers.Add(subscriber);
        }

        public void Unsubscribe(INotificationSubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public void NotifyAll(BacklogItem item, string message)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Notify(item, message);
            }
        }
    }

    public abstract class NotificationChannel
    {
        public abstract void Send(string message);
    }

    public class EmailChannel : NotificationChannel
    {
        public override void Send(string message)
        {
            // Simuleer e-mailnotificatie
            Console.WriteLine($"[Email] {message}");
        }
    }

    public class SlackChannel : NotificationChannel
    {
        public override void Send(string message)
        {
            // Simuleer Slack-notificatie
            Console.WriteLine($"[Slack] {message}");
        }
    }

    public class SmsChannel : NotificationChannel
    {
        public override void Send(string message)
        {
            // Simuleer sms-notificatie
            Console.WriteLine($"[SMS] {message}");
        }
    }

    // Creational design pattern. Dit is een voorbeeld van het Factory method Pattern. Op basis van een string worden er verschillende soorten NotificationChannel objecten aangemaakt
    public static class NotificationChannelFactory
    {
        public static NotificationChannel CreateChannel(string type)
        {
            return type.ToLower() switch
            {
                "email" => new EmailChannel(),
                "slack" => new SlackChannel(),
                "sms" => new SmsChannel(),
                _ => throw new ArgumentException($"Unknown channel type: {type}")
            };
        }
    }

    public class NotificationChannelSubscriber : INotificationSubscriber
    {
        private readonly NotificationChannel _channel;
        public NotificationChannelSubscriber(NotificationChannel channel)
        {
            _channel = channel;
        }
        public void Notify(BacklogItem item, string message)
        {
            _channel.Send($"BacklogItem '{item.Title}': {message}");
        }
    }
}