using System;
using System.Linq;
using InfluxDB.Net.Collector.Business;
using InfluxDB.Net.Collector.Interface;
using Moq;
using NUnit.Framework;

namespace InfluxDB.Net.Collector.Tests.Business.ConfigBusinessTests
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
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);
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
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);
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
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);
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
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<InfluxDB.Net.Collector><Database><Url>X</Url><Username>X</Username><Password>X</Password><Name>X</Name></Database><CounterGroup Name=\"{0}\" SecondsInterval=\"{1}\"><Counter><CounterName>{2}</CounterName><CategoryName>{3}</CategoryName><InstanceName>{4}</InstanceName></Counter></CounterGroup></InfluxDB.Net.Collector>", counterGroupName, secondsInterval, counterName, categoryName, instanceName));
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);
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