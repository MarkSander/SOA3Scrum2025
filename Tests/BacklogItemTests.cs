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
    }
}