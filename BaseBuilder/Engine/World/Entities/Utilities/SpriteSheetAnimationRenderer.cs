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
        Animation CurrentAnimation;
        Direction Direction;

        int NextAnimationTickMS;
        
        public SpriteSheetAnimationRenderer(PointD2D refKeyPixel, Dictionary<string, Animation> stringToAnimation)
        {
            RefKeyPixel = refKeyPixel;
            StringToAnimation = stringToAnimation;

            CurrentAnimation = stringToAnimation["DownMove"];
            NextAnimationTickMS = CurrentAnimation.GetFrameTime();
        }

        public void FromMessage(NetIncomingMessage message)
        {
            NextAnimationTickMS = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(NextAnimationTickMS);
        }

        private void UpdateDirection(double dx, double dy)
        {
            var currentFrameHolder = CurrentAnimation.CurrentFrame + 1;
            if(Math.Sign(dx) == -1 && Direction != Direction.Left)
            {//Left
                Direction = Direction.Left;
                CurrentAnimation.CurrentFrame = 0;
                CurrentAnimation = StringToAnimation["LeftMove"];
                CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
            }
            else if (Math.Sign(dx) == 1 && Direction != Direction.Right)
            {//Right
                Direction = Direction.Right;
                CurrentAnimation.CurrentFrame = 0;
                CurrentAnimation = StringToAnimation["RightMove"];
                CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
            }
            else if (Math.Sign(dy) == 1 && Direction != Direction.Down)
            {//Down
                Direction = Direction.Down;
                CurrentAnimation.CurrentFrame = 0;
                CurrentAnimation = StringToAnimation["DownMove"];
                CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
            }
            else if (Math.Sign(dy) == -1 && Direction != Direction.Up)
            {//Up
                Direction = Direction.Up;
                CurrentAnimation.CurrentFrame = 0;
                CurrentAnimation = StringToAnimation["UpMove"];
                CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
            }
        }

        private void UpdateAnimation(int timeMS)
        {
            NextAnimationTickMS -= timeMS;
            if (NextAnimationTickMS <= 0)
            {
                CurrentAnimation.CurrentFrame = (CurrentAnimation.CurrentFrame + 1) % CurrentAnimation.Count();

                NextAnimationTickMS = CurrentAnimation.GetFrameTime();
            }
        }

        public void UpdateSprite(SharedGameState shareState, int timeMS, double dx, double dy, string SpecialAnimation = null)
        {
            UpdateDirection(dx, dy);

            if (SpecialAnimation != null)
            {
                var currentFrameHolder = CurrentAnimation.CurrentFrame;
                if (SpecialAnimation == "ChopTree")
                {
                    switch (Direction)
                    {
                        case Direction.Up:
                            CurrentAnimation.CurrentFrame = 0;
                            CurrentAnimation = StringToAnimation["ChopTreeUp"];
                            CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
                            break;
                        case Direction.Down:
                            CurrentAnimation.CurrentFrame = 0;
                            CurrentAnimation = StringToAnimation["ChopTreeDown"];
                            CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
                            break;
                        case Direction.Left:
                            CurrentAnimation.CurrentFrame = 0;
                            CurrentAnimation = StringToAnimation["ChopTreeLeft"];
                            CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
                            break;
                        case Direction.Right:
                            CurrentAnimation.CurrentFrame = 0;
                            CurrentAnimation = StringToAnimation["ChopTreeRight"];
                            CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
                            break;
                    }
                }
                else
                {
                    CurrentAnimation.CurrentFrame = 0;
                    CurrentAnimation = StringToAnimation[SpecialAnimation];
                    CurrentAnimation.CurrentFrame = currentFrameHolder % CurrentAnimation.Count();
                }
            }
            
            UpdateAnimation(timeMS);
        }

        public void Reset(Direction? direction = null)
        {
            CurrentAnimation.CurrentFrame = 0;
            if (direction.HasValue)
            {
                switch (direction.Value)
                {
                    case Direction.Up:
                        CurrentAnimation = StringToAnimation["UpMove"];
                        break;
                    case Direction.Down:
                        CurrentAnimation = StringToAnimation["DownMove"];
                        break;
                    case Direction.Left:
                        CurrentAnimation = StringToAnimation["LeftMove"];
                        break;
                    case Direction.Right:
                        CurrentAnimation = StringToAnimation["RightMove"];
                        break;
                }
            }
            else
            {
                switch (Direction)
                {
                    case Direction.Up:
                        CurrentAnimation = StringToAnimation["UpMove"];
                        break;
                    case Direction.Down:
                        CurrentAnimation = StringToAnimation["DownMove"];
                        break;
                    case Direction.Left:
                        CurrentAnimation = StringToAnimation["LeftMove"];
                        break;
                    case Direction.Right:
                        CurrentAnimation = StringToAnimation["RightMove"];
                        break;
                }
            }

            CurrentAnimation.CurrentFrame = 0;
            NextAnimationTickMS = CurrentAnimation.GetFrameTime();
        }

        public void MoveComplete(SharedGameState shareState)
        {
            CurrentAnimation.CurrentFrame = 0;
            switch (Direction)
            {
                case Direction.Up:
                    CurrentAnimation = StringToAnimation["UpMove"];
                    break;
                case Direction.Down:
                    CurrentAnimation = StringToAnimation["DownMove"];
                    break;
                case Direction.Left:
                    CurrentAnimation = StringToAnimation["LeftMove"];
                    break;
                case Direction.Right:
                    CurrentAnimation = StringToAnimation["RightMove"];
                    break;
            }
            CurrentAnimation.CurrentFrame = 0;
        }

        public void Render(RenderContext context, int x, int y, int w, int h, Color overlay)
        {
            var frameInfo = CurrentAnimation.GetFrameRenderInfo(context);

            var texture = frameInfo.Item1;
            var drawRec = frameInfo.Item2;
            var keyPixel = frameInfo.Item3;

            var shift = RefKeyPixel - keyPixel;

            var worldWidth = drawRec.Width / 32.0;
            var worldHeight = drawRec.Height / 32.0;
            var endX = x + ((shift.X / 32.0) * context.Camera.Zoom) - (worldWidth * ((drawRec.Width - 32) / drawRec.Width) * context.Camera.Zoom);
            var endY = y + ((shift.Y / 32.0) * context.Camera.Zoom) - (worldHeight * ((drawRec.Height - 32) / drawRec.Height) * context.Camera.Zoom);

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
