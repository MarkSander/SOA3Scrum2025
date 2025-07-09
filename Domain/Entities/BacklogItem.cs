using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    // Vertegenwoordigt een BacklogItem binnen een Sprint.
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

        //Dit is een voorbeeld van het Observer Pattern. Op basis van een subscriber wordt er een notificatie verstuurd.
        public void Subscribe(INotificationSubscriber subscriber) => _notifier.Subscribe(subscriber);
        public void Unsubscribe(INotificationSubscriber subscriber) => _notifier.Unsubscribe(subscriber);

        // de state veranderen bijvoorbeeld van doing naar doen. dit kan alleen als de CanTransitionTo method true teruggeeft.
        public void ChangeState(IBacklogItemState newState)
        {
            if (State.CanTransitionTo(newState))
            {
                var oldState = State.Name;
                State = newState;
                if (State.Name == "ReadyForTesting" || State.Name == "Done" || State.Name == "Todo")
                {
                    _notifier.NotifyAll(this, $"Status changed from {oldState} to {State.Name}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Transition from {State.Name} to {newState.Name} is not allowed.");
            }
        }

        public string GetStatus() => State?.Name ?? "Unknown";
        public int GetEffortPoints()
        {
            int total = EffortPoints;
            foreach (var item in WorkItems)
                total += item.GetEffortPoints();
            return total;
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
        public bool CanTransitionTo(IBacklogItemState newState) => newState is DoingState;
    }

    public class DoingState : IBacklogItemState
    {
        public string Name => "Doing";
        public bool CanTransitionTo(IBacklogItemState newState) => newState is ReadyForTestingState || newState is TodoState;
    }

    public class ReadyForTestingState : IBacklogItemState
    {
        public string Name => "ReadyForTesting";
        public bool CanTransitionTo(IBacklogItemState newState) => newState is TestingState || newState is DoingState;
    }

    public class TestingState : IBacklogItemState
    {
        public string Name => "Testing";
        public bool CanTransitionTo(IBacklogItemState newState) => newState is TestedState || newState is ReadyForTestingState;
    }

    public class TestedState : IBacklogItemState
    {
        public string Name => "Tested";
        public bool CanTransitionTo(IBacklogItemState newState) => newState is DoneState || newState is TestingState;
    }

    public class DoneState : IBacklogItemState
    {
        public string Name => "Done";
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

    // Creational design pattern. Dit is een voorbeeld van het Factory method Pattern. Op basis van een string worden er verschillende soorten NotificationChannel objecten aangemaakt
    public class NotificationChannelFactory
    {
        public static NotificationChannel CreateChannel(string type)
        {
            return type.ToLower() switch
            {
                "email" => new EmailChannel(),
                "slack" => new SlackChannel(),
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