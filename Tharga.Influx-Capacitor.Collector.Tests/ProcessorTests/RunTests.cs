using System.Linq;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.ProcessorTests
{
    [TestFixture]
    public class RunTests
    {
        [Test]
        public async void Should_run()
        {
            //Arrange
            var configBusinessMock = new Mock<IConfigBusiness>(MockBehavior.Strict);
            var config = Mock.Of<IConfig>(x => x.Database == Mock.Of<IDatabaseConfig>());
            configBusinessMock.Setup(x => x.LoadFiles(It.IsAny<string[]>())).Returns(config);
            var counterBusinessMock = new Mock<ICounterBusiness>(MockBehavior.Strict);
            counterBusinessMock.Setup(x => x.GetPerformanceCounterGroups(config)).Returns(Mocks.Of<IPerformanceCounterGroup>().Take(10).ToList());
            var influxDbAgentLoaderMock = new Mock<IInfluxDbAgentLoader>(MockBehavior.Strict);
            var influxDbAgentMock = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            influxDbAgentMock.Setup(x => x.PingAsync()).ReturnsAsync(new Pong());
            influxDbAgentLoaderMock.Setup(x => x.GetAgent(config.Database)).Returns(influxDbAgentMock.Object);
            var processor = new Processor(configBusinessMock.Object, counterBusinessMock.Object, influxDbAgentLoaderMock.Object);
            var configFiles = new string[] { };

            //Act
            await processor.RunAsync(configFiles, false);

            //Assert
            counterBusinessMock.Verify(x => x.GetPerformanceCounterGroups(config), Times.Once);
            influxDbAgentMock.Verify(x => x.PingAsync(), Times.Once);
        }
    }
}