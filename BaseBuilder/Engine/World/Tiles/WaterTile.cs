using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.World.Tiles
{
    public class WaterTile : Tile
    {
        protected const short ID = 4;
        protected const string SheetName = "tile_water_anim";

        protected static Dictionary<Direction, List<Rectangle>> DirectionsToCenterFlows;
        protected static Dictionary<Direction, List<Rectangle>> DirectionToVerticalFlows;

        static WaterTile()
        {
            TileIdentifier.Register(typeof(WaterTile), ID);

            var rightFlow = new List<Rectangle>();
            rightFlow.Add(new Rectangle(32, 32, 32, 32));
            rightFlow.Add(new Rectangle(129, 32, 32, 32));
            rightFlow.Add(new Rectangle(226, 32, 32, 32));
            
            var leftFlow = new List<Rectangle>();
            leftFlow.Add(new Rectangle(32, 129, 32, 32));
            leftFlow.Add(new Rectangle(129, 129, 32, 32));
            leftFlow.Add(new Rectangle(226, 129, 32, 32));

            var upFlow = new List<Rectangle>();
            upFlow.Add(new Rectangle(323, 32, 32, 32));
            upFlow.Add(new Rectangle(323, 129, 32, 32));
            upFlow.Add(new Rectangle(323, 226, 32, 32));

            var downFlow = new List<Rectangle>();
            downFlow.Add(new Rectangle(420, 32, 32, 32));
            downFlow.Add(new Rectangle(420, 129, 32, 32));
            downFlow.Add(new Rectangle(420, 226, 32, 32));

            DirectionsToCenterFlows = new Dictionary<Direction, List<Rectangle>>();
            DirectionsToCenterFlows.Add(Direction.Right, rightFlow);
            DirectionsToCenterFlows.Add(Direction.Left, leftFlow);
            DirectionsToCenterFlows.Add(Direction.Up, upFlow);
            DirectionsToCenterFlows.Add(Direction.Down, downFlow);
        }

        public override bool Ground { get { return false; } }
        public override bool Water { get { return true; } }
        public override bool RequiresUpdate { get { return true; } }

        protected Rectangle DestRect;
        protected Rectangle SrcRect;
        protected List<Rectangle> SourceRectsFlowingOneAnimationCycle;
        protected Direction FlowDirection;
        protected int CurrentAnimPosition;
        protected int TimeToNextAnimMS;

        protected bool GrassLeftEdge;
        protected bool GrassRightEdge;
        protected bool GrassTopEdge;
        protected bool GrassBottomEdge;

        public WaterTile(PointI2D position, RectangleD2D collisionMesh, Direction flow) : base(position, collisionMesh)
        {
            FlowDirection = flow;
            CurrentAnimPosition = 0;

            DestRect = new Rectangle(0, 0, (int)collisionMesh.Width, (int)collisionMesh.Height);
            SrcRect = new Rectangle(0, 0, 32, 32);
        }

        public WaterTile(PointI2D position, RectangleD2D collisionMesh, NetIncomingMessage message) : base(position, collisionMesh)
        {
            FlowDirection = (Direction)message.ReadInt32();

            DestRect = new Rectangle(0, 0, (int)collisionMesh.Width, (int)collisionMesh.Height);
            SrcRect = new Rectangle(0, 0, 32, 32);
        }

        public override void Write(NetOutgoingMessage message)
        {
            message.Write((int)FlowDirection);
        }

        public override void Loaded(SharedGameState gameState)
        {
            base.Loaded(gameState);

            SourceRectsFlowingOneAnimationCycle = DirectionsToCenterFlows[FlowDirection];

            var left = GetTileFromRelative(gameState.World, -1, 0);
            var right = GetTileFromRelative(gameState.World, 1, 0);
            var top = GetTileFromRelative(gameState.World, 0, -1);
            var bottom = GetTileFromRelative(gameState.World, 0, 1);

            GrassLeftEdge = (left != null) && !typeof(WaterTile).IsAssignableFrom(left.GetType());
            GrassRightEdge = (right != null) && !typeof(WaterTile).IsAssignableFrom(right.GetType());
            GrassTopEdge = (top != null) && !typeof(WaterTile).IsAssignableFrom(top.GetType());
            GrassBottomEdge = (bottom != null) && !typeof(WaterTile).IsAssignableFrom(bottom.GetType());
        }
        
        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {

            var srcRect = SourceRectsFlowingOneAnimationCycle[CurrentAnimPosition];
            var text = context.Content.Load<Texture2D>(SheetName);

            DestRect.X = (int)screenTopLeft.X;
            DestRect.Y = (int)screenTopLeft.Y;
            DestRect.Width = (int)context.Camera.Zoom;
            DestRect.Height = (int)context.Camera.Zoom;

            context.SpriteBatch.Draw(text, DestRect, srcRect, overlay);

            if(GrassLeftEdge)
            {
                SrcRect.X = srcRect.X;
                SrcRect.Y = srcRect.Y - 32;
                SrcRect.Width = 2;
                SrcRect.Height = 32;

                DestRect.X = (int)screenTopLeft.X;
                DestRect.Y = (int)screenTopLeft.Y;
                DestRect.Width = 2;
                DestRect.Height = (int)context.Camera.Zoom;

                context.SpriteBatch.Draw(text, DestRect, SrcRect, overlay);
            }
            if (GrassRightEdge)
            {
                SrcRect.X = srcRect.X + 30;
                SrcRect.Y = srcRect.Y - 32;
                SrcRect.Width = 2;
                SrcRect.Height = 32;

                DestRect.X = (int)(screenTopLeft.X + (30 / 32.0) * context.Camera.Zoom);
                DestRect.Y = (int)screenTopLeft.Y;
                DestRect.Width = 2;
                DestRect.Height = (int)context.Camera.Zoom;

                context.SpriteBatch.Draw(text, DestRect, SrcRect, overlay);
            }
            if (GrassTopEdge)
            {
                SrcRect.X = srcRect.X - 32;
                SrcRect.Y = srcRect.Y;
                SrcRect.Width = 32;
                SrcRect.Height = 2;

                DestRect.X = (int)screenTopLeft.X;
                DestRect.Y = (int)screenTopLeft.Y;
                DestRect.Width = (int)context.Camera.Zoom;
                DestRect.Height = 2;

                context.SpriteBatch.Draw(text, DestRect, SrcRect, overlay);
            }
            if (GrassBottomEdge)
            {
                SrcRect.X = srcRect.X - 32;
                SrcRect.Y = srcRect.Y + 30;
                SrcRect.Width = 32;
                SrcRect.Height = 2;

                DestRect.X = (int)screenTopLeft.X;
                DestRect.Y = (int)Math.Round(screenTopLeft.Y + (30 / 32.0) * context.Camera.Zoom);
                DestRect.Width = (int)context.Camera.Zoom;
                DestRect.Height = 2;

                context.SpriteBatch.Draw(text, DestRect, SrcRect, overlay);
            }
        }

        public override void Update(UpdateContext context)
        {
            TimeToNextAnimMS -= context.ElapsedMS;
            if (TimeToNextAnimMS <= 0)
            {
                CurrentAnimPosition = (CurrentAnimPosition + 1) % SourceRectsFlowingOneAnimationCycle.Count;
                TimeToNextAnimMS = 200;
            }
        }
    }
}
