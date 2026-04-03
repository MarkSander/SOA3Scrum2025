using System;
using Domain.Entities;
using Xunit;

namespace Tests
{
    public class SprintReportTests
    {
        [Fact]
        public void BasicSprintReport_Generate_ReturnsSprintData()
        {
            var report = new BasicSprintReport();
            Assert.Equal("Sprint data", report.Generate());
        }

        [Fact]
        public void HeaderDecorator_AddsHeader()
        {
            var report = new HeaderDecorator(new BasicSprintReport());
            Assert.StartsWith("[Bedrijfslogo + Header]", report.Generate());
        }

        [Fact]
        public void FooterDecorator_AddsFooter()
        {
            var report = new FooterDecorator(new BasicSprintReport());
            Assert.EndsWith("[Footer]", report.Generate());
        }

        [Fact]
        public void ChartDecorator_AddsChart()
        {
            var report = new ChartDecorator(new BasicSprintReport());
            Assert.EndsWith("[Chart]", report.Generate());
        }

        // Strategy Pattern tests
        [Fact]
        public void PdfExportStrategy_ExportsCorrectly()
        {
            var strategy = new PdfExportStrategy();
            var result = strategy.Export("Test content");

            Assert.Contains("[PDF Export]", result);
            Assert.Contains("Test content", result);
            Assert.Equal(".pdf", strategy.FileExtension);
        }

        [Fact]
        public void PngExportStrategy_ExportsCorrectly()
        {
            var strategy = new PngExportStrategy();
            var result = strategy.Export("Test content");

            Assert.Contains("[PNG Export", result);
            Assert.Contains("Test content", result);
            Assert.Equal(".png", strategy.FileExtension);
        }

        [Fact]
        public void CsvExportStrategy_ExportsCorrectly()
        {
            var strategy = new CsvExportStrategy();
            var result = strategy.Export("Test content");

            Assert.Contains("[CSV Export]", result);
            Assert.Equal(".csv", strategy.FileExtension);
        }

        [Fact]
        public void SprintReport_SetExportStrategy_AllowsExport()
        {
            var sprintReport = new SprintReport
            {
                Report = new BasicSprintReport()
            };
            sprintReport.SetExportStrategy(new PdfExportStrategy());

            var result = sprintReport.Export();

            Assert.Contains("[PDF Export]", result);
            Assert.Contains("Sprint data", result);
        }

        [Fact]
        public void SprintReport_Export_ThrowsWhenStrategyNotSet()
        {
            var sprintReport = new SprintReport
            {
                Report = new BasicSprintReport()
            };

            Assert.Throws<InvalidOperationException>(() => sprintReport.Export());
        }

        [Fact]
        public void SprintReport_CanSwitchExportStrategies()
        {
            var sprintReport = new SprintReport
            {
                Report = new BasicSprintReport()
            };

            // Eerst PDF
            sprintReport.SetExportStrategy(new PdfExportStrategy());
            var pdfResult = sprintReport.Export();
            Assert.Contains("[PDF Export]", pdfResult);

            // Dan PNG
            sprintReport.SetExportStrategy(new PngExportStrategy());
            var pngResult = sprintReport.Export();
            Assert.Contains("[PNG Export", pngResult);
        }

        [Fact]
        public void SprintReport_WithDecoratorAndStrategy_WorksTogether()
        {
            // Test dat Decorator en Strategy patterns samen werken
            var basicReport = new BasicSprintReport();
            var decoratedReport = new HeaderDecorator(new FooterDecorator(basicReport));

            var sprintReport = new SprintReport
            {
                Report = decoratedReport
            };
            sprintReport.SetExportStrategy(new PdfExportStrategy());

            var result = sprintReport.Export();

            Assert.Contains("[PDF Export]", result);
            Assert.Contains("[Bedrijfslogo + Header]", result);
            Assert.Contains("[Footer]", result);
        }
    }
}