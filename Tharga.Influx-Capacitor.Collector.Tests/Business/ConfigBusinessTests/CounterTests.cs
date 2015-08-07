using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Business.ConfigBusinessTests
{
    [TestFixture]
    public class CounterTests
    {
        [Test]
        public void Should_throw_if_group_has_no_Name()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<CounterGroup></CounterGroup>"));
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);
            IConfig config = null;
            Exception exception = null;

            //Act
            try
            {
                config = configBusiness.LoadFile("myFile.xml");
            }
            catch (Exception exp)
            {
                exception = exp;
            }

            //Assert            
            Assert.That(config, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("No Name attribute specified for the CounterGroup."));
        }

        [Test]
        public void Should_throw_if_group_has_no_SecondsInterval()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<CounterGroup Name=\"A\"></CounterGroup>"));
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);
            IConfig config = null;
            Exception exception = null;

            //Act
            try
            {
                config = configBusiness.LoadFile("myFile.xml");
            }
            catch (Exception exp)
            {
                exception = exp;
            }

            //Assert            
            Assert.That(config, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("No SecondsInterval attribute specified for the CounterGroup."));
        }

        [Test]
        public void Should_throw_if_group_SecondsInterval_is_not_numeric()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<CounterGroup Name=\"A\" SecondsInterval=\"A\"></CounterGroup>"));
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);
            IConfig config = null;
            Exception exception = null;

            //Act
            try
            {
                config = configBusiness.LoadFile("myFile.xml");
            }
            catch (Exception exp)
            {
                exception = exp;
            }

            //Assert            
            Assert.That(config, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("Cannot parse attribute SecondsInterval value to integer."));
        }

        [Test]
        public void Should_set_all_provided_values_to_the_counter()
        {
            //Arrange
            var counterGroupName = "AA";
            var secondsInterval = 11;
            var counterName = "A";
            var categoryName = "B";
            var instanceName = "C";
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<{0}><Database><Url>X</Url><Username>X</Username><Password>X</Password><Name>X</Name></Database><CounterGroup Name=\"{1}\" SecondsInterval=\"{2}\"><Counter><CounterName>{3}</CounterName><CategoryName>{4}</CategoryName><InstanceName>{5}</InstanceName></Counter></CounterGroup></{0}>", Constants.ServiceName, counterGroupName, secondsInterval, counterName, categoryName, instanceName));
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);
            Exception exception = null;

            //Act
            var config = configBusiness.LoadFile("myFile.xml");

            //Assert            
            Assert.That(config, Is.Not.Null);
            Assert.That(exception, Is.Null);
            Assert.That(config.Groups.Count, Is.EqualTo(1));
            Assert.That(config.Groups.Single().Name, Is.EqualTo(counterGroupName));
            Assert.That(config.Groups.Single().SecondsInterval, Is.EqualTo(secondsInterval));
            Assert.That(config.Groups.Single().Counters.Count(), Is.EqualTo(1));
            Assert.That(config.Groups.Single().Counters.Single().CategoryName, Is.EqualTo(categoryName));
            Assert.That(config.Groups.Single().Counters.Single().CounterName, Is.EqualTo(counterName));
            Assert.That(config.Groups.Single().Counters.Single().InstanceName, Is.EqualTo(instanceName));
        }
    }
}