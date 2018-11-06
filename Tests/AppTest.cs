using System;
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
    }
}