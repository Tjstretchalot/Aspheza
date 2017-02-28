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
        string SpriteSheetName;
        Rectangle CurrentRect;

        int NextAnimationTickMS;

        int CurrentDraw;
        Direction Direction;
        List<Rectangle> _DownMove;
        List<Rectangle> _UpMove;
        List<Rectangle> _RightMove;
        List<Rectangle> _LeftMove;

        public SpriteSheetAnimationRenderer(string sheetName, List<Rectangle> downMove, List<Rectangle> upMove, List<Rectangle> rightMove, List<Rectangle> leftMove)
        {
            SpriteSheetName = sheetName;

            _DownMove = downMove;
            _UpMove = upMove;
            _RightMove = rightMove;
            _LeftMove = leftMove;

            CurrentRect = _DownMove[0];
            CurrentDraw = 0;
        }

        public void FromMessage(NetIncomingMessage message)
        {
            int crX = message.ReadInt32();
            int crY = message.ReadInt32();
            int crW = message.ReadInt32();
            int crH = message.ReadInt32();

            CurrentRect = new Rectangle(crX, crY, crW, crH);

            NextAnimationTickMS = message.ReadInt32();
            CurrentDraw = message.ReadInt32();
            Direction = (Direction)message.ReadInt32();
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(CurrentRect.X);
            message.Write(CurrentRect.Y);
            message.Write(CurrentRect.Width);
            message.Write(CurrentRect.Height);

            message.Write(NextAnimationTickMS);
            message.Write(CurrentDraw);
            message.Write((int)Direction);
        }

        private void UpdateDirection(double dx, double dy)
        {
            
            if(Math.Sign(dx) == -1 && Direction != Direction.Left)
            {
                CurrentDraw = (CurrentDraw + 1) % _LeftMove.Count;
                Direction = Direction.Left;
                CurrentRect = _LeftMove[CurrentDraw];
            }
            else if (Math.Sign(dx) == 1 && Direction != Direction.Right)
            {
                CurrentDraw = (CurrentDraw + 1) % _RightMove.Count;
                Direction = Direction.Right;
                CurrentRect = _RightMove[CurrentDraw];
            }
            else if (Math.Sign(dy) == 1 && Direction != Direction.Down)
            {
                CurrentDraw = (CurrentDraw + 1) % _DownMove.Count;
                Direction = Direction.Down;
                CurrentRect = _DownMove[CurrentDraw];
            }
            else if (Math.Sign(dy) == -1 && Direction != Direction.Up)
            {
                CurrentDraw = (CurrentDraw + 1) % _UpMove.Count;
                Direction = Direction.Up;
                CurrentRect = _UpMove[CurrentDraw];
            }
        }

        private void UpdateAnimation(int timeMS)
        {
            NextAnimationTickMS -= timeMS;
            if (NextAnimationTickMS <= 0)
            {
                NextAnimationTickMS = 250;
                if (Direction == Direction.Left)
                {
                    CurrentDraw = (CurrentDraw + 1) % _LeftMove.Count;
                    CurrentRect = _LeftMove[CurrentDraw];
                }
                else if (Direction == Direction.Right)
                {
                    CurrentDraw = (CurrentDraw + 1) % _RightMove.Count;
                    CurrentRect = _RightMove[CurrentDraw];
                }
                else if (Direction == Direction.Down)
                {
                    CurrentDraw = (CurrentDraw + 1) % _DownMove.Count;
                    CurrentRect = _DownMove[CurrentDraw];
                }
                else if (Direction == Direction.Up)
                {
                    CurrentDraw = (CurrentDraw + 1) % _UpMove.Count;
                    CurrentRect = _UpMove[CurrentDraw];
                }
            }
        }

        public void UpdateSprite(SharedGameState shareState, int timeMS, double dx, double dy)
        {
            UpdateDirection(dx, dy);
            UpdateAnimation(timeMS);
        }

        public void MoveComplete(SharedGameState shareState)
        {
            if (Direction == Direction.Down)
            {
                CurrentDraw = 0;
                CurrentRect = _DownMove[0];
            }
            else if (Direction == Direction.Up)
            {
                CurrentDraw = 0;
                CurrentRect = _UpMove[0];
            }
            else if (Direction == Direction.Right)
            {
                CurrentDraw = 0;
                CurrentRect = _RightMove[0];
            }
            else if (Direction == Direction.Left)
            {
                CurrentDraw = 0;
                CurrentRect = _LeftMove[0];
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
