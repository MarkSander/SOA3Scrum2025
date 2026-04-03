namespace Domain.Entities
{
    /// <summary>
    /// Vertegenwoordigt een rapportage van een Sprint (Decorator Pattern).
    /// Gebruikt Strategy Pattern voor export naar verschillende formaten.
    /// </summary>
    public class SprintReport
    {
        public Sprint? Sprint { get; set; }
        public IReport? Report { get; set; }
        private IReportExportStrategy? _exportStrategy;

        public string Generate() => Report?.Generate() ?? "No report available";

        // Strategy Pattern: Stel de export strategie in
        public void SetExportStrategy(IReportExportStrategy strategy)
        {
            _exportStrategy = strategy;
        }

        // Exporteer het rapport met de gekozen strategie
        public string Export()
        {
            if (_exportStrategy == null)
                throw new InvalidOperationException("Export strategy not set. Use SetExportStrategy first.");

            string content = Generate();
            return _exportStrategy.Export(content);
        }
    }

    public interface IReport
    {
        string Generate();
    }

    public class BasicSprintReport : IReport
    {
        public virtual string Generate() => "Sprint data";
    }

    // Decorator Pattern voor het toevoegen van headers, footers, charts, etc.
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

    // Strategy Pattern: Interface voor verschillende export formaten
    public interface IReportExportStrategy
    {
        string Export(string content);
        string FileExtension { get; }
    }

    public class PdfExportStrategy : IReportExportStrategy
    {
        public string FileExtension => ".pdf";

        public string Export(string content)
        {
            // Stub implementatie voor PDF export
            return $"[PDF Export]\n{content}\n[End PDF]";
        }
    }

    public class PngExportStrategy : IReportExportStrategy
    {
        public string FileExtension => ".png";

        public string Export(string content)
        {
            // Stub implementatie voor PNG export
            return $"[PNG Export - Image Format]\n{content}\n[End PNG]";
        }
    }

    public class CsvExportStrategy : IReportExportStrategy
    {
        public string FileExtension => ".csv";

        public string Export(string content)
        {
            // Stub implementatie voor CSV export
            return $"[CSV Export]\n{content.Replace("\n", ",")}\n[End CSV]";
        }
    }
}