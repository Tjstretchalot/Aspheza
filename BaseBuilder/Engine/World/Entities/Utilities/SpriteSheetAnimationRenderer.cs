using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.Entities.Utilities
{
    public class SpriteSheetAnimationRenderer
    {
        private Dictionary<string, Animation> StringToAnimation;

        PointD2D RefKeyPixel;
        string CurrentAnimationName;
        int CurrentAnimationFrame;
        Animation CurrentAnimation;
        Direction Direction;

        int NextAnimationTickMS;
        
        public SpriteSheetAnimationRenderer(PointD2D refKeyPixel, Dictionary<string, Animation> stringToAnimation)
        {
            RefKeyPixel = refKeyPixel;
            StringToAnimation = stringToAnimation;

            CurrentAnimationName = "DownMove";
            CurrentAnimationFrame = 0;
            CurrentAnimation = stringToAnimation["DownMove"];
            NextAnimationTickMS = CurrentAnimation.GetFrameTime(CurrentAnimationFrame);
        }

        public void FromMessage(NetIncomingMessage message)
        {
            CurrentAnimationName = message.ReadString();
            CurrentAnimation = StringToAnimation[CurrentAnimationName];
            CurrentAnimationFrame = message.ReadInt32();
            NextAnimationTickMS = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(CurrentAnimationName);
            message.Write(CurrentAnimationFrame);
            message.Write(NextAnimationTickMS);
        }

        private void UpdateDirection(double dx, double dy)
        {
            if(Math.Sign(dx) == -1 && Direction != Direction.Left)
            {//Left
                Direction = Direction.Left;
                CurrentAnimationName = "LeftMove";
                CurrentAnimation = StringToAnimation["LeftMove"];
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;
            }
            else if (Math.Sign(dx) == 1 && Direction != Direction.Right)
            {//Right
                Direction = Direction.Right;
                CurrentAnimationName = "RightMove";
                CurrentAnimation = StringToAnimation["RightMove"];
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;
            }
            else if (Math.Sign(dy) == 1 && Direction != Direction.Down)
            {//Down
                Direction = Direction.Down;
                CurrentAnimationName = "DownMove";
                CurrentAnimation = StringToAnimation["DownMove"];
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;
            }
            else if (Math.Sign(dy) == -1 && Direction != Direction.Up)
            {//Up
                Direction = Direction.Up;
                CurrentAnimationName = "UpMove";
                CurrentAnimation = StringToAnimation["UpMove"];
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;
            }
        }

        private void UpdateAnimation(int timeMS)
        {
            NextAnimationTickMS -= timeMS;
            if (NextAnimationTickMS <= 0)
            {
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;

                NextAnimationTickMS = CurrentAnimation.GetFrameTime(CurrentAnimationFrame);
            }
        }

        public void UpdateSprite(SharedGameState shareState, int timeMS, double dx, double dy, string SpecialAnimation = null)
        {
            UpdateDirection(dx, dy);

            if (SpecialAnimation != null)
            {
                if (SpecialAnimation.Equals("ChopTree"))
                {
                    switch (Direction)
                    {
                        case Direction.Up:
                            CurrentAnimationName = "ChopTreeUp";
                            CurrentAnimation = StringToAnimation["ChopTreeUp"];
                            break;
                        case Direction.Down:
                            CurrentAnimationName = "ChopTreeDown";
                            CurrentAnimation = StringToAnimation["ChopTreeDown"];
                            break;
                        case Direction.Left:
                            CurrentAnimationName = "ChopTreeLeft";
                            CurrentAnimation = StringToAnimation["ChopTreeLeft"];
                            break;
                        case Direction.Right:
                            CurrentAnimationName = "ChopTreeRight";
                            CurrentAnimation = StringToAnimation["ChopTreeRight"];
                            break;
                    }
                }
                else
                {
                    CurrentAnimationName = SpecialAnimation;
                    CurrentAnimation = StringToAnimation[SpecialAnimation];
                }
                CurrentAnimationFrame = (CurrentAnimationFrame + 1) % CurrentAnimation.FrameCount;
            }
            
            UpdateAnimation(timeMS);
        }

        public void Reset(Direction? direction = null)
        {
            if (direction.HasValue)
            {
                switch (direction.Value)
                {
                    case Direction.Up:
                        CurrentAnimationName = "UpMove";
                        CurrentAnimation = StringToAnimation["UpMove"];
                        break;
                    case Direction.Down:
                        CurrentAnimationName = "DownMove";
                        CurrentAnimation = StringToAnimation["DownMove"];
                        break;
                    case Direction.Left:
                        CurrentAnimationName = "LeftMove";
                        CurrentAnimation = StringToAnimation["LeftMove"];
                        break;
                    case Direction.Right:
                        CurrentAnimationName = "RightMove";
                        CurrentAnimation = StringToAnimation["RightMove"];
                        break;
                }
            }
            else
            {
                switch (Direction)
                {
                    case Direction.Up:
                        CurrentAnimationName = "UpMove";
                        CurrentAnimation = StringToAnimation["UpMove"];
                        break;
                    case Direction.Down:
                        CurrentAnimationName = "DownMove";
                        CurrentAnimation = StringToAnimation["DownMove"];
                        break;
                    case Direction.Left:
                        CurrentAnimationName = "LeftMove";
                        CurrentAnimation = StringToAnimation["LeftMove"];
                        break;
                    case Direction.Right:
                        CurrentAnimationName = "RightMove";
                        CurrentAnimation = StringToAnimation["RightMove"];
                        break;
                }
            }

            CurrentAnimationFrame = 0;
            NextAnimationTickMS = CurrentAnimation.GetFrameTime(CurrentAnimationFrame);
        }

        public void MoveComplete(SharedGameState shareState)
        {
            switch (Direction)
            {
                case Direction.Up:
                    CurrentAnimationName = "UpMove";
                    CurrentAnimation = StringToAnimation["UpMove"];
                    break;
                case Direction.Down:
                    CurrentAnimationName = "DownMove";
                    CurrentAnimation = StringToAnimation["DownMove"];
                    break;
                case Direction.Left:
                    CurrentAnimationName = "LeftMove";
                    CurrentAnimation = StringToAnimation["LeftMove"];
                    break;
                case Direction.Right:
                    CurrentAnimationName = "RightMove";
                    CurrentAnimation = StringToAnimation["RightMove"];
                    break;
            }

            CurrentAnimationFrame = 0;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, int w, int h, Color overlay)
        {
            var frameInfo = CurrentAnimation.GetFrameRenderInfo(context, CurrentAnimationFrame);

            var texture = frameInfo.Item1;
            var drawRec = frameInfo.Item2;
            var keyPixel = frameInfo.Item3;

            var shift = RefKeyPixel - keyPixel;

            var worldWidth = drawRec.Width / 32.0;
            var worldHeight = drawRec.Height / 32.0;
            var endX = screenTopLeft.X + ((shift.X / 32.0) * context.Camera.Zoom) - (worldWidth * ((drawRec.Width - 32) / drawRec.Width) * context.Camera.Zoom);
            var endY = screenTopLeft.Y + ((shift.Y / 32.0) * context.Camera.Zoom) - (worldHeight * ((drawRec.Height - 32) / drawRec.Height) * context.Camera.Zoom);

            context.SpriteBatch.Draw(texture,
                sourceRectangle: drawRec,
                destinationRectangle: new Rectangle(
                    (int)(endX), (int)(endY),
                    (int)(worldWidth * context.Camera.Zoom), (int)(worldHeight * context.Camera.Zoom)
                    ),
                color: overlay);
        }
    }
}
