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
    }
}