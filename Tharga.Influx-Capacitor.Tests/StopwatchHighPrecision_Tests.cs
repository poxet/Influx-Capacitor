using System;
using System.Threading;
using NUnit.Framework;

namespace Tharga.Influx_Capacitor.Tests
{
    [TestFixture]
    public class StopwatchHighPrecision_Tests
    {
        [Test]
        public void Should_not_give_any_time_when_not_started()
        {
            //Arrange
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();

            //Act
            Thread.Sleep(100);

            //Assert
            Assert.That(sw.ElapsedTotal, Is.EqualTo(0));
            Assert.That(sw.ElapsedSegment, Is.EqualTo(0));
        }

        [Test]
        public void Should_give_time_when_started()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();

            //Act
            sw.Start();
            Thread.Sleep(waitTime);

            //Assert
            var segment = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.LessThan(waitTime * 2));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.LessThan(waitTime * 2));
        }

        [Test]
        public void Should_not_continue_when_stopped()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            sw.Stop();

            //Act
            Thread.Sleep(waitTime*2);

            //Assert
            var segment = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.LessThan(waitTime * 2));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.LessThan(waitTime * 2));
        }

        [Test]
        public void Should_get_new_segments_when_running()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            var segment1 = sw.ElapsedSegment;

            //Act
            var segment2 = sw.ElapsedSegment;

            //Assert
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.LessThan(waitTime * 2));
            Assert.That(new TimeSpan(segment2).TotalMilliseconds, Is.LessThan(waitTime));
        }

        [Test]
        public void Should_not_get_new_segments_when_never_started()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            //sw.Start();
            Thread.Sleep(waitTime);

            //Act
            var segment1 = sw.ElapsedSegment;

            //Assert
            var segment2 = sw.ElapsedSegment;
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.EqualTo(0));
            Assert.That(new TimeSpan(segment2).TotalMilliseconds, Is.LessThan(waitTime));
        }

        [Test]
        public void Should_not_get_new_segments_when_stopped()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            sw.Stop();
            Thread.Sleep(waitTime);

            //Act
            var segment1 = sw.ElapsedSegment;

            //Assert
            var segment2 = sw.ElapsedSegment;
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.GreaterThanOrEqualTo(waitTime));
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.LessThan(waitTime * 2));
            Assert.That(new TimeSpan(segment2).TotalMilliseconds, Is.LessThan(waitTime));
        }

        [Test]
        public void When_resetting_and_the_timer_is_never_started()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            Thread.Sleep(waitTime);

            //Act
            sw.Reset();
            Thread.Sleep(waitTime);

            //Assert
            var segment1 = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.EqualTo(0));
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.EqualTo(0));
        }

        [Test]
        public void When_resetting_and_the_timer_is_started()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);

            //Act
            sw.Reset();

            //Assert
            var segment1 = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.LessThan(waitTime));
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.LessThan(waitTime));
        }

        [Test]
        public void When_resetting_and_the_timer_is_stopped()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            sw.Stop();

            //Act
            sw.Reset();
            Thread.Sleep(waitTime);

            //Assert
            var segment1 = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.EqualTo(0));
            Assert.That(new TimeSpan(segment1).TotalMilliseconds, Is.EqualTo(0));
        }

        [Test]
        public void Should_not_continue_when_paused()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            sw.Pause();

            //Act
            Thread.Sleep(waitTime * 2);

            //Assert
            var segment = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.LessThan(waitTime * 2));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.GreaterThan(waitTime));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.LessThan(waitTime * 2));
        }

        [Test]
        public void Should_continue_on_when_resumed_after_pause()
        {
            //Arrange
            var waitTime = 100;
            var sw = new Tharga.InfluxCapacitor.Entities.StopwatchHighPrecision();
            sw.Start();
            Thread.Sleep(waitTime);
            sw.Pause();
            Thread.Sleep(waitTime);
            sw.Resume();

            //Act
            Thread.Sleep(waitTime);

            //Assert
            var segment = sw.ElapsedSegment;
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.GreaterThan(waitTime*2));
            Assert.That(new TimeSpan(sw.ElapsedTotal).TotalMilliseconds, Is.LessThan(waitTime * 3));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.GreaterThan(waitTime*2));
            Assert.That(new TimeSpan(segment).TotalMilliseconds, Is.LessThan(waitTime * 3));
        }
    }
}