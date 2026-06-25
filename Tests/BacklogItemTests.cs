using System;
using Domain.Entities;
using NSubstitute;
using Xunit;

namespace Tests
{
    public class BacklogItemTests
    {
        [Fact]
        public void ChangeState_ValidTransition_TodoToDoing_Succeeds()
        {
            // Arrange
            var item = new BacklogItem("Test item", "Beschrijving");
            var doingState = new DoingState();

            // Act
            item.ChangeState(doingState);

            // Assert
            Assert.Equal("Doing", item.State.Name);
        }

        [Fact]
        public void ChangeState_InvalidTransition_TodoToTested_Throws()
        {
            // Arrange
            var item = new BacklogItem("Test item", "Beschrijving");
            var testedState = new TestedState();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => item.ChangeState(testedState));
        }

        [Fact]
        public void ChangeState_ReadyForTesting_SendsNotification()
        {
            // Arrange
            var item = new BacklogItem("Test item", "Beschrijving");
            var subscriber = Substitute.For<INotificationSubscriber>();
            item.Subscribe(subscriber);
            item.ChangeState(new DoingState());

            // Act
            item.ChangeState(new ReadyForTestingState());

            // Assert
            subscriber.Received(1).Notify(item, Arg.Is<string>(msg => msg.Contains("ReadyForTesting")));
        }

        [Fact]
        public void ChangeState_Done_SendsNotification()
        {
            // Arrange
            var item = new BacklogItem("Test item", "Beschrijving");
            var subscriber = Substitute.For<INotificationSubscriber>();
            item.Subscribe(subscriber);
            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());
            item.ChangeState(new TestingState());
            item.ChangeState(new TestedState());

            // Act
            item.ChangeState(new DoneState());

            // Assert
            subscriber.Received(1).Notify(item, Arg.Is<string>(msg => msg.Contains("Done")));
        }

        [Fact]
        public void ChangeState_InvalidTransition_ThrowsAndNoNotification()
        {
            // Arrange
            var item = new BacklogItem("Test item", "Beschrijving");
            var subscriber = Substitute.For<INotificationSubscriber>();
            item.Subscribe(subscriber);
            var testedState = new TestedState();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => item.ChangeState(testedState));
            subscriber.DidNotReceive().Notify(Arg.Any<BacklogItem>(), Arg.Any<string>());
        }

        // Composite Pattern tests
        [Fact]
        public void AddWorkItem_AddsActivityToBacklogItem()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            var activity = new Activity("Test Activity", 5);

            backlogItem.AddWorkItem(activity);

            Assert.Contains(activity, backlogItem.WorkItems);
            Assert.Contains(activity, backlogItem.Activities);
        }

        [Fact]
        public void RemoveWorkItem_RemovesActivityFromBacklogItem()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            var activity = new Activity("Test Activity", 5);

            backlogItem.AddWorkItem(activity);
            backlogItem.RemoveWorkItem(activity);

            Assert.DoesNotContain(activity, backlogItem.WorkItems);
            Assert.DoesNotContain(activity, backlogItem.Activities);
        }

        [Fact]
        public void GetEffortPoints_ReturnsSum_WithActivities()
        {
            var backlogItem = new BacklogItem("Test Item", "Description", effortPoints: 10);
            var activity1 = new Activity("Activity 1", 5);
            var activity2 = new Activity("Activity 2", 3);

            backlogItem.AddWorkItem(activity1);
            backlogItem.AddWorkItem(activity2);

            // 10 (backlog) + 5 (activity1) + 3 (activity2) = 18
            Assert.Equal(18, backlogItem.GetEffortPoints());
        }

        [Fact]
        public void GetEffortPoints_Recursive_WithNestedActivities()
        {
            var backlogItem = new BacklogItem("Test Item", "Description", effortPoints: 10);
            var activity = new Activity("Activity", 5);
            var subActivity = new Activity("SubActivity", 3);

            activity.Add(subActivity);
            backlogItem.AddWorkItem(activity);

            // 10 (backlog) + 5 (activity) + 3 (subActivity) = 18
            Assert.Equal(18, backlogItem.GetEffortPoints());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsFalse_WhenActivitiesNotDone()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            var activity1 = new Activity("Activity 1");
            var activity2 = new Activity("Activity 2");

            backlogItem.AddWorkItem(activity1);
            backlogItem.AddWorkItem(activity2);

            activity1.Status = ActivityStatus.Done;
            activity2.Status = ActivityStatus.Todo;

            Assert.False(backlogItem.CanMarkAsDone());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsTrue_WhenAllActivitiesDone()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            var activity1 = new Activity("Activity 1");
            var activity2 = new Activity("Activity 2");

            backlogItem.AddWorkItem(activity1);
            backlogItem.AddWorkItem(activity2);

            activity1.Status = ActivityStatus.Done;
            activity2.Status = ActivityStatus.Done;

            Assert.True(backlogItem.CanMarkAsDone());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsTrue_WhenNoActivities()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            Assert.True(backlogItem.CanMarkAsDone());
        }

        [Fact]
        public void GetStatus_ShowsWarning_WhenDoneButActivitiesNotComplete()
        {
            var backlogItem = new BacklogItem("Test Item", "Description");
            var activity = new Activity("Activity");
            backlogItem.AddWorkItem(activity);
            activity.Status = ActivityStatus.Done; // mag naar Done

            // Doorloop alle states tot Done (toegestaan want activity is Done).
            backlogItem.ChangeState(new DoingState());
            backlogItem.ChangeState(new ReadyForTestingState());
            backlogItem.ChangeState(new TestingState());
            backlogItem.ChangeState(new TestedState());
            backlogItem.ChangeState(new DoneState());

            // Activity wordt achteraf teruggezet -> defensieve waarschuwing in GetStatus.
            activity.Status = ActivityStatus.Todo;

            var status = backlogItem.GetStatus();
            Assert.Contains("not all activities completed", status);
        }

        // --- Casus business rules: state transitions ---

        [Fact]
        public void ChangeState_ToDone_Throws_WhenActivitiesNotComplete()
        {
            // BR: een backlog item mag pas naar Done als alle activities Done zijn.
            var backlogItem = new BacklogItem("Test Item", "Description");
            backlogItem.AddWorkItem(new Activity("Open work")); // blijft Todo

            backlogItem.ChangeState(new DoingState());
            backlogItem.ChangeState(new ReadyForTestingState());
            backlogItem.ChangeState(new TestingState());
            backlogItem.ChangeState(new TestedState());

            Assert.Throws<InvalidOperationException>(() => backlogItem.ChangeState(new DoneState()));
            Assert.Equal("Tested", backlogItem.State.Name);
        }

        [Fact]
        public void ChangeState_ReadyForTestingToTodo_NotifiesScrumMaster()
        {
            // Casus: tester ziet dat item niet klaar is -> terug naar Todo, scrum master genotificeerd.
            var item = new BacklogItem("Test item", "Beschrijving");
            var subscriber = Substitute.For<INotificationSubscriber>();
            item.Subscribe(subscriber);
            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());

            item.ChangeState(new TodoState());

            Assert.Equal("Todo", item.State.Name);
            subscriber.Received(1).Notify(item, Arg.Is<string>(msg => msg.Contains("scrum master")));
        }

        [Fact]
        public void ChangeState_TestingToTodo_IsAllowed()
        {
            // Casus: tijdens testen gaat er iets mis -> werk voor developer, terug naar Todo.
            var item = new BacklogItem("Test item", "Beschrijving");
            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());
            item.ChangeState(new TestingState());

            item.ChangeState(new TodoState());

            Assert.Equal("Todo", item.State.Name);
        }

        [Fact]
        public void ChangeState_TestedToReadyForTesting_IsAllowed()
        {
            // Casus: (lead) developer keurt DoD af -> terug naar ReadyForTesting voor hertest.
            var item = new BacklogItem("Test item", "Beschrijving");
            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());
            item.ChangeState(new TestingState());
            item.ChangeState(new TestedState());

            item.ChangeState(new ReadyForTestingState());

            Assert.Equal("ReadyForTesting", item.State.Name);
        }

        [Fact]
        public void ChangeState_ReadyForTestingToDoing_Throws()
        {
            // Casus: 'terug naar doing kan niet'.
            var item = new BacklogItem("Test item", "Beschrijving");
            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());

            Assert.Throws<InvalidOperationException>(() => item.ChangeState(new DoingState()));
        }

        [Fact]
        public void ChangeState_ToDone_Succeeds_WhenAllActivitiesDone()
        {
            var item = new BacklogItem("Test item", "Beschrijving");
            var activity = new Activity("Work");
            item.AddWorkItem(activity);
            activity.Status = ActivityStatus.Done;

            item.ChangeState(new DoingState());
            item.ChangeState(new ReadyForTestingState());
            item.ChangeState(new TestingState());
            item.ChangeState(new TestedState());
            item.ChangeState(new DoneState());

            Assert.Equal("Done", item.State.Name);
        }
    }
}