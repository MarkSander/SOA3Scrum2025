using System;
using Domain.Entities;
using System.Collections.Generic;

namespace Domain
{
    public static class Class1
    {
        public static void Main()
        {
            // User entities
            var dev = new Developer("Dev Alice");
            var sm = new ScrumMaster("Scrum Bob");
            var po = new ProductOwner("Owner Carol");

            // Activity with sub-activities
            var activity = new Activity("Main Activity", 5) { AssignedDeveloper = dev };
            var subActivity = new Activity("Sub Activity", 2) { AssignedDeveloper = dev };
            activity.Add(subActivity);

            // Discussion
            var thread = new DiscussionThread();
            var comment = new DiscussionComment { Author = dev, Content = "Looks good!", CreatedAt = DateTime.Now };
            thread.AddComment(comment);

            // BacklogItem
            var backlogItem = new BacklogItem("Feature X", "Implement feature X", 8)
            {
                AssignedDeveloper = dev,
                Discussion = thread
            };
            backlogItem.Activities.Add(activity);

            // State transitions
            backlogItem.ChangeState(new DoingState());
            backlogItem.ChangeState(new ReadyForTestingState());

            // Pipeline
            var pipeline = new Pipeline();
            pipeline.AddAction(new FetchSourceCodeAction());
            pipeline.AddAction(new BuildAction());
            pipeline.AddAction(new TestAction());
            pipeline.Run();

            // Sprint
            var sprint = new Sprint("Sprint 1", DateTime.Now, DateTime.Now.AddDays(14))
            {
                Pipeline = pipeline
            };
            sprint.BacklogItems.Add(backlogItem);

            // Project
            var project = new Project("Demo Project", po);
            project.Sprints.Add(sprint);

            // SprintReport with decorators
            IReport report = new BasicSprintReport();
            report = new HeaderDecorator(report);
            report = new FooterDecorator(report);
            var sprintReport = new SprintReport { Sprint = sprint, Report = report };
            Console.WriteLine(sprintReport.Generate());

            // Output some info
            Console.WriteLine($"Project: {project.Name}, Owner: {project.ProductOwner.Name}");
            Console.WriteLine($"Sprint: {sprint.Name}, Backlog Items: {sprint.BacklogItems.Count}");
            Console.WriteLine($"Backlog Item: {backlogItem.Title}, State: {backlogItem.State.Name}");
            Console.WriteLine($"Activity: {activity.Name}, SubActivities: {activity.SubActivities.Count}");
            Console.WriteLine($"Discussion Comments: {thread.Comments.Count}");
        }
    }
}
