using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net.Models;
using NUnit.Framework;
using Tharga.InfluxCapacitor;

namespace Tharga.Influx_Capacitor.Tests
{
    [TestFixture]
    public class PointValidatorCleanTests
    {
        [Test]
        public void Should_return_all_if_all_are_valid()
        {
            //Arrange
            var points = new[]
            {
                new Point
                {
                    Measurement = "x",
                    Fields = new Dictionary<string, object> { { "A", "1" } },
                    Tags = new Dictionary<string, object> { { "B", "2" } }
                },
                new Point
                {
                    Measurement = "x",
                    Fields = new Dictionary<string, object> { { "A", "1" } },
                    Tags = new Dictionary<string, object> { { "B", "2" } }
                },
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Clean(points);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.Count(), Is.EqualTo(points.Length));
        }

        [Test]
        public void Should_return_none_if_none_are_valud()
        {
            //Arrange
            var points = new[]
            {
                new Point
                {
                },
                new Point
                {
                },
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Clean(points);

            //Assert
            Assert.That(response, Is.Empty);
        }

        [Test]
        public void Should_return_some_if_some_are_valid()
        {
            //Arrange
            var points = new[]
            {
                new Point
                {
                    Measurement = "x",
                    Fields = new Dictionary<string, object> { { "A", "1" } },
                    Tags = new Dictionary<string, object> { { "B", "2" } }
                },
                new Point
                {
                },
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Clean(points);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.Count(), Is.EqualTo(1));
        }

    }
}