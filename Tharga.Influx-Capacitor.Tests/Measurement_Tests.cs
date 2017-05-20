using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Net.Models;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor;
using Tharga.InfluxCapacitor.Entities;
using Tharga.InfluxCapacitor.Interface;

namespace Tharga.Influx_Capacitor.Tests
{
    [TestFixture]
    class Measurement_Tests
    {
        [Test]
        public void Should_measure_void_function()
        {
            //Arrange
            Point point = null;
            var q = new Mock<IQueue>(MockBehavior.Strict);
            q.Setup(x => x.Enqueue(It.IsAny<Point>())).Callback<Point>(p => { point = p; });
            var mr = new Measure(q.Object);

            //Act
            mr.Execute(m =>
            {
                F();
            });

            //Assert
            Assert.That(point, Is.Not.Null);
            Assert.That(point.Fields, Is.Not.Empty);
            Assert.That(point.Fields.Count, Is.EqualTo(1));
            Assert.That(point.Fields.First().Value, Is.GreaterThan(200));
            Assert.That(point.Fields.First().Value, Is.LessThan(250));
        }

        [Test]
        public async void Should_measure_async_function()
        {
            //Arrange
            Point point = null;
            var q = new Mock<IQueue>(MockBehavior.Strict);
            q.Setup(x => x.Enqueue(It.IsAny<Point>())).Callback<Point>(p => { point = p; });
            var mr = new Measure(q.Object);

            //Act
            await mr.Execute(async m =>
            {
                await FA();
            });

            //Assert
            Assert.That(point, Is.Not.Null);
            Assert.That(point.Fields, Is.Not.Empty);
            Assert.That(point.Fields.Count, Is.EqualTo(1));
            Assert.That(point.Fields.First().Value, Is.GreaterThan(200));
            Assert.That(point.Fields.First().Value, Is.LessThan(250));
        }

        [Test]
        public async void Should_measure_async_function_with_response()
        {
            //Arrange
            Point point = null;
            var q = new Mock<IQueue>(MockBehavior.Strict);
            q.Setup(x => x.Enqueue(It.IsAny<Point>())).Callback<Point>(p => { point = p; });
            var mr = new Measure(q.Object);

            //Act
            var response = await mr.ExecuteAsync(async m =>
            {
                return await Task.Run(async () =>
                {
                    return await FA();
                });
            });

            //Assert
            Assert.That(response, Is.EqualTo("Bob Loblaw"));
            Assert.That(point, Is.Not.Null);
            Assert.That(point.Fields, Is.Not.Empty);
            Assert.That(point.Fields.Count, Is.EqualTo(1));
            Assert.That(point.Fields.First().Value, Is.GreaterThan(200));
            Assert.That(point.Fields.First().Value, Is.LessThan(250));
        }

        private string F()
        {
            Thread.Sleep(200);
            return "Bob Loblaw";
        }

        private async Task<string> FA()
        {
            //return await Task.Run(async () =>
            //{
              Thread.Sleep(200);
              return "Bob Loblaw";
            //});
        }
    }
}
