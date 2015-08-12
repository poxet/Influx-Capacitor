using System.Collections.Generic;
using System.Diagnostics;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Handlers;
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
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var collectorEngine = new CollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object);

            //Act
            collectorEngine.CollectRegisterCounterValuesAsync().Wait();

            //Assert
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
        }

        [Test]
        public void Should_not_send_data_to_database_if_there_are_no_counters()
        {
            //Arrange
            string databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.SetupGet(x => x.PerformanceCounterInfos).Returns(new List<IPerformanceCounterInfo> { });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var collectorEngine = new CollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object);

            //Act
            collectorEngine.CollectRegisterCounterValuesAsync().Wait();

            //Assert
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
        }
    }
}