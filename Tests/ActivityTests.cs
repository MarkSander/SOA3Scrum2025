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
    }
}