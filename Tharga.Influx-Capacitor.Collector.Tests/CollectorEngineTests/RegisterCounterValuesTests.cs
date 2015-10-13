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
            performanceCounterGroupMock.SetupGet(x => x.RefreshInstanceInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total"), null, null) });
            performanceCounterGroupMock.SetupGet(x => x.Tags).Returns(new ITag[] { });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            sendBusinessMock.Setup(x => x.Enqueue(It.IsAny<Point[]>()));
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            tagLaoderMock.Setup(x => x.GetGlobalTags()).Returns(new[] { Mock.Of<ITag>(x => x.Name == "B") });
            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            var response = collectorEngine.CollectRegisterCounterValuesAsync().Result;

            //Assert
            tagLaoderMock.Verify(x => x.GetGlobalTags(), Times.Once);
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
            Assert.That(response, Is.EqualTo(1));
        }

        [Test]
        public void Should_not_send_data_to_database_if_there_are_no_counters()
        {
            //Arrange
            string databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.RefreshInstanceInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("A");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { });
            performanceCounterGroupMock.SetupGet(x => x.Tags).Returns(new ITag[] { });
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            sendBusinessMock.Setup(x => x.Enqueue(It.IsAny<Point[]>()));
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            tagLaoderMock.Setup(x => x.GetGlobalTags()).Returns(new[] { Mock.Of<ITag>(x => x.Name == "B") });
            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            var response = collectorEngine.CollectRegisterCounterValuesAsync().Result;

            //Assert
            tagLaoderMock.Verify(x => x.GetGlobalTags(), Times.Once);
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
            Assert.That(response, Is.EqualTo(0));
        }
    }
}