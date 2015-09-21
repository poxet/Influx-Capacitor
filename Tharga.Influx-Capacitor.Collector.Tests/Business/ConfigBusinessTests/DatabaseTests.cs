using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Business.ConfigBusinessTests
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
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);

            //Act
            var config = configBusiness.LoadFile("myFile.xml");

            //Assert    
            var database = config.Databases.First();
            Assert.That(database, Is.Not.Null);
            Assert.That(database.Url, Is.EqualTo(url));
            Assert.That(database.Username, Is.EqualTo(username));
            Assert.That(database.Password, Is.EqualTo(password));
            Assert.That(database.Name, Is.EqualTo(databaseName));
        }

        [Test]
        public void Should_not_throw_if_there_is_no_database_configuration_information()
        {
            //Arrange
            var folderPath = "ABC";
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.GetApplicationFolderPath()).Returns(folderPath);
            fileLoaderMock.Setup(x => x.DoesDirectoryExist(folderPath)).Returns(true);
            fileLoaderMock.Setup(x => x.DoesFileExist(It.IsAny<string>())).Returns(false);
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns(() => "<A></A>");
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);

            //Act
            var config = configBusiness.LoadFile("myFile.xml");

            //Assert
            Assert.That(config, Is.Not.Null);
        }

        [Test]
        public void Should_throw_if_there_are_several_database_configuration_information()
        {
            //Arrange
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.ReadAllText("File1.xml")).Returns(() => "<Database><Url>A</Url><Username>A</Username><Password>A</Password><Name>A</Name></Database>");
            fileLoaderMock.Setup(x => x.ReadAllText("File2.xml")).Returns(() => "<Database><Url>A</Url><Username>A</Username><Password>A</Password><Name>A</Name></Database>");
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);
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
            Assert.That(config, Is.Not.Null);
            Assert.That(exception, Is.Null);
            Assert.That(config.Databases.Count, Is.EqualTo(2));
        }
    }
}