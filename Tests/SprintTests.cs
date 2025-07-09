using System;
using Domain.Entities;
using Xunit;
using System;

namespace Tests
{
    public class SprintTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var start = DateTime.Now;
            var end = start.AddDays(14);
            var sprint = new Sprint("Sprint 1", start, end);
            Assert.Equal("Sprint 1", sprint.Name);
            Assert.Equal(start, sprint.StartDate);
            Assert.Equal(end, sprint.EndDate);
            Assert.Equal(SprintStatus.Planned, sprint.Status);
            Assert.Empty(sprint.BacklogItems);
        }
    }
}