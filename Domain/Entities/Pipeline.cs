using System.Collections.Generic;
using System;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een development pipeline voor een Sprint (Command Pattern).
    /// </summary>
    public interface IPipelineAction
    {
        void Execute();
        string Name { get; }
    }

    public class FetchSourceCodeAction : IPipelineAction
    {
        public string Name => "Fetch Source Code";
        public void Execute() => Console.WriteLine("Fetching source code...");
    }

    public class InstallPackagesAction : IPipelineAction
    {
        public string Name => "Install Packages";
        public void Execute() => Console.WriteLine("Installing packages...");
    }

    public class BuildAction : IPipelineAction
    {
        public string Name => "Build";
        public void Execute() => Console.WriteLine("Building project...");
    }

    public class TestAction : IPipelineAction
    {
        public string Name => "Test";
        public void Execute() => Console.WriteLine("Running tests...");
    }

    public class AnalyseAction : IPipelineAction
    {
        public string Name => "Analyse";
        public void Execute() => Console.WriteLine("Analysing code...");
    }

    public class DeployAction : IPipelineAction
    {
        public string Name => "Deploy";
        public void Execute() => Console.WriteLine("Deploying application...");
    }

    public class Pipeline
    {
        public List<IPipelineAction> Actions { get; set; }

        public Pipeline()
        {
            Actions = new List<IPipelineAction>();
        }

        public void AddAction(IPipelineAction action) => Actions.Add(action);

        public void Run()
        {
            foreach (var action in Actions)
            {
                action.Execute();
            }
        }
    }
}