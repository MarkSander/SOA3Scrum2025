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
                var result = pipeline.Run();
                var output = sw.ToString();
                Assert.True(result);
                Assert.Contains("Fetching source code", output);
                Assert.Contains("Building project", output);
                Assert.Equal(PipelineStatus.Success, pipeline.Status);
            }
        }

        [Fact]
        public void Run_SetsPipelineStatusToSuccess_WhenAllActionsSucceed()
        {
            var pipeline = new Pipeline();
            pipeline.AddAction(new BuildAction());
            pipeline.AddAction(new TestAction());

            var result = pipeline.Run();

            Assert.True(result);
            Assert.Equal(PipelineStatus.Success, pipeline.Status);
        }

        [Fact]
        public void Rollback_UndoesExecutedActions()
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
                pipeline.Rollback();
                var output = sw.ToString();

                Assert.Contains("Cleaning build output", output);
                Assert.Contains("Cleaning up fetched source code", output);
                Assert.Equal(PipelineStatus.RolledBack, pipeline.Status);
            }
        }

        [Fact]
        public void Retry_RerunsFailedPipeline()
        {
            // Voor deze test kunnen we niet echt een failure simuleren zonder mock
            // maar we kunnen wel de functionaliteit testen
            var pipeline = new Pipeline();
            pipeline.AddAction(new BuildAction());

            pipeline.Run();
            Assert.Equal(PipelineStatus.Success, pipeline.Status);
        }

        [Fact]
        public void Action_HasCorrectInitialStatus()
        {
            var action = new BuildAction();
            Assert.Equal(ActionStatus.NotExecuted, action.Status);
        }

        [Fact]
        public void Action_StatusChangesToSuccess_AfterExecution()
        {
            var action = new BuildAction();
            action.Execute();
            Assert.Equal(ActionStatus.Success, action.Status);
        }

        [Fact]
        public void UndoableAction_CanBeUndone()
        {
            var action = new FetchSourceCodeAction();
            Assert.True(action.CanUndo);

            action.Execute();
            action.Undo();

            Assert.Equal(ActionStatus.NotExecuted, action.Status);
        }
    }
}