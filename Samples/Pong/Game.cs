using System;
using System.Drawing;
using Imagini;

namespace Pong
{
    public class Game : App2D
    {
        // Logical size
        const int ScreenWidth = 320;
        const int ScreenHeight = 240;

        // Actual size in pixels
        const int WindowWidth = 800;
        const int WindowHeight = 600;

        // Gameplay-related values
        const int PadWidth = 10;
        const int PadHeight = 50;
        const int PadSpeed = 2;
        const int BallSpeed = 2;
        const int BallSize = 10;
        const int PadStartPosition = (ScreenHeight - PadHeight) / 2;
        const int BallStartX = (ScreenWidth - BallSize) / 2;
        const int BallStartY = (ScreenHeight - BallSize) / 2;

        /* Game state */
        bool Paused = true;

        // Position of the pads' centers
        int LeftPadPosition;
        int LeftPadDirection = 0;
        int RightPadPosition;
        int RightPadDirection = 0;
        // Ball position
        int BallX;
        int BallY;
        int BallXDirection = 1;
        int BallYDirection = 1;
        // Scores
        int LeftScore = 0;
        int RightScore = 0;

        Rectangle BallRectangle => new Rectangle(
            BallX - BallSize / 2,
            BallY - BallSize / 2,
            BallSize,
            BallSize);

        Rectangle LeftPadRectangle => new Rectangle(
            0,
            LeftPadPosition,
            PadWidth,
            PadHeight
        );

        Rectangle RightPadRectangle => new Rectangle(
            ScreenWidth - PadWidth,
            RightPadPosition,
            PadWidth,
            PadHeight
        );

        public Game() : base(new WindowSettings()
        {
            WindowWidth = WindowWidth,
            WindowHeight = WindowHeight,
            Title = "SPACE to pause, W/S - left pad, Up/Down - right pad",
        })
        {
            Graphics.SetLogicalSize(ScreenWidth, ScreenHeight);
            ResetPositions();
            Events.Keyboard.KeyPressed += KeyPressed;
            Events.Keyboard.KeyReleased += KeyReleased;
        }

        protected override void Update(TimeSpan frameTime)
        {
            base.Update(frameTime);
            if (Paused) return;

            LeftPadPosition += LeftPadDirection * PadSpeed;
            RightPadPosition += RightPadDirection * PadSpeed;
            LeftPadPosition = Math.Clamp(LeftPadPosition, 0, ScreenHeight - PadHeight);
            RightPadPosition = Math.Clamp(RightPadPosition, 0, ScreenHeight - PadHeight);

            BallX += BallXDirection * BallSpeed;
            BallY += BallYDirection * BallSpeed;
            if (BallY <= BallSize || BallY >= ScreenHeight - BallSize)
                BallYDirection = -BallYDirection;

            if (LeftPadRectangle.IntersectsWith(BallRectangle) ||
                RightPadRectangle.IntersectsWith(BallRectangle))
            {
                BallXDirection = -BallXDirection;
                BallX += BallXDirection * PadWidth;
            }

            if (BallX < PadWidth)
            {
                RightScore++;
                ResetPositions();
                Paused = true;
            }

            if (BallX > ScreenWidth - PadWidth)
            {
                LeftScore++;
                ResetPositions();
                Paused = true;
            }
        }

        private void KeyPressed(object sender, KeyboardEventArgs args)
        {
            switch(args.Key.Scancode)
            {
                case Scancode.W:
                    LeftPadDirection = -1; break;
                case Scancode.S:
                    LeftPadDirection = 1; break;
                case Scancode.UP:
                    RightPadDirection = -1; break;
                case Scancode.DOWN:
                    RightPadDirection = 1; break;
            }
        }

        private void KeyReleased(object sender, KeyboardEventArgs args)
        {
            switch(args.Key.Scancode)
            {
                case Scancode.W:
                case Scancode.S:
                    LeftPadDirection = 0;
                    break;
                case Scancode.UP:
                case Scancode.DOWN:
                    RightPadDirection = 0;
                    break;
                case Scancode.SPACE:
                    Paused = !Paused;
                    break;
            }
        }

        protected override void Draw(TimeSpan frameTime)
        {
            Graphics.SetDrawingColor(Color.Black);
            Graphics.Clear();

            Graphics.SetDrawingColor(Color.White);
            Graphics.FillRect(BallRectangle);
            Graphics.FillRect(LeftPadRectangle);
            Graphics.FillRect(RightPadRectangle);

            // Draw scores as vertical lines
            for (int i = 0; i < LeftScore; i++)
                Graphics.DrawLine(40 + i * 2, 10, 40 + i * 2, 20);
            for (int i = 0; i < RightScore; i++)
                Graphics.DrawLine(ScreenWidth - 40 - i * 2, 10, ScreenWidth - 40 - i * 2, 20);
        }

        void ResetPositions()
        {
            LeftPadPosition = RightPadPosition = PadStartPosition;
            BallX = BallStartX;
            BallY = BallStartY;
        }
    }
}