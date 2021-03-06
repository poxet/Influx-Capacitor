﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Tharga.InfluxCapacitor.Collector.Business;
using Tharga.InfluxCapacitor.Collector.Entities;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Tests.Business.CounterBusinessTests
{
    [TestFixture]
    public class FilterTests
    {
        [Test]
        public void Should_Apply_Filter_Which_Returns_None()
        {
            // The test filter remove all counters

            // Arrange
            var filterMock = new Mock<ICounterInstanceFilter>();
            var n = new Naming("MyInstance");
            filterMock.Setup(x => x.Execute(n)).Returns(new Naming((string)null));
            var filters = new[] { filterMock.Object };
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(c => c.CategoryName == "Processor" && c.CounterName == counterName && c.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(cg => cg.Name == "A" && cg.Counters == counters && cg.Filters == filters).Take(1).ToList();
            var config = Mock.Of<IConfig>(cfg => cfg.Groups == counterGroup);
            var counterBusiness = new CounterBusiness();

            // Act
            var result = counterBusiness.GetPerformanceCounterGroups(config);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var resultCounters = result.First().GetCounters();
            Assert.That(resultCounters.Count(), Is.EqualTo(0), "Filter should have removed all performances counters");
        }

        [Test]
        public void Should_Apply_Filter_Which_Returns_One()
        {
            // The test filter match one counter : _Total 

            // Arrange
            var filterExpression = new Func<Naming, Naming>(z => z.Name == "_Total" ? z : null);
            var filterMock = new Mock<ICounterInstanceFilter>();
            filterMock.Setup(x => x.Execute(It.IsAny<Naming>())).Returns(filterExpression);
            var filters = new[] { filterMock.Object };
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == counterName && z.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters && y.Filters == filters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new CounterBusiness();

            // Act
            var result = counterBusiness.GetPerformanceCounterGroups(config);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var resultCounters = result.First().GetCounters();
            Assert.That(resultCounters.Count(), Is.EqualTo(1));
            Assert.AreEqual(resultCounters.First().InstanceName.Name, "_Total");
        }

        [Test]
        [Ignore("")]
        public void Should_Apply_Filter_Which_Returns_All()
        {
            // The test filter match all counters
            // For the processor time, it will result in having Environment.ProcessorCount + 1 counters (_Total)

            // Arrange
            var filterExpression = new Func<Naming, Naming>(z => z);
            var filterMock = new Mock<ICounterInstanceFilter>();
            filterMock.Setup(x => x.Execute(It.IsAny<Naming>())).Returns(filterExpression);
            var filters = new[] { filterMock.Object };
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName.Name == "% Processor Time" && z.InstanceName.Name == "*" && z.MachineName == null && z.Max == null && z.Tags == new List<ITag> {}).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters && y.Filters == filters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new CounterBusiness();

            // Act
            var result = counterBusiness.GetPerformanceCounterGroups(config);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var resultCounters = result.First().GetCounters();
            Assert.That(resultCounters.Count(), Is.EqualTo(Environment.ProcessorCount + 1));
        }

        [Test]
        public void Should_Apply_Filter_Which_Returns_Some()
        {
            // The test filter ignores all counters with an instance named "_Total"
            // For the processor time, it will result in having Environment.ProcessorCount counters

            // Arrange
            var filterExpression = new Func<Naming, Naming>(z => z.Name == "_Total" ? null : z);
            var filterMock = new Mock<ICounterInstanceFilter>();
            filterMock.Setup(x => x.Execute(It.IsAny<Naming>())).Returns(filterExpression);
            var filters = new[] { filterMock.Object };
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == counterName && z.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters && y.Filters == filters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new CounterBusiness();

            // Act
            var result = counterBusiness.GetPerformanceCounterGroups(config);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var resultCounters = result.First().GetCounters();
            Assert.That(resultCounters.Count(), Is.EqualTo(Environment.ProcessorCount));
        }

        [Test]
        public void Should_Apply_Replacement_Filter()
        {
            // The test filter to instance named "_Total", and rename "_Total" into "Total"

            // Arrange
            var filterExpression = new Func<Naming, Naming>(z => z.Name == "_Total" ? new Naming("Total") : null);
            var filterMock = new Mock<ICounterInstanceFilter>();
            filterMock.Setup(x => x.Execute(It.IsAny<Naming>())).Returns(filterExpression);
            var filters = new[] { filterMock.Object };
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == counterName && z.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters && y.Filters == filters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new CounterBusiness();

            // Act
            var result = counterBusiness.GetPerformanceCounterGroups(config);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(1));
            var resultCounters = result.First().GetCounters();
            Assert.That(resultCounters.Count(), Is.EqualTo(1));
            Assert.That(resultCounters.First().InstanceName.Name, Is.EqualTo("Total"));
        }

        [Test(Description = "Check that when a replacement filter resuts in multiple instances with the same name, we guarantee uniqueness with an autoincremented suffix")]
        public void Apply_Replacement_Filter_With_Multiple_Names_without_alias()
        {
            // arrange
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == counterName && z.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new PerformanceCounterProvider(null, counter => new[]
            {
                new PerformanceCounterInfo(null, null, new Naming("w3wp"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp"), null, null, null, null, null, null),
            });

            // act
            var result = new[] { counterBusiness.GetGroup(counterGroup.First()) };

            // assert
            Assert.That(result.Length, Is.EqualTo(1));
            var resultCounters = result.First().GetCounters().ToList();
            Assert.That(resultCounters.Count, Is.EqualTo(4));
            Assert.That(resultCounters.First().InstanceName.Name, Is.EqualTo("w3wp"));
            Assert.That(resultCounters.First().InstanceName.Alias, Is.Null);
            Assert.That(resultCounters.ElementAt(1).InstanceName.Name, Is.EqualTo("w3wp#2"));
            Assert.That(resultCounters.ElementAt(1).InstanceName.Alias, Is.Null);
            Assert.That(resultCounters.ElementAt(2).InstanceName.Name, Is.EqualTo("w3wp#3"));
            Assert.That(resultCounters.ElementAt(2).InstanceName.Alias, Is.Null);
            Assert.That(resultCounters.ElementAt(3).InstanceName.Name, Is.EqualTo("w3wp#4"));
            Assert.That(resultCounters.ElementAt(3).InstanceName.Alias, Is.Null);
        }

        [Test]
        public void Apply_Replacement_Filter_With_Multiple_Names_with_alias()
        {
            // arrange
            var counterName = new Naming("% Processor Time");
            var instanceName = new Naming("*");
            var counters = Mocks.Of<ICounter>(z => z.CategoryName == "Processor" && z.CounterName == counterName && z.InstanceName == instanceName).Take(1).ToList();
            var counterGroup = Mocks.Of<ICounterGroup>(y => y.Name == "A" && y.SecondsInterval == 10 && y.Counters == counters).Take(1).ToList();
            var config = Mock.Of<IConfig>(x => x.Groups == counterGroup);
            var counterBusiness = new PerformanceCounterProvider(null, counter => new[]
            {
                new PerformanceCounterInfo(null, null, new Naming("w3wp", "w3wpa"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp", "w3wpa"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp", "w3wpa"), null, null, null, null, null, null),
                new PerformanceCounterInfo(null, null, new Naming("w3wp", "w3wpa"), null, null, null, null, null, null),
            });

            // act
            var result = new[] { counterBusiness.GetGroup(counterGroup.First()) };

            // assert
            Assert.That(result.Length, Is.EqualTo(1));
            var resultCounters = result.First().GetCounters().ToList();
            Assert.That(resultCounters.Count, Is.EqualTo(4));
            Assert.That(resultCounters.First().InstanceName.Name, Is.EqualTo("w3wp"));
            Assert.That(resultCounters.First().InstanceName.Alias, Is.EqualTo("w3wpa"));
            Assert.That(resultCounters.ElementAt(1).InstanceName.Name, Is.EqualTo("w3wp#2"));
            Assert.That(resultCounters.ElementAt(1).InstanceName.Alias, Is.EqualTo("w3wpa#2"));
            Assert.That(resultCounters.ElementAt(2).InstanceName.Name, Is.EqualTo("w3wp#3"));
            Assert.That(resultCounters.ElementAt(2).InstanceName.Alias, Is.EqualTo("w3wpa#3"));
            Assert.That(resultCounters.ElementAt(3).InstanceName.Name, Is.EqualTo("w3wp#4"));
            Assert.That(resultCounters.ElementAt(3).InstanceName.Alias, Is.EqualTo("w3wpa#4"));
        }
    }
}