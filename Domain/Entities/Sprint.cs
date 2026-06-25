using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Type van een sprint (casus): een sprint mikt op een review met de product owner
    /// en stakeholders, of op een release (deployment) van de software.
    /// </summary>
    public enum SprintType
    {
        Review,
        Release
    }

    /// <summary>
    /// Vertegenwoordigt een Sprint binnen een project (State Pattern).
    /// De sprint doorloopt verschillende stadia met per stadium eigen spelregels:
    /// in 'Planned' is hij wijzigbaar, tijdens uitvoering/pipeline niet, en de
    /// afronding verschilt voor review- en release-sprints.
    /// </summary>
    public class Sprint
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public SprintType Type { get; private set; }
        public List<BacklogItem> BacklogItems { get; } = new List<BacklogItem>();
        public Pipeline? Pipeline { get; set; }

        // State Pattern: de huidige toestand van de sprint.
        public ISprintState State { get; private set; }
        public string StatusName => State.Name;

        // Casus: een review-sprint kan pas worden afgesloten als er een
        // samenvatting van de review als document is geüpload.
        public string? ReviewSummary { get; private set; }
        public bool HasReviewSummary => !string.IsNullOrWhiteSpace(ReviewSummary);

        // Notificatiekanalen (e-mail, Slack, sms, ...) naar product owner / scrum master.
        public List<NotificationChannel> NotificationChannels { get; } = new List<NotificationChannel>();

        public Sprint(string name, DateTime startDate, DateTime endDate, SprintType type = SprintType.Review)
        {
            Id = Guid.NewGuid();
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
            Type = type;
            State = new PlannedState();
        }

        // --- Publieke API: delegeert naar de huidige state (State Pattern) ---

        public void AddBacklogItem(BacklogItem item) => State.AddBacklogItem(this, item);

        /// <summary>Eigenschappen wijzigen mag alleen zolang de sprint nog 'Planned' is.</summary>
        public void Reschedule(DateTime startDate, DateTime endDate)
        {
            EnsureEditable();
            StartDate = startDate;
            EndDate = endDate;
        }

        public void Rename(string name)
        {
            EnsureEditable();
            Name = name;
        }

        public void Start() => State.Start(this);
        public void Finish() => State.Finish(this);
        public void StartRelease() => State.StartRelease(this);
        public void CloseAfterReview() => State.CloseAfterReview(this);
        public void Cancel() => State.Cancel(this);

        /// <summary>Upload de reviewsamenvatting; alleen relevant voor een afgeronde review-sprint.</summary>
        public void UploadReviewSummary(string document) => State.UploadReviewSummary(this, document);

        // --- Interne helpers die door de states gebruikt worden ---

        internal void SetState(ISprintState state) => State = state;
        internal void AddItemInternal(BacklogItem item) => BacklogItems.Add(item);
        internal void SetReviewSummary(string document) => ReviewSummary = document;

        internal void Notify(string message)
        {
            foreach (var channel in NotificationChannels)
            {
                channel.Send(message);
            }
        }

        /// <summary>
        /// Voert het releaseproces uit via de gekoppelde development pipeline.
        /// Succes -> sprint gereleased en gesloten (notificatie PO + SM).
        /// Fout   -> scrum master krijgt melding en kan opnieuw proberen of annuleren.
        /// </summary>
        internal void RunReleasePipeline()
        {
            if (Type != SprintType.Release)
            {
                throw new InvalidOperationException("Only a release sprint can run a release pipeline.");
            }
            if (Pipeline == null)
            {
                throw new InvalidOperationException("No development pipeline is configured for this release sprint.");
            }

            // Tijdens het uitvoeren van de pipeline kan de sprint niet gewijzigd worden.
            SetState(new ReleasingState());
            bool success = Pipeline.Run();

            if (success)
            {
                SetState(new ClosedState());
                Notify($"Sprint '{Name}' has been released and closed. Product owner and scrum master are notified.");
            }
            else
            {
                SetState(new ReleaseFailedState());
                Notify($"Sprint '{Name}' release pipeline failed. Scrum master is notified (retry or cancel possible).");
            }
        }

        private void EnsureEditable()
        {
            if (State is not PlannedState)
            {
                throw new InvalidOperationException(
                    $"Sprint '{Name}' can only be modified while it is in the Planned state (current: {State.Name}).");
            }
        }
    }

    /// <summary>
    /// State Pattern: interface voor de sprint-stadia. Per stadium bepaalt de state
    /// welke acties zijn toegestaan; niet-toegestane acties gooien een exception.
    /// </summary>
    public interface ISprintState
    {
        string Name { get; }
        void AddBacklogItem(Sprint sprint, BacklogItem item);
        void Start(Sprint sprint);
        void Finish(Sprint sprint);
        void StartRelease(Sprint sprint);
        void CloseAfterReview(Sprint sprint);
        void UploadReviewSummary(Sprint sprint, string document);
        void Cancel(Sprint sprint);
    }

    /// <summary>
    /// Basisklasse die elke actie standaard als 'niet toegestaan' afhandelt.
    /// Concrete states overschrijven alleen de acties die in dat stadium mogen.
    /// </summary>
    public abstract class SprintStateBase : ISprintState
    {
        public abstract string Name { get; }

        public virtual void AddBacklogItem(Sprint sprint, BacklogItem item) => NotAllowed(nameof(AddBacklogItem));
        public virtual void Start(Sprint sprint) => NotAllowed(nameof(Start));
        public virtual void Finish(Sprint sprint) => NotAllowed(nameof(Finish));
        public virtual void StartRelease(Sprint sprint) => NotAllowed(nameof(StartRelease));
        public virtual void CloseAfterReview(Sprint sprint) => NotAllowed(nameof(CloseAfterReview));
        public virtual void UploadReviewSummary(Sprint sprint, string document) => NotAllowed(nameof(UploadReviewSummary));
        public virtual void Cancel(Sprint sprint) => NotAllowed(nameof(Cancel));

        protected void NotAllowed(string action)
            => throw new InvalidOperationException($"Action '{action}' is not allowed while the sprint is in state '{Name}'.");
    }

    /// <summary>Sprint is aangemaakt: eigenschappen wijzigbaar, backlog items toevoegen mag.</summary>
    public class PlannedState : SprintStateBase
    {
        public override string Name => "Planned";

        public override void AddBacklogItem(Sprint sprint, BacklogItem item) => sprint.AddItemInternal(item);

        public override void Start(Sprint sprint) => sprint.SetState(new ActiveState());

        // Een sprint die nog niet gestart is kan ook al geannuleerd worden.
        public override void Cancel(Sprint sprint)
        {
            sprint.SetState(new CancelledState());
            sprint.Notify($"Sprint '{sprint.Name}' has been cancelled. Product owner and scrum master are notified.");
        }
    }

    /// <summary>Sprint wordt uitgevoerd: voorgaande activiteiten kunnen niet meer aangepast worden.</summary>
    public class ActiveState : SprintStateBase
    {
        public override string Name => "Active";

        // 'De tijd is op' -> sprint krijgt status Finished.
        public override void Finish(Sprint sprint) => sprint.SetState(new FinishedState());
    }

    /// <summary>
    /// Sprint is afgelopen. Afhankelijk van het type volgt een review-afsluiting
    /// (met geüpload document) of een release via de development pipeline.
    /// </summary>
    public class FinishedState : SprintStateBase
    {
        public override string Name => "Finished";

        public override void UploadReviewSummary(Sprint sprint, string document)
        {
            if (sprint.Type != SprintType.Review)
            {
                throw new InvalidOperationException("Only a review sprint uses a review summary document.");
            }
            if (string.IsNullOrWhiteSpace(document))
            {
                throw new ArgumentException("A review summary document is required.", nameof(document));
            }
            sprint.SetReviewSummary(document);
        }

        public override void CloseAfterReview(Sprint sprint)
        {
            if (sprint.Type != SprintType.Review)
            {
                throw new InvalidOperationException("Only a review sprint is closed via a sprint review.");
            }
            // Casus: afsluiten kan alleen wanneer de reviewsamenvatting is geüpload.
            if (!sprint.HasReviewSummary)
            {
                throw new InvalidOperationException("Cannot close the sprint: the review summary has not been uploaded.");
            }
            sprint.SetState(new ClosedState());
            sprint.Notify($"Sprint '{sprint.Name}' review is completed and the sprint is closed.");
        }

        public override void StartRelease(Sprint sprint)
        {
            if (sprint.Type != SprintType.Release)
            {
                throw new InvalidOperationException("Only a release sprint can start a release.");
            }
            sprint.RunReleasePipeline();
        }

        public override void Cancel(Sprint sprint)
        {
            sprint.SetState(new CancelledState());
            sprint.Notify($"Sprint '{sprint.Name}' has been cancelled. Product owner and scrum master are notified.");
        }
    }

    /// <summary>De development pipeline draait: de sprint is gelocked en kan niet wijzigen.</summary>
    public class ReleasingState : SprintStateBase
    {
        public override string Name => "Releasing";
        // Alle acties zijn niet toegestaan zolang de pipeline loopt (geërfd gedrag).
    }

    /// <summary>De release-pipeline faalde: scrum master kan opnieuw proberen of annuleren.</summary>
    public class ReleaseFailedState : SprintStateBase
    {
        public override string Name => "ReleaseFailed";

        // Opnieuw het releaseproces proberen (bv. server was tijdelijk onbereikbaar).
        public override void StartRelease(Sprint sprint) => sprint.RunReleasePipeline();

        public override void Cancel(Sprint sprint)
        {
            sprint.SetState(new CancelledState());
            sprint.Notify($"Release of sprint '{sprint.Name}' has been cancelled. Product owner and scrum master are notified.");
        }
    }

    /// <summary>Sprint is succesvol afgerond/gereleased. Eindtoestand.</summary>
    public class ClosedState : SprintStateBase
    {
        public override string Name => "Closed";
    }

    /// <summary>Sprint/release is geannuleerd. Eindtoestand.</summary>
    public class CancelledState : SprintStateBase
    {
        public override string Name => "Cancelled";
    }
}
