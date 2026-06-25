using System;
using Domain.Entities;
using NSubstitute;
using Xunit;

namespace Tests
{
    public class SprintTests
    {
        private static Sprint NewSprint(SprintType type = SprintType.Review)
        {
            var start = new DateTime(2026, 1, 1);
            return new Sprint("Sprint 1", start, start.AddDays(14), type);
        }

        [Fact]
        public void Constructor_InitializesProperties_InPlannedState()
        {
            var sprint = NewSprint();

            Assert.Equal("Sprint 1", sprint.Name);
            Assert.Equal("Planned", sprint.StatusName);
            Assert.IsType<PlannedState>(sprint.State);
            Assert.Empty(sprint.BacklogItems);
        }

        [Fact]
        public void AddBacklogItem_InPlanned_AddsItem()
        {
            var sprint = NewSprint();

            sprint.AddBacklogItem(new BacklogItem("Item", "Desc"));

            Assert.Single(sprint.BacklogItems);
        }

        [Fact]
        public void Reschedule_InPlanned_UpdatesDates()
        {
            var sprint = NewSprint();
            var newEnd = new DateTime(2026, 2, 1);

            sprint.Reschedule(sprint.StartDate, newEnd);

            Assert.Equal(newEnd, sprint.EndDate);
        }

        [Fact]
        public void Start_InPlanned_MovesToActive()
        {
            var sprint = NewSprint();

            sprint.Start();

            Assert.Equal("Active", sprint.StatusName);
        }

        [Fact]
        public void AddBacklogItem_WhenActive_Throws()
        {
            var sprint = NewSprint();
            sprint.Start();

            // Casus: tijdens de uitvoering kunnen voorgaande activiteiten niet meer aangepast worden.
            Assert.Throws<InvalidOperationException>(() => sprint.AddBacklogItem(new BacklogItem("Late", "Desc")));
        }

        [Fact]
        public void Reschedule_WhenActive_Throws()
        {
            var sprint = NewSprint();
            sprint.Start();

            Assert.Throws<InvalidOperationException>(() => sprint.Reschedule(sprint.StartDate, sprint.EndDate.AddDays(1)));
        }

        [Fact]
        public void Finish_WhenActive_MovesToFinished()
        {
            var sprint = NewSprint();
            sprint.Start();

            sprint.Finish();

            Assert.Equal("Finished", sprint.StatusName);
        }

        [Fact]
        public void CloseAfterReview_WithoutSummary_Throws()
        {
            var sprint = NewSprint(SprintType.Review);
            sprint.Start();
            sprint.Finish();

            // Casus: afsluiten kan alleen wanneer de reviewsamenvatting is geüpload.
            Assert.Throws<InvalidOperationException>(() => sprint.CloseAfterReview());
            Assert.Equal("Finished", sprint.StatusName);
        }

        [Fact]
        public void CloseAfterReview_WithSummary_ClosesAndNotifies()
        {
            var sprint = NewSprint(SprintType.Review);
            var channel = Substitute.For<NotificationChannel>();
            sprint.NotificationChannels.Add(channel);
            sprint.Start();
            sprint.Finish();
            sprint.UploadReviewSummary("Review notes document");

            sprint.CloseAfterReview();

            Assert.Equal("Closed", sprint.StatusName);
            channel.Received(1).Send(Arg.Is<string>(m => m.Contains("closed")));
        }

        [Fact]
        public void UploadReviewSummary_EmptyDocument_Throws()
        {
            var sprint = NewSprint(SprintType.Review);
            sprint.Start();
            sprint.Finish();

            Assert.Throws<ArgumentException>(() => sprint.UploadReviewSummary("  "));
        }

        [Fact]
        public void StartRelease_OnReviewSprint_Throws()
        {
            var sprint = NewSprint(SprintType.Review);
            sprint.Start();
            sprint.Finish();

            Assert.Throws<InvalidOperationException>(() => sprint.StartRelease());
        }

        [Fact]
        public void StartRelease_WithSuccessfulPipeline_ClosesAndNotifies()
        {
            var sprint = NewSprint(SprintType.Release);
            var channel = Substitute.For<NotificationChannel>();
            sprint.NotificationChannels.Add(channel);
            var pipeline = new Pipeline();
            pipeline.AddAction(new BuildAction());
            sprint.Pipeline = pipeline;
            sprint.Start();
            sprint.Finish();

            sprint.StartRelease();

            Assert.Equal("Closed", sprint.StatusName);
            channel.Received(1).Send(Arg.Is<string>(m => m.Contains("released")));
        }

        [Fact]
        public void StartRelease_WithoutPipeline_Throws()
        {
            var sprint = NewSprint(SprintType.Release);
            sprint.Start();
            sprint.Finish();

            Assert.Throws<InvalidOperationException>(() => sprint.StartRelease());
        }

        [Fact]
        public void StartRelease_WithFailingPipeline_MovesToReleaseFailedAndNotifies()
        {
            var sprint = NewSprint(SprintType.Release);
            var channel = Substitute.For<NotificationChannel>();
            sprint.NotificationChannels.Add(channel);
            sprint.Pipeline = BuildFailingPipeline();
            sprint.Start();
            sprint.Finish();

            sprint.StartRelease();

            Assert.Equal("ReleaseFailed", sprint.StatusName);
            channel.Received(1).Send(Arg.Is<string>(m => m.Contains("failed")));
        }

        [Fact]
        public void Retry_AfterFailedRelease_CanSucceed()
        {
            var sprint = NewSprint(SprintType.Release);
            sprint.Pipeline = BuildFailingPipeline();
            sprint.Start();
            sprint.Finish();
            sprint.StartRelease(); // fails -> ReleaseFailed

            // Vervang de pipeline door een werkende en probeer opnieuw.
            var working = new Pipeline();
            working.AddAction(new BuildAction());
            sprint.Pipeline = working;

            sprint.StartRelease(); // retry

            Assert.Equal("Closed", sprint.StatusName);
        }

        [Fact]
        public void Cancel_AfterFailedRelease_MovesToCancelled()
        {
            var sprint = NewSprint(SprintType.Release);
            sprint.Pipeline = BuildFailingPipeline();
            sprint.Start();
            sprint.Finish();
            sprint.StartRelease();

            sprint.Cancel();

            Assert.Equal("Cancelled", sprint.StatusName);
        }

        [Fact]
        public void Cancel_WhenFinished_MovesToCancelledAndNotifies()
        {
            var sprint = NewSprint(SprintType.Release);
            var channel = Substitute.For<NotificationChannel>();
            sprint.NotificationChannels.Add(channel);
            sprint.Start();
            sprint.Finish();

            sprint.Cancel();

            Assert.Equal("Cancelled", sprint.StatusName);
            channel.Received(1).Send(Arg.Is<string>(m => m.Contains("cancelled")));
        }

        [Fact]
        public void Operations_OnClosedSprint_Throw()
        {
            var sprint = NewSprint(SprintType.Review);
            sprint.Start();
            sprint.Finish();
            sprint.UploadReviewSummary("doc");
            sprint.CloseAfterReview();

            Assert.Throws<InvalidOperationException>(() => sprint.Start());
            Assert.Throws<InvalidOperationException>(() => sprint.Cancel());
        }

        [Fact]
        public void ReleasingState_LocksAllOperations()
        {
            // De transiënte 'Releasing' toestand vergrendelt de sprint: rechtstreeks getest.
            var sprint = NewSprint(SprintType.Release);
            var state = new ReleasingState();

            Assert.Equal("Releasing", state.Name);
            Assert.Throws<InvalidOperationException>(() => state.Finish(sprint));
            Assert.Throws<InvalidOperationException>(() => state.AddBacklogItem(sprint, new BacklogItem("X", "Y")));
        }

        private static Pipeline BuildFailingPipeline()
        {
            var failing = Substitute.For<IPipelineAction>();
            failing.Name.Returns("Failing");
            failing.When(a => a.Execute()).Do(_ => throw new InvalidOperationException("boom"));

            var pipeline = new Pipeline();
            pipeline.AddAction(failing);
            return pipeline;
        }
    }
}
