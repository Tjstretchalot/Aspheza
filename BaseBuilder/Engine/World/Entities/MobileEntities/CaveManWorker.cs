using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.Entities.Utilities.Animations;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class CaveManWorker : MobileEntity, Container
    {
        private SpriteSheetAnimationRenderer2 AnimationRenderer;
        private const double SpeedConst = 0.005;
        private static CollisionMeshD2D _CollisionMesh;

        //static private Dictionary<string, Animation> StringToAnimation;

        public EntityInventory Inventory { get; set; }

        static CaveManWorker()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });

            /* StringToAnimation = new Dictionary<string, Animation>()
        {
            { "DownMove", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0, 0, 32, 32), new Rectangle(32, 0, 32, 32), new Rectangle(64, 0, 32, 32), new Rectangle(96, 0, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 250, 250, 250, 250 }) },
            { "UpMove", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0, 32, 32, 32), new Rectangle(32, 32, 32, 32), new Rectangle(64, 32, 32, 32), new Rectangle(96, 32, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 250, 250, 250, 250 }) },
            { "RightMove", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0, 64, 32, 32), new Rectangle(32, 64, 32, 32), new Rectangle(64, 64, 32, 32), new Rectangle(96, 64, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 250, 250, 250, 250 }) },
            { "LeftMove", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0, 96, 32, 32), new Rectangle(32, 96, 32, 32), new Rectangle(64, 96, 32, 32), new Rectangle(96, 96, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 250, 250, 250, 250 }) },
            
            { "ChopWood", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0+16, 128+16, 33, 32), new Rectangle(64+16, 128+16, 32, 32), new Rectangle(128+16, 128+8, 32, 40), new Rectangle(192+16, 128+13, 35, 35), new Rectangle(256+16, 192+8, 30, 40), new Rectangle(320+16, 192+16, 32, 32), new Rectangle(384+11, 192+16, 37, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 17), new PointD2D(13, 12), new PointD2D(13, 17), new PointD2D(13, 17), new PointD2D(13, 9), new PointD2D(8, 9) },
                new List<int> { 72, 72, 72, 72, 72, 72, 72 }) },
                
            { "ChopTreeUp", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0+16, 192+16, 32, 32), new Rectangle(64+4, 192+16, 44, 32), new Rectangle(128+0, 192+16, 48, 32), new Rectangle(192+14, 192+16, 34, 32), new Rectangle(256+16, 192+16, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(25, 9), new PointD2D(29, 9), new PointD2D(15, 9), new PointD2D(13, 9), },
                new List<int> { 100, 100, 100, 100, 100 }) },
            { "ChopTreeDown", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0+16, 256+16, 32, 41), new Rectangle(64 + 16, 256+16, 45, 38), new Rectangle(128 + 16, 256+16, 48, 32), new Rectangle(192 + 16, 256+16, 45, 38), new Rectangle(256 + 16, 256+16, 32, 38) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 100, 100, 100, 100, 100 }) },
            { "ChopTreeRight", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0 + 16, 320 + 16, 32, 33), new Rectangle(64 + 16, 320 + 16, 32, 32), new Rectangle(128 + 10, 320 + 16, 38, 32), new Rectangle(192+16, 320 + 16, 32, 32), new Rectangle(256 + 16, 320 + 16, 33, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(19, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { 125, 125, 125, 125, 125 }) },
            { "ChopTreeLeft", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0 + 16, 384 + 16, 32, 33), new Rectangle(64 + 16, 384 + 16, 32, 32), new Rectangle(128 + 16, 384 + 16, 38, 32), new Rectangle(192 + 16, 384 + 16, 32, 32), new Rectangle(256 + 16, 384 + 15, 33, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 10), },
                new List<int> { 125, 125, 125, 125, 125 }) },
        };*/
        }

        public CaveManWorker(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
            Inventory = new EntityInventory(6);
            Inventory.SetStackSizeFor(Material.Sapling, 5);
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public CaveManWorker() : base()
        {
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);

            WriteTasks(message);
        }

        public override void OnMove(SharedGameState gameState, int timeMS, double dx, double dy)
        {
            if (AnimationRenderer == null)
                return;
            Direction Direction = Direction.Left;
            if (Math.Sign(dx) == -1)
            {//Left
                Direction = Direction.Left;
            }
            else if (Math.Sign(dx) == 1)
            {//Right
                Direction = Direction.Right;
            }
            else if (Math.Sign(dy) == 1)
            {//Down
                Direction = Direction.Down;
            }
            else if (Math.Sign(dy) == -1)
            {//Up
                Direction = Direction.Up;
            }
            AnimationRenderer.StartAnimation(AnimationType.Moving, Direction);
            AnimationRenderer.Update(timeMS);
        }

        public void OnChopping(int timeMS)
        {
            if (AnimationRenderer == null)
                return;
            AnimationRenderer.StartAnimation(AnimationType.Logging, Direction.Left);
            AnimationRenderer.Update(timeMS);
        }
        
        public override void OnStop(SharedGameState sharedState)
        {
            if (AnimationRenderer == null)
                return;
            AnimationRenderer.EndAnimation();
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if (AnimationRenderer == null)
                InitRenderer(context);
            AnimationRenderer.Render(context, overlay, screenTopLeft, 1);
        }

        public void InitRenderer(RenderContext context)
        {
            string sourceFile = "CaveManWorker/CaveManSpriteSheet";

            AnimationRenderer = new AnimationRendererBuilder(context.Content)

            // Idle
            .BeginAnimation(null, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame()
            .EndAnimation()
            // End Idle

            // Movements
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Moving, new PointD2D(0, 0), defaultSourceTexture: sourceFile, yLocation: 0)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Moving, new PointD2D(0, 0), defaultSourceTexture: sourceFile, yLocation: 32)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Moving, new PointD2D(0, 0), defaultSourceTexture: sourceFile, yLocation: 64)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Moving, new PointD2D(0, 0), defaultSourceTexture: sourceFile, yLocation: 96)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // End Movements

            // Chopping
            .BeginAnimation(null, AnimationType.Chopping, new PointD2D(0, 0), 72, sourceFile, 64, 128)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 8, 40, topLeftDif: new PointD2D(0, 8))
            .AddFrame(true, 16, 13, 35, 35, topLeftDif: new PointD2D(0, 3))
            .AddFrame(true, 16, 8, 40, topLeftDif: new PointD2D(0, 8))
            .AddFrame(true, 16, 16)
            .AddFrame(true, 11, 16, width: 37, topLeftDif: new PointD2D(5, 0))
            .EndAnimation()
            //EndChopping

            // Logging
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Logging, new PointD2D(0, 0), 100, sourceFile, 64, 192)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 4, 16, 44, topLeftDif: new PointD2D(12, 0))
            .AddFrame(true, 0, 16, 48, topLeftDif: new PointD2D(16, 0))
            .AddFrame(true, 14, 16, 34, topLeftDif: new PointD2D(2, 0))
            .AddFrame(true, 16, 16)
            .EndAnimation()
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Logging, new PointD2D(0, 0), 100, sourceFile, 64, 256)
            .AddFrame(true, 16, 16, width: 41)
            .AddFrame(true, 16, 16, 45, 38)
            .AddFrame(true, 16, 16, 48)
            .AddFrame(true, 16, 16, 45, 38)
            .AddFrame(true, 16, 16, width: 38)
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Logging, new PointD2D(0, 0), 125, sourceFile, 64, 320, 1)
            .AddFrame(true, 16, 16, 33)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 10, 16, 38, topLeftDif: new PointD2D(6, 0))
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 33)
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Logging, new PointD2D(0, 0), 100, sourceFile, 64, 384, 1)
            .AddFrame(true, 16, 16, 33)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, 38)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 33, topLeftDif: new PointD2D(1, 0))
            .EndAnimation()
            // End Logging

            .Build();
        }
    }
}
