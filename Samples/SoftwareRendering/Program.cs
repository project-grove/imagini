using System;
using System.Drawing;
using System.Linq;
using Imagini;
using Imagini.Drawing;

namespace SoftwareRendering
{
    class Program
    {
        static void Main(string[] args)
        {
            (new SampleApp()).Run();
        }
    }

    class SampleApp : App2D
    {
        const int WindowWidth = 400;
        const int WindowHeight = 300;
        const int RectWidth = 50;
        const int RectHeight = 25;
        const int StepPerFrame = 5;

        int PosX = 0;
        int PosY = 0;
        int DirectionX = 1;
        int DirectionY = 1;

        Color BackgroundColor = Color.BurlyWood;
        Color RectangleColor = Color.Sienna;

        public SampleApp() : base(new WindowSettings()
        {
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight
        }, RendererInfo.All.First(p => !p.IsHardwareAccelerated))
        { }

        protected override void Update(TimeSpan frameTime)
        {
            PosX += DirectionX * StepPerFrame;
            PosY += DirectionY * StepPerFrame;
            if (PosX <= 0 || PosX >= WindowWidth - RectWidth)
                DirectionX = -DirectionX;
            if (PosY <= 0 || PosY >= WindowHeight - RectHeight)
                DirectionY = -DirectionY;
        }

        protected override void Draw(TimeSpan frameTime)
        {
            Surface.Fill(BackgroundColor);
            Surface.Fill(RectangleColor, 
                new Rectangle(PosX, PosY, RectWidth, RectHeight));
        }
    }
}
