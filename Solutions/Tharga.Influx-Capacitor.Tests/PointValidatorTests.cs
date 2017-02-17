using System.Collections.Generic;
using System.Linq;
using InfluxDB.Net.Models;
using NUnit.Framework;
using Tharga.InfluxCapacitor;

namespace Tharga.Influx_Capacitor.Tests
{
    [TestFixture]
    public class PointValidatorTests
    {
        [Test]
        public void Should_be_valid()
        {
            //Arrange
            var point = new Point
            {
                Measurement = "x",
                Fields = new Dictionary<string, object> { { "A", "1" } },
                Tags = new Dictionary<string, object> { { "B", "2" } }
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Empty);
        }

        [Test]
        public void Should_be_invalid_if_there_is_no_measurement_name()
        {
            //Arrange
            var point = new Point
            {
                Fields = new Dictionary<string, object> { { "A", "1" } },
                Tags = new Dictionary<string, object> { { "B", "2" } }
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.First(), Is.EqualTo("There is no name for measurement."));
        }

        [Test]
        public void Should_be_invalid_if_there_are_no_fields()
        {
            //Arrange
            var point = new Point
            {
                Measurement = "x",
                Tags = new Dictionary<string, object> { { "B", "2" } }
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.First(), Is.EqualTo("There are no fields for measurement x."));
        }

        [Test]
        public void Should_be_valid_if_there_are_no_tags()
        {
            //Arrange
            var point = new Point
            {
                Measurement = "x",
                Fields = new Dictionary<string, object> { { "A", "1" } },
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Empty);
        }

        [Test]
        public void Should_be_invalid_when_field_value_is_missing()
        {
            //Arrange
            var point = new Point
            {
                Measurement = "x",
                Fields = new Dictionary<string, object> { { "A", null } },
                Tags = new Dictionary<string, object> { { "B", "2" } }
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.First(), Is.EqualTo("Value missing for field A for measurement x."));
        }

        [Test]
        public void Should_be_invalid_when_Tag_value_is_missing()
        {
            //Arrange
            var point = new Point
            {
                Measurement = "x",
                Fields = new Dictionary<string, object> { { "A", "1" } },
                Tags = new Dictionary<string, object> { { "B", null } }
            };
            var validator = new PointValidator();

            //Act
            var response = validator.Validate(point);

            //Assert
            Assert.That(response, Is.Not.Empty);
            Assert.That(response.First(), Is.EqualTo("Value missing for tag B for measurement x."));
        }
    }
}