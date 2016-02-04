using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo> { new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")) });
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

        [Test]
        public void Should_send_multiple_points_when_not_using_fieldname()
        {
            //Arrange
            string databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.RefreshInstanceInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("cpu");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo>
            {
                new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total")),
                new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Idle Time", "_Total"))
            });
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
            Assert.That(response, Is.EqualTo(2));
        }

        [Test]
        public void Should_send_one_point_when_using_fieldname()
        {
            //Arrange
            string databaseName = "AA";
            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.RefreshInstanceInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("cpu");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo>
            {
                new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Processor Time", "_Total"), "processor_pct_active", null, null, null),
                new PerformanceCounterInfo(string.Empty, new PerformanceCounter("Processor", "% Idle Time", "_Total"), "processor_pct_idle", null, null, null)
            });
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
        public void Should_limit_value_when_max_reached()
        {
            //Arrange
            string databaseName = "AA";
            float value = 100f;
            var enqueueCallback = new Action<Point[]>(px => value = (float)px.First().Fields.First().Value);
            var perfCounter = new PerformanceCounter("Processor", "Interrupts/sec", "_Total");
            perfCounter.NextValue(); // we have to read the counter at least one time to ensure we have a value

            var performanceCounterGroupMock = new Mock<IPerformanceCounterGroup>(MockBehavior.Strict);
            performanceCounterGroupMock.SetupGet(x => x.SecondsInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.RefreshInstanceInterval).Returns(1);
            performanceCounterGroupMock.SetupGet(x => x.Name).Returns("cpu");
            performanceCounterGroupMock.Setup(x => x.GetFreshCounters()).Returns(new List<IPerformanceCounterInfo>
            {
                new PerformanceCounterInfo(string.Empty, perfCounter, null, null, null, 100f),
            });
            performanceCounterGroupMock.SetupGet(x => x.Tags).Returns(new ITag[0]);

            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            sendBusinessMock
                .Setup(x => x.Enqueue(It.IsAny<Point[]>()))
                .Callback(enqueueCallback);

            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            tagLaoderMock
                .Setup(x => x.GetGlobalTags())
                .Returns(new[] { Mock.Of<ITag>(x => x.Name == "B") });

            var collectorEngine = new ExactCollectorEngine(performanceCounterGroupMock.Object, sendBusinessMock.Object, tagLaoderMock.Object, false);

            //Act
            var response = collectorEngine.CollectRegisterCounterValuesAsync().Result;

            //Assert
            tagLaoderMock.Verify(x => x.GetGlobalTags(), Times.Once);
            sendBusinessMock.Verify(x => x.Enqueue(It.IsAny<Point[]>()), Times.Once);
            Assert.That(value, Is.LessThanOrEqualTo(100f));
        }
    }
}