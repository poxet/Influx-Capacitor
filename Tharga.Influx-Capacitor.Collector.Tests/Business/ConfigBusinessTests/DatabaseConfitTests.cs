using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Business.ConfigBusinessTests
{
    [TestFixture]
    public class DatabaseConfitTests
    {
        //TODO: Try opening config when the folder does not exists
        [Test]
        public void Should_create_folder_when_it_does_not_exist()
        {
            //Arrange
            var directoryPath = "ABC";
            var doesFolderExist = false;
            var doesFileExist = true;
            var fileLoaderMock = new Mock<IFileLoaderAgent>(MockBehavior.Strict);
            fileLoaderMock.Setup(x => x.GetApplicationFolderPath()).Returns(directoryPath);
            fileLoaderMock.Setup(x => x.DoesDirectoryExist(directoryPath)).Returns(() => doesFolderExist).Callback(() => { doesFolderExist = true; });
            fileLoaderMock.Setup(x => x.DoesFileExist(It.IsAny<string>())).Returns(() => doesFileExist).Callback(() => { doesFileExist = false; }); ;
            fileLoaderMock.Setup(x => x.CreateDirectory(directoryPath));
            fileLoaderMock.Setup(x => x.DeleteFile(It.IsAny<string>()));
            fileLoaderMock.Setup(x => x.WriteAllText(It.IsAny<string>(), It.IsAny<string>()));
            fileLoaderMock.Setup(x => x.ReadAllText(It.IsAny<string>())).Returns("<a/>");
            var configBusiness = new ConfigBusiness(fileLoaderMock.Object);

            //Act
            var config = configBusiness.OpenDatabaseConfig();

            //Assert
            Assert.That(config, Is.Not.Null);
            Assert.That(config.Url, Is.EqualTo(Constants.NoConfigUrl));
            Assert.That(config.Username, Is.EqualTo(null));
            Assert.That(config.Password, Is.EqualTo(null));
            Assert.That(config.Name, Is.EqualTo(null));
            fileLoaderMock.Verify(x => x.CreateDirectory(directoryPath), Times.Once);
        }

        //TODO: Try opening config when the folder exists
        //TODO: Try saving config when the folder exists
        //TODO: Try saving config when the folder does not exists
    }
}