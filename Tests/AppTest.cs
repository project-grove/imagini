using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
    public class SampleApp : App2D
    {
        public int UpdateCalls { get; private set; }
        public int DrawCalls { get; private set; }

        public bool SimulateSlowRunning { get; set; }

        public SampleApp(bool visible = true) : base(new WindowSettings()
        {
            IsVisible = visible,
            WindowWidth = 100,
            WindowHeight = 50,
            IsResizable = true
        })
        { 
            IsFixedTimeStep = false;
        }

        protected override void Draw(TimeSpan dt) => DrawCalls++;
        protected override void Update(TimeSpan frameTime)
        {
            if (SimulateSlowRunning)
                Thread.Sleep(TargetElapsedTime.Milliseconds + 4);
            UpdateCalls++;
        }
    }

    public class AppTest : TestBase, IDisposable
    {
        private SampleApp app = new SampleApp(visible: false);

        public void Dispose() => app.Dispose();

        [Fact]
        public void ShouldCallUpdateAndDrawAtLeastOneTimePerTick()
        {
            PrintTestName();
            app.UpdateCalls.Should().Be(0, "the app just started");
            app.DrawCalls.Should().Be(0, "the app just started");
            app.Tick();
            app.UpdateCalls.Should().Be(1, "one tick performed");
            app.DrawCalls.Should().Be(1, "one tick performed");
        }

        [Fact]
        public void ShouldNotCallDrawIfSuppressed()
        {
            PrintTestName();
            app.SuppressDraw();
            app.Tick();
            app.UpdateCalls.Should().Be(1, "one tick performed");
            app.DrawCalls.Should().Be(0, "the draw was suppressed");
            app.Tick();
            app.UpdateCalls.Should().Be(2, "second tick performed");
            app.DrawCalls.Should().Be(1, "the next draw should not be suppressed");
        }

        [Fact]
        public void ShouldWorkInFixedTimeStepMode()
        {
            PrintTestName();
            app.IsFixedTimeStep = true;
            for (int i = 1; i <= 5; i++)
            {
                app.Tick();
                app.ElapsedAppTime.Should().BeCloseTo(app.TargetElapsedTime * i, 1,
                    "the app has fixed time step by default");
            }
        }

        [Fact]
        public void ShouldWorkInVariableTimeStepMode()
        {
            PrintTestName();
            app.IsFixedTimeStep = false;
            for (int i = 0; i < 5; i++)
                app.Tick();
            app.ElapsedAppTime.Should().BeCloseTo(TimeSpan.Zero, 1,
                "the app runs as fast as it can");
        }

        [Fact]
        public void ShouldReportSlowRunning()
        {
            PrintTestName();
            app.IsFixedTimeStep = true;
            var frames = 10;
            app.SimulateSlowRunning = true;
            for (var i = 0; i < frames; i++)
                app.Tick();
            app.IsRunningSlowly.Should().BeTrue();
            app.SimulateSlowRunning = false;
            for (var i = 0; i < frames * 5; i++)
                app.Tick();
            app.IsRunningSlowly.Should().BeFalse();
        }

        [Fact]
        public void ShouldNotExitIfCancelled()
        {
            PrintTestName();
            app.IsExited.Should().BeFalse();
            app.IsExiting.Should().BeFalse();
            // let's cancel our first exit request
            EventHandler<AppExitEventArgs> cancel = (sender, e) => e.Cancel = true;
            app.Exiting += cancel;
            app.RequestExit();
            app.IsExiting.Should().BeTrue();
            app.Tick();
            app.IsExited.Should().BeFalse();
            app.IsExiting.Should().BeTrue();
            app.CancelExitRequest();
            app.IsExiting.Should().BeFalse();

            // unwire the cancellation handler and try to exit again
            app.Exiting -= cancel;
            app.RequestExit();
            app.Tick();
            app.IsExited.Should().BeTrue();
        }

        [Fact]
        public void ShouldToggleMouseVisibility()
        {
            PrintTestName();
            app.IsMouseVisible = false;
            app.IsMouseVisible.Should().BeFalse();
            app.IsMouseVisible = true;
            app.IsMouseVisible.Should().BeTrue();
        }

        [Fact]
        public void ShouldReportEventsWhenStateIsModified()
        {
            PrintTestName();
            var expected = new HashSet<string>() {
                "resized",
                "deactivated",
                "activated",
                "closed",
                "disposed"
            };
            var actual = new HashSet<string>();
            app.Window.Show();
            app.Window.Raise();
            Window.OverrideCurrentWith(app.Window);
            app.IsActive.Should().BeTrue();
            // it gets called when the initial size of window is set too
            app.Resized += (s, e) => actual.Add("resized");
            app.Deactivated += (s, e) => actual.Add("deactivated");
            app.Activated += (s, e) => actual.Add("activated");
            app.Exiting += (s, e) => { actual.Add("closed"); e.Cancel = true; };
            app.Disposed += (s, e) => actual.Add("disposed");

            app.Window.Hide(); app.Tick();
            app.Window.Raise(); app.Tick();
            app.RequestExit(); app.Tick();

            Window.OverrideCurrentWith(app.Window);
            app.Dispose();
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ShouldDisallowInvalidTimingValues()
        {
            PrintTestName();
            // TargetElapsedTime
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.TargetElapsedTime = TimeSpan.FromMilliseconds(-1));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.TargetElapsedTime = app.InactiveSleepTime
                    .Add(TimeSpan.FromMilliseconds(1)));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.TargetElapsedTime = app.MaxElapsedTime
                    .Add(TimeSpan.FromMilliseconds(1)));
            // InactiveSleepTime
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.InactiveSleepTime = TimeSpan.FromMilliseconds(-1));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.InactiveSleepTime = app.TargetElapsedTime
                    .Subtract(TimeSpan.FromMilliseconds(1)));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.InactiveSleepTime = app.MaxElapsedTime
                    .Add(TimeSpan.FromMilliseconds(1)));
            // MaxElapsedTime
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.MaxElapsedTime = TimeSpan.FromMilliseconds(-1));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.MaxElapsedTime = app.TargetElapsedTime
                    .Subtract(TimeSpan.FromMilliseconds(1)));
            Assert.ThrowsAny<ArgumentOutOfRangeException>(() => 
                app.MaxElapsedTime = app.InactiveSleepTime
                    .Subtract(TimeSpan.FromMilliseconds(1)));
            
            // Check valid values
            app.MaxElapsedTime += TimeSpan.FromMilliseconds(1);
            app.InactiveSleepTime += TimeSpan.FromMilliseconds(1);
            app.TargetElapsedTime += TimeSpan.FromMilliseconds(1);
        }

        [Fact]
        public void ShouldResetElapsedTimeIfRequested()
        {
            PrintTestName();
            app.IsFixedTimeStep = true;
            app.ElapsedAppTime.Should().Be(TimeSpan.Zero);
            app.Tick();
            app.ElapsedAppTime.Should().BeCloseTo(app.TargetElapsedTime, 1);
            app.ResetElapsedTime();
            app.ElapsedAppTime.Should().BeCloseTo(TimeSpan.Zero);
        }
    }
}