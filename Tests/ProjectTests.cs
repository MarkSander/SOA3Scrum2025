using System;
using Domain.Entities;
using Xunit;

namespace Tests
{
    public class ProjectTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            var owner = new ProductOwner("Owner");
            var project = new Project("Test Project", owner);
            Assert.Equal("Test Project", project.Name);
            Assert.Equal(owner, project.ProductOwner);
            Assert.Empty(project.Sprints);
        }
    }
}