using System;
using Domain.Entities;
using Xunit;

namespace Tests
{
    public class UserTests
    {
        [Fact]
        public void Developer_InitializesName()
        {
            var dev = new Developer("Dev");
            Assert.Equal("Dev", dev.Name);
        }

        [Fact]
        public void ScrumMaster_InitializesName()
        {
            var sm = new ScrumMaster("SM");
            Assert.Equal("SM", sm.Name);
        }

        [Fact]
        public void ProductOwner_InitializesName()
        {
            var po = new ProductOwner("PO");
            Assert.Equal("PO", po.Name);
        }
    }
}