namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een rapportage van een Sprint (Decorator Pattern).
    /// </summary>
    public class SprintReport
    {
        public Sprint? Sprint { get; set; }
        public IReport? Report { get; set; }
        public string Generate() => Report?.Generate() ?? "No report available";
    }

    public interface IReport
    {
        string Generate();
    }

    public class BasicSprintReport : IReport
    {
        public virtual string Generate() => "Sprint data";
    }

    public abstract class ReportDecorator : IReport
    {
        protected readonly IReport _component;
        protected ReportDecorator(IReport component) { _component = component; }
        public abstract string Generate();
    }

    public class HeaderDecorator : ReportDecorator
    {
        public HeaderDecorator(IReport component) : base(component) { }
        public override string Generate() => "[Bedrijfslogo + Header]\n" + _component.Generate();
    }

    public class FooterDecorator : ReportDecorator
    {
        public FooterDecorator(IReport component) : base(component) { }
        public override string Generate() => _component.Generate() + "\n[Footer]";
    }

    public class ChartDecorator : ReportDecorator
    {
        public ChartDecorator(IReport component) : base(component) { }
        public override string Generate() => _component.Generate() + "\n[Chart]";
    }
}