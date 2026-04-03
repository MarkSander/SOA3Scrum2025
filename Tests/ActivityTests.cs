using System;
using System.Collections.Generic;
using Domain.Entities;
using Xunit;

namespace Tests
{
    public class ActivityTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var activity = new Activity("Test Activity", 5);
            Assert.Equal("Test Activity", activity.Name);
            Assert.Equal(ActivityStatus.Todo, activity.Status);
            Assert.Equal(5, activity.EffortPoints);
            Assert.Empty(activity.SubActivities);
        }

        [Fact]
        public void Add_AddsSubActivity()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);
            Assert.Contains(child, parent.SubActivities);
        }

        [Fact]
        public void Remove_RemovesSubActivity()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);
            parent.Remove(child);
            Assert.DoesNotContain(child, parent.SubActivities);
        }

        [Fact]
        public void GetStatus_ReturnsStatusAsString()
        {
            var activity = new Activity("Test");
            activity.Status = ActivityStatus.Done;
            Assert.Equal("Done", activity.GetStatus());
        }

        // Composite Pattern tests
        [Fact]
        public void GetEffortPoints_ReturnsSum_WhenHasSubActivities()
        {
            var parent = new Activity("Parent", 5);
            var child1 = new Activity("Child1", 3);
            var child2 = new Activity("Child2", 2);

            parent.Add(child1);
            parent.Add(child2);

            // 5 (parent) + 3 (child1) + 2 (child2) = 10
            Assert.Equal(10, parent.GetEffortPoints());
        }

        [Fact]
        public void GetEffortPoints_Recursive_WithNestedActivities()
        {
            var parent = new Activity("Parent", 10);
            var child = new Activity("Child", 5);
            var grandChild = new Activity("GrandChild", 3);

            child.Add(grandChild);
            parent.Add(child);

            // 10 + 5 + 3 = 18
            Assert.Equal(18, parent.GetEffortPoints());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsFalse_WhenSubActivitiesNotDone()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);

            child.Status = ActivityStatus.Todo;

            Assert.False(parent.CanMarkAsDone());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsTrue_WhenAllSubActivitiesDone()
        {
            var parent = new Activity("Parent");
            var child1 = new Activity("Child1");
            var child2 = new Activity("Child2");

            parent.Add(child1);
            parent.Add(child2);

            child1.Status = ActivityStatus.Done;
            child2.Status = ActivityStatus.Done;

            Assert.True(parent.CanMarkAsDone());
        }

        [Fact]
        public void CanMarkAsDone_ReturnsTrue_WhenNoSubActivities()
        {
            var activity = new Activity("Test");
            Assert.True(activity.CanMarkAsDone());
        }

        [Fact]
        public void SetStatus_ToDone_ThrowsException_WhenSubActivitiesNotDone()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);

            child.Status = ActivityStatus.Todo;

            var exception = Assert.Throws<InvalidOperationException>(
                () => parent.SetStatus(ActivityStatus.Done)
            );
            Assert.Contains("not all sub-activities are completed", exception.Message);
        }

        [Fact]
        public void SetStatus_ToDone_Succeeds_WhenAllSubActivitiesDone()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);

            child.Status = ActivityStatus.Done;
            parent.SetStatus(ActivityStatus.Done);

            Assert.Equal(ActivityStatus.Done, parent.Status);
        }

        [Fact]
        public void GetStatus_ShowsWarning_WhenDoneButSubActivitiesNotDone()
        {
            var parent = new Activity("Parent");
            var child = new Activity("Child");
            parent.Add(child);

            // Force set status (bypassing validation for test)
            parent.Status = ActivityStatus.Done;
            child.Status = ActivityStatus.Todo;

            var status = parent.GetStatus();
            Assert.Contains("not all sub-activities completed", status);
        }
    }
}