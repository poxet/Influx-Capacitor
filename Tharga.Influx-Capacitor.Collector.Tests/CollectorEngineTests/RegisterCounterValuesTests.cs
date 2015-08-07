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
    public class RegisterCounterValuesTests
    {
        [Test]
        public void Should_send_data_to_database()
        {
            //Arrange
            string databaseName = "AA";
            var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            client.Setup(x => x.WriteAsync(It.IsAny<Point[]>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var collectorEngine = new CollectorEngine(client.Object, databaseName, performanceCounterGroup.Object, false);

            //Act
            collectorEngine.RegisterCounterValuesAsync().Wait();

            //Assert
            client.Verify(x => x.WriteAsync(It.IsAny<Point[]>()), Times.Once);
        }

        [Test]
        public void Should_not_send_data_to_database_if_there_are_no_counters()
        {
            //Arrange
            string databaseName = "AA";
            var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            client.Setup(x => x.WriteAsync(It.IsAny<Point[]>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { });
            var collectorEngine = new CollectorEngine(client.Object, databaseName, performanceCounterGroup.Object, false);

            //Act
            collectorEngine.RegisterCounterValuesAsync().Wait();

            //Assert
            client.Verify(x => x.WriteAsync(It.IsAny<Point[]>()), Times.Never);
        }
    }
}