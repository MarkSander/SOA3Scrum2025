using System;
using Domain.Entities;
using Xunit;

namespace Tests
{
    public class DiscussionThreadTests
    {
        [Fact]
        public void AddComment_AddsComment_WhenNotClosed()
        {
            var thread = new DiscussionThread();
            var comment = new DiscussionComment { Content = "Test", Author = null };
            thread.AddComment(comment);
            Assert.Contains(comment, thread.Comments);
        }

        [Fact]
        public void AddComment_DoesNotAdd_WhenClosed()
        {
            var thread = new DiscussionThread();
            thread.Close();
            var comment = new DiscussionComment { Content = "Test", Author = null };
            thread.AddComment(comment);
            Assert.DoesNotContain(comment, thread.Comments);
        }

        [Fact]
        public void Close_SetsIsClosedToTrue()
        {
            var thread = new DiscussionThread();
            thread.Close();
            Assert.True(thread.IsClosed);
        }
    }
}