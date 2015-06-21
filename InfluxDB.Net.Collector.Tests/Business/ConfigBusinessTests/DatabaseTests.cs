using System;
using InfluxDB.Net.Collector.Business;
using InfluxDB.Net.Collector.Interface;
using Moq;
using NUnit.Framework;

namespace InfluxDB.Net.Collector.Tests.Business.ConfigBusinessTests
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void Should_fill_database_configuration()
        {
            //Arrange
            var url = "A";
            var username = "B";
            var password = "C";
            var databaseName = "D";
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<Database><Url>{0}</Url><Username>{1}</Username><Password>{2}</Password><Name>{3}</Name></Database>", url, username, password, databaseName));
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);

            //Act
            var config = configBusiness.LoadFile("myFile.xml");            

            //Assert            
            Assert.That(config.Database, Is.Not.Null);
            Assert.That(config.Database.Url, Is.EqualTo(url));
            Assert.That(config.Database.Username, Is.EqualTo(username));
            Assert.That(config.Database.Password, Is.EqualTo(password));
            Assert.That(config.Database.Name, Is.EqualTo(databaseName));
        }

        [Test]
        public void Should_throw_if_there_is_no_database_configuration_information()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => string.Format("<A></A>"));
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            registryRepositoryMock.Setup(x => x.GetSetting(It.IsAny<RegistryHKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty);
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
            Assert.That(exception.Message, Is.EqualTo("No url provided.\r\nParameter name: url"));
            registryRepositoryMock.Verify(x => x.GetSetting(It.IsAny<RegistryHKey>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public void Should_throw_if_there_are_several_database_configuration_information()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText("File1.xml")).Returns(() => "<Database><Url>A</Url><Username>A</Username><Password>A</Password><Name>A</Name></Database>");
            fileLoaderMock.Setup(x => x.ReadAllText("File2.xml")).Returns(() => "<Database><Url>A</Url><Username>A</Username><Password>A</Password><Name>A</Name></Database>");
            var registryRepositoryMock = new Mock<IRegistryRepository>(MockBehavior.Strict);
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object, registryRepositoryMock.Object);
            IConfig config = null;
            Exception exception = null;

            //Act
            try
            {
                config = configBusiness.LoadFiles(new[] { "File1.xml", "File2.xml" });
            }
            catch (Exception exp)
            {
                exception = exp;
            }

            //Assert            
            Assert.That(config, Is.Null);
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception.Message, Is.EqualTo("There are database configuration sections in more than one file."));
        }
    }
}
