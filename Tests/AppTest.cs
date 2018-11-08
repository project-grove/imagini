using System;
using System.Collections.Generic;
using FluentAssertions;
using Imagini;
using Xunit;

namespace Tests
{
    public class SampleApp : App2D
    {
        public int UpdateCalls { get; private set; }
        public int DrawCalls { get; private set; }

        public SampleApp() : base(new WindowSettings()
        {
            IsVisible = false,
            WindowWidth = 100,
            WindowHeight = 50
        }) {}

        protected override void Draw(TimeSpan dt) => DrawCalls++;
        protected override void Update(TimeSpan frameTime) => UpdateCalls++;
    }

    public class AppTest : IDisposable
    {
        private SampleApp app = new SampleApp();

        public void Dispose() => app.Dispose();

        [Fact]
        public void ShouldCallUpdateAndDrawAtLeastOneTimePerTick()
        {
            app.UpdateCalls.Should().Be(0, "the app just started");
            app.DrawCalls.Should().Be(0, "the app just started");
            app.Tick();
            app.UpdateCalls.Should().Be(1, "one tick performed");
            app.DrawCalls.Should().Be(1, "one tick performed");
        }

        [Fact]
        public void ShouldNotCallDrawIfSuppressed()
        {
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
            app.IsFixedTimeStep = false;
            for (int i = 0; i < 5; i++)
                app.Tick();
            app.ElapsedAppTime.Should().BeCloseTo(TimeSpan.Zero, 1,
                "the app runs as fast as it can");
        }

        [Fact]
        public void ShouldNotExitIfCancelled()
        {
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
            app.IsMouseVisible = false;
            app.IsMouseVisible.Should().BeFalse();
            app.IsMouseVisible = true;
            app.IsMouseVisible.Should().BeTrue();
        }

        [Fact]
        public void ShouldReportEventsWhenStateIsModified()
        {
            var app = new SampleApp();
            var expected = new List<string>() {
                "activated",
                "deactivated",
                "closed",
                "disposed"
            };

            // TODO Wire events and check if they are fired

            app.Dispose();
        }

        // TODO Write more tests to ensure high coverage
    }
}