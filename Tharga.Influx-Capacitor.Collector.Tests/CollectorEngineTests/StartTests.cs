using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.CollectorEngineTests
{
    [TestFixture]
    public class StartTests
    {
        [Test]
        [Ignore("")]
        public async Task Should_not_send_data_to_database_when_no_SecondsInterval()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(0);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Never);
        }

        [Test]
        [Ignore("")]
        public async Task Should_send_data_to_database_when_started()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
        }

        [Test]
        [Ignore("")]
        public async Task Should_send_data_every_SecondsInterval()
        {
            //Arrange
            var databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            await collectorEngine.StartAsync();
            Thread.Sleep(2000);

            //Assert
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Never);
        }
    }
}