using System.Collections.Generic;
using System;

namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een development pipeline voor een Sprint (Command Pattern).
    /// Commands kunnen uitgevoerd, ongedaan gemaakt en opnieuw geprobeerd worden.
    /// </summary>
    public interface IPipelineAction
    {
        void Execute();
        void Undo();
        bool CanUndo { get; }
        string Name { get; }
        ActionStatus Status { get; }
    }

    public enum ActionStatus
    {
        NotExecuted,
        Executing,
        Success,
        Failed
    }

    public abstract class PipelineActionBase : IPipelineAction
    {
        public abstract string Name { get; }
        public ActionStatus Status { get; protected set; }
        public abstract bool CanUndo { get; }

        protected PipelineActionBase()
        {
            Status = ActionStatus.NotExecuted;
        }

        public virtual void Execute()
        {
            Status = ActionStatus.Executing;
            try
            {
                ExecuteAction();
                Status = ActionStatus.Success;
            }
            catch (Exception)
            {
                Status = ActionStatus.Failed;
                throw;
            }
        }

        protected abstract void ExecuteAction();

        public virtual void Undo()
        {
            if (!CanUndo)
                throw new InvalidOperationException($"Action {Name} cannot be undone.");

            UndoAction();
            Status = ActionStatus.NotExecuted;
        }

        protected abstract void UndoAction();
    }

    public class FetchSourceCodeAction : PipelineActionBase
    {
        public override string Name => "Fetch Source Code";
        public override bool CanUndo => true;

        protected override void ExecuteAction() 
            => Console.WriteLine("Fetching source code from repository...");

        protected override void UndoAction() 
            => Console.WriteLine("Cleaning up fetched source code...");
    }

    public class InstallPackagesAction : PipelineActionBase
    {
        public override string Name => "Install Packages";
        public override bool CanUndo => true;

        protected override void ExecuteAction() 
            => Console.WriteLine("Installing NuGet packages...");

        protected override void UndoAction() 
            => Console.WriteLine("Removing installed packages...");
    }

    public class BuildAction : PipelineActionBase
    {
        public override string Name => "Build";
        public override bool CanUndo => true;

        protected override void ExecuteAction() 
            => Console.WriteLine("Building project...");

        protected override void UndoAction() 
            => Console.WriteLine("Cleaning build output...");
    }

    public class TestAction : PipelineActionBase
    {
        public override string Name => "Test";
        public override bool CanUndo => false;

        protected override void ExecuteAction() 
            => Console.WriteLine("Running unit tests...");

        protected override void UndoAction() 
            => throw new NotSupportedException("Test action cannot be undone.");
    }

    public class AnalyseAction : PipelineActionBase
    {
        public override string Name => "Analyse";
        public override bool CanUndo => false;

        protected override void ExecuteAction() 
            => Console.WriteLine("Analysing code with SonarCloud...");

        protected override void UndoAction() 
            => throw new NotSupportedException("Analyse action cannot be undone.");
    }

    public class DeployAction : PipelineActionBase
    {
        public override string Name => "Deploy";
        public override bool CanUndo => true;

        protected override void ExecuteAction() 
            => Console.WriteLine("Deploying application to Azure...");

        protected override void UndoAction() 
            => Console.WriteLine("Rolling back deployment...");
    }

    public class Pipeline
    {
        public List<IPipelineAction> Actions { get; set; }
        private readonly Stack<IPipelineAction> _executedActions;
        public PipelineStatus Status { get; private set; }

        public Pipeline()
        {
            Actions = new List<IPipelineAction>();
            _executedActions = new Stack<IPipelineAction>();
            Status = PipelineStatus.NotStarted;
        }

        public void AddAction(IPipelineAction action) => Actions.Add(action);

        // Command Pattern: Execute alle actions sequentieel
        public bool Run()
        {
            Status = PipelineStatus.Running;
            _executedActions.Clear();

            foreach (var action in Actions)
            {
                try
                {
                    Console.WriteLine($"Executing: {action.Name}");
                    action.Execute();
                    _executedActions.Push(action);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Action {action.Name} failed: {ex.Message}");
                    Status = PipelineStatus.Failed;
                    return false;
                }
            }

            Status = PipelineStatus.Success;
            return true;
        }

        // Command Pattern: Retry de hele pipeline vanaf het begin
        public bool Retry()
        {
            if (Status != PipelineStatus.Failed)
                throw new InvalidOperationException("Can only retry a failed pipeline.");

            Console.WriteLine("Retrying pipeline...");
            return Run();
        }

        // Command Pattern: Rollback uitgevoerde actions (undo)
        public void Rollback()
        {
            Console.WriteLine("Rolling back pipeline...");

            while (_executedActions.Count > 0)
            {
                var action = _executedActions.Pop();
                if (action.CanUndo)
                {
                    try
                    {
                        Console.WriteLine($"Undoing: {action.Name}");
                        action.Undo();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to undo {action.Name}: {ex.Message}");
                    }
                }
            }

            Status = PipelineStatus.RolledBack;
        }
    }

    public enum PipelineStatus
    {
        NotStarted,
        Running,
        Success,
        Failed,
        RolledBack
    }
}