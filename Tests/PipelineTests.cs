using System;
using Domain.Entities;
using Xunit;
using System.IO;

namespace Tests
{
    public class PipelineTests
    {
        [Fact]
        public void AddAction_AddsActionToPipeline()
        {
            var pipeline = new Pipeline();
            var action = new BuildAction();
            pipeline.AddAction(action);
            Assert.Contains(action, pipeline.Actions);
        }

        [Fact]
        public void Run_ExecutesAllActions()
        {
            var pipeline = new Pipeline();
            var action1 = new FetchSourceCodeAction();
            var action2 = new BuildAction();
            pipeline.AddAction(action1);
            pipeline.AddAction(action2);
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                pipeline.Run();
                var output = sw.ToString();
                Assert.Contains("Fetching source code", output);
                Assert.Contains("Building project", output);
            }
        }
    }
}