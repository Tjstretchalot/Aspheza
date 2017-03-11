using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
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
        private Dictionary<string, Tuple<string, List<Rectangle>>> StringToSourceFileAndRecList;

        string SpriteSheetName;
        string CurrentAnimation;
        List<Rectangle> CurrentRecList;
        Rectangle CurrentRect;
        int CurrentDraw;
        Direction Direction;

        int NextAnimationTickMS;
        
        public SpriteSheetAnimationRenderer(Dictionary<string, Tuple<string, List<Rectangle>>> stringToSourceRecList)
        {

            StringToSourceFileAndRecList = stringToSourceRecList;

            CurrentAnimation = "DownMove";
            SpriteSheetName = StringToSourceFileAndRecList["DownMove"].Item1;
            CurrentRecList = StringToSourceFileAndRecList["DownMove"].Item2;
            CurrentRect = CurrentRecList[0];
            CurrentDraw = 0;
        }

        public void FromMessage(NetIncomingMessage message)
        {
            int crX = message.ReadInt32();
            int crY = message.ReadInt32();
            int crW = message.ReadInt32();
            int crH = message.ReadInt32();

            CurrentRect = new Rectangle(crX, crY, crW, crH);

            CurrentAnimation = message.ReadString();
            CurrentRecList = StringToSourceFileAndRecList[CurrentAnimation].Item2;
            NextAnimationTickMS = message.ReadInt32();
            CurrentDraw = message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(CurrentRect.X);
            message.Write(CurrentRect.Y);
            message.Write(CurrentRect.Width);
            message.Write(CurrentRect.Height);

            message.Write(CurrentAnimation);
            message.Write(NextAnimationTickMS);
            message.Write(CurrentDraw);
        }

        private void UpdateDirection(double dx, double dy)
        {
            if(Math.Sign(dx) == -1 && Direction != Direction.Left)
            {
                CurrentDraw = (CurrentDraw + 1) % CurrentRecList.Count;
                Direction = Direction.Left;
                CurrentAnimation = "LeftMove";
                CurrentRecList = StringToSourceRecList["LeftMove"];
                CurrentRect = CurrentRecList[CurrentDraw];
            }
            else if (Math.Sign(dx) == 1 && Direction != Direction.Right)
            {
                CurrentDraw = (CurrentDraw + 1) % CurrentRecList.Count;
                Direction = Direction.Right;
                CurrentAnimation = "RightMove";
                CurrentRecList = StringToSourceRecList["RightMove"];
                CurrentRect = CurrentRecList[CurrentDraw];
            }
            else if (Math.Sign(dy) == 1 && Direction != Direction.Down)
            {
                CurrentDraw = (CurrentDraw + 1) % CurrentRecList.Count;
                Direction = Direction.Down;
                CurrentAnimation = "DownMove";
                CurrentRecList = StringToSourceRecList["DownMove"];
                CurrentRect = CurrentRecList[CurrentDraw];
            }
            else if (Math.Sign(dy) == -1 && Direction != Direction.Up)
            {
                CurrentDraw = (CurrentDraw + 1) % CurrentRecList.Count;
                Direction = Direction.Up;
                CurrentAnimation = "UpMove";
                CurrentRecList = StringToSourceRecList["UpMove"];
                CurrentRect = CurrentRecList[CurrentDraw];
            }
        }

        private void UpdateAnimation(int timeMS)
        {
            NextAnimationTickMS -= timeMS;
            if (NextAnimationTickMS <= 0)
            {
                NextAnimationTickMS = 250;

                CurrentDraw = (CurrentDraw + 1) % CurrentRecList.Count;
                CurrentRect = CurrentRecList[CurrentDraw];
            }
        }

        public void UpdateSprite(SharedGameState shareState, int timeMS, double dx, double dy, string SpecialAnimation = null)
        {
            UpdateDirection(dx, dy);
            switch (SpecialAnimation)
            {
                case "ChopWood":
                    SpriteSheetName = StringToSourceFileAndRecList["ChopWood"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["ChopWood"].Item2;
                    CurrentRect = CurrentRecList[CurrentDraw];
                    break;
                case "ChopTreeUp":
                    SpriteSheetName = StringToSourceFileAndRecList["ChopTreeUp"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["ChopTreeUp"].Item2;
                    CurrentRect = CurrentRecList[CurrentDraw];
                    break;
                case "ChopTreeDown":
                    SpriteSheetName = StringToSourceFileAndRecList["ChopTreeDown"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["ChopTreeDown"].Item2;
                    CurrentRect = CurrentRecList[CurrentDraw];
                    break;
                case "ChopTreeLeft":
                    SpriteSheetName = StringToSourceFileAndRecList["ChopTreeLeft"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["ChopTreeLeft"].Item2;
                    CurrentRect = CurrentRecList[CurrentDraw];
                    break;
                case "ChopTreeRight":
                    SpriteSheetName = StringToSourceFileAndRecList["ChopTreeRight"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["ChopTreeRight"].Item2;
                    CurrentRect = CurrentRecList[CurrentDraw];
                    break;
            }
            UpdateAnimation(timeMS);
        }

        public void Reset(Direction? direction = null)
        {
            if (direction.HasValue)
            {
                switch (direction)
                {
                    case Direction.Up:
                        CurrentAnimation = "UpMove";
                        SpriteSheetName = StringToSourceFileAndRecList["UpMove"].Item1;
                        CurrentRecList = StringToSourceFileAndRecList["UpMove"].Item2;
                        CurrentRect = CurrentRecList[0];
                        break;
                    case Direction.Down:
                        CurrentAnimation = "DownMove";
                        SpriteSheetName = StringToSourceFileAndRecList["DownMove"].Item1;
                        CurrentRecList = StringToSourceFileAndRecList["DownMove"].Item2;
                        CurrentRect = CurrentRecList[0];
                        break;
                    case Direction.Left:
                        CurrentAnimation = "LeftMove";
                        SpriteSheetName = StringToSourceFileAndRecList["LeftMove"].Item1;
                        CurrentRecList = StringToSourceFileAndRecList["LeftMove"].Item2;
                        CurrentRect = CurrentRecList[0];
                        break;
                    case Direction.Right:
                        CurrentAnimation = "RightMove";
                        SpriteSheetName = StringToSourceFileAndRecList["RightMove"].Item1;
                        CurrentRecList = StringToSourceFileAndRecList["RightMove"].Item2;
                        CurrentRect = CurrentRecList[0];
                        break;
                }
            }

            CurrentDraw = 0;
            NextAnimationTickMS = 250;
        }

        public void MoveComplete(SharedGameState shareState)
        {
            switch (Direction)
            {
                case Direction.Down:
                    CurrentDraw = 0;
                    CurrentAnimation = "DownMove";
                    SpriteSheetName = StringToSourceFileAndRecList["DownMove"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["DownMove"].Item2;
                    CurrentRect = CurrentRecList[0];
                    break;
                case Direction.Up:
                    CurrentDraw = 0;
                    CurrentAnimation = "UpMove";
                    SpriteSheetName = StringToSourceFileAndRecList["UpMove"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["UpMove"].Item2;
                    CurrentRect = CurrentRecList[0];
                    break;
                case Direction.Right:
                    CurrentDraw = 0;
                    CurrentAnimation = "RightMove";
                    SpriteSheetName = StringToSourceFileAndRecList["RightMove"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["RightMove"].Item2;
                    CurrentRect = CurrentRecList[0];
                    break;
                case Direction.Left:
                    CurrentDraw = 0;
                    CurrentAnimation = "LeftMove";
                    SpriteSheetName = StringToSourceFileAndRecList["LeftMove"].Item1;
                    CurrentRecList = StringToSourceFileAndRecList["LeftMove"].Item2;
                    CurrentRect = CurrentRecList[0];
                    break;
            }
        }

        public void Render(RenderContext context, int x, int y, int w, int h, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteSheetName);

            context.SpriteBatch.Draw(texture,
                sourceRectangle: CurrentRect,
                destinationRectangle: new Rectangle(
                    (int)(x), (int)(y),
                    (int)(w * context.Camera.Zoom), (int)(h * context.Camera.Zoom)
                    ),
                color: overlay);
        }
    }
}
