using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een discussie (thread) gekoppeld aan een BacklogItem.
    /// </summary>
    public class DiscussionThread
    {
        public Guid Id { get; set; }
        public List<DiscussionComment> Comments { get; set; } = new List<DiscussionComment>();
        public bool IsClosed { get; set; }
        public List<string> StringComments { get; set; }

        public DiscussionThread()
        {
            StringComments = new List<string>();
        }

        public void AddComment(DiscussionComment comment) { if (!IsClosed) Comments.Add(comment); }
        public void Close() => IsClosed = true;
    }
    public class DiscussionComment
    {
        public Guid Id { get; set; }
        public User? Author { get; set; }
        public string? Content { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}