using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using InfluxDB.Net.Collector.Entities;
using InfluxDB.Net.Collector.Interface;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;

namespace InfluxDB.Net.Collector.Tests.CollectorEngineTests
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
            client.Setup(x => x.WriteAsync(It.IsAny<TimeUnit>(), It.IsAny<Serie>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var collectorEngine = new CollectorEngine(client.Object, databaseName, performanceCounterGroup.Object, false);

            //Act
            collectorEngine.RegisterCounterValuesAsync().Wait();

            //Assert
            client.Verify(x => x.WriteAsync(It.IsAny<TimeUnit>(), It.IsAny<Serie>()), Times.Once);
        }

        [Test]
        public void Should_not_send_data_to_database_if_there_are_no_counters()
        {
            //Arrange
            string databaseName = "AA";
            var client = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            client.Setup(x => x.WriteAsync(It.IsAny<TimeUnit>(), It.IsAny<Serie>())).ReturnsAsync(new InfluxDbApiResponse(HttpStatusCode.Accepted, string.Empty));
            var performanceCounterGroup = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroup.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroup.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroup.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { });
            var collectorEngine = new CollectorEngine(client.Object, databaseName, performanceCounterGroup.Object, false);

            //Act
            collectorEngine.RegisterCounterValuesAsync().Wait();

            //Assert
            client.Verify(x => x.WriteAsync(It.IsAny<TimeUnit>(), It.IsAny<Serie>()), Times.Never);
        }
    }
}