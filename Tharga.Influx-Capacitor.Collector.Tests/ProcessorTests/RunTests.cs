using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Handlers;
using Tharga.InfluxCapacitor.Collector.Interface;
using Tharga.Influx_Capacitor.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.ProcessorTests
{
    [TestFixture]
    public class RunTests
    {
        [Test]
        [Ignore("")]
        public async Task Should_run()
        {
            //Arrange
            var configBusinessMock = new Mock<IConfigBusiness>(MockBehavior.Strict);
            var config = Mock.Of<IConfig>(x => x.Databases == Mocks.Of<IDatabaseConfig>().Take(1));
            configBusinessMock.Setup(x => x.LoadFiles(It.IsAny<string[]>())).Returns(config);
            var counterBusinessMock = new Mock<ICounterBusiness>(MockBehavior.Strict);
            counterBusinessMock.Setup(x => x.GetPerformanceCounterGroups(config)).Returns(Mocks.Of<IPerformanceCounterGroup>().Take(10).ToList());
            var publisherBusinessMock = new Mock<IPublisherBusiness>(MockBehavior.Strict);
            var influxDbAgentLoaderMock = new Mock<IInfluxDbAgentLoader>(MockBehavior.Strict);
            var influxDbAgentMock = new Mock<IInfluxDbAgent>(MockBehavior.Strict);
            influxDbAgentMock.Setup(x => x.PingAsync()).ReturnsAsync(new Pong());
            influxDbAgentLoaderMock.Setup(x => x.GetAgent(config.Databases.First())).Returns(influxDbAgentMock.Object);
            var sendBusinessMock = new Mock<ISendBusiness>(MockBehavior.Strict);
            var tagLaoderMock = new Mock<ITagLoader>(MockBehavior.Strict);
            var processor = new Processor(configBusinessMock.Object, counterBusinessMock.Object, publisherBusinessMock.Object, sendBusinessMock.Object, tagLaoderMock.Object);
            var configFiles = new string[] { };

            //Act
            await processor.RunAsync(configFiles);

            //Assert
            counterBusinessMock.Verify(x => x.GetPerformanceCounterGroups(config), Times.Once);
            influxDbAgentMock.Verify(x => x.PingAsync(), Times.Once);
        }
    }
}