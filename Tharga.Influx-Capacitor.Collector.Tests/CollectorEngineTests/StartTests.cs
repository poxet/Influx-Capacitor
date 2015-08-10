using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.CollectorEngineTests
{
    [TestFixture]
    public class StartTests
    {
        [Test]
        public async void Should_not_send_data_to_database_when_no_SecondsInterval()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(0);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var collectorEngine = new CollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            Assert.Fail("Should assert that the message is enqued.");
        }

        [Test]
        public async void Should_send_data_to_database_when_started()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var collectorEngine = new CollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            Assert.Fail("Should assert that the message is enqued.");
        }

        [Test]
        public async void Should_send_data_every_SecondsInterval()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var collectorEngine = new CollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object);

            //Act
            await collectorEngine.StartAsync();
            Thread.Sleep(2000);

            //Assert
            Assert.Fail("Should assert that the message is enqued.");
        }
    }
}