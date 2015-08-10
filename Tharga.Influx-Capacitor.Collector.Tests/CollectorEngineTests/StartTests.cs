using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using InfluxDB.Net;
using InfluxDB.Net.Models;
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
            //var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            //client.Setup(x => x.WriteAsync(It.IsAny<Point[]>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(0);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { });
            var collectorEngine = new CollectorEngine(performanceCounterGroup.Object);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            //client.Verify(x => x.WriteAsync(It.IsAny<Point[]>()), Times.Never);
            Assert.Fail("Should assert that the message is enqued.");
        }

        [Test]
        public async void Should_send_data_to_database_when_started()
        {
            //Arrange
            var databaseName = "AA";
            //var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            //client.Setup(x => x.WriteAsync(It.IsAny<Point[]>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var collectorEngine = new CollectorEngine(performanceCounterGroup.Object);

            //Act
            await collectorEngine.StartAsync();

            //Assert
            //client.Verify(x => x.WriteAsync(It.IsAny<Point[]>()), Times.Once);
            Assert.Fail("Should assert that the message is enqued.");
        }

        [Test]
        public async void Should_send_data_every_SecondsInterval()
        {
            //Arrange
            var databaseName = "AA";
            //var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            //client.Setup(x => x.WriteAsync(It.IsAny<Point[]>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var collectorEngine = new CollectorEngine(performanceCounterGroup.Object);

            //Act
            await collectorEngine.StartAsync();
            System.Threading.Thread.Sleep(2000);

            //Assert
            //client.Verify(x => x.WriteAsync(It.IsAny<Point[]>()), Times.AtLeast(2));
            Assert.Fail("Should assert that the message is enqued.");
        }
    }
}