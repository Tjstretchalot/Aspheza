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
        private SpriteSheetAnimationRenderer AnimationRenderer;
        private const double SpeedConst = 0.005;
        private static CollisionMeshD2D _CollisionMesh;

        static private Dictionary<string, Animation> StringToAnimation = new Dictionary<string, Animation>()
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
            /*
            { "ChopWood", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> {  },
                new List<PointD2D> {  },
                new List<int> { 250, 250, 250, 250, 250 }) },
                */
            { "ChopTreeUp", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0+16, 192+16, 32, 32), new Rectangle(64+4, 192+16, 44, 32), new Rectangle(128+0, 192+16, 48, 32), new Rectangle(192+14, 192+16, 34, 32), new Rectangle(256+16, 192+16, 32, 32) },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(25, 9), new PointD2D(29, 9), new PointD2D(15, 9), new PointD2D(13, 9), },
                new List<int> { 125, 125, 125, 125, 125 }) },
            { "ChopTreeDown", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { new Rectangle(0+16, 256+16, 32, 41), new Rectangle(64 + 16, 256+16, 45, 38), new Rectangle(128 + 16, 256+16, 48, 32), new Rectangle(192 + 16, 256+16, 45, 38), /*new Rectangle(256 + 16, 256+16, 32, 38)*/ },
                new List<PointD2D> { new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9),/* new PointD2D(13, 9), */},
                new List<int> { 125, 125, 125, 125, /*125*/ }) },
            { "ChopTreeRight", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> { /*new Rectangle(0 + 16, 320 + 16, 32, 33),*/ new Rectangle(64 + 16, 320 + 16, 32, 32), new Rectangle(128 + 10, 320 + 16, 38, 32), new Rectangle(192+16, 320 + 16, 32, 32), new Rectangle(256 + 16, 320 + 16, 33, 32) },
                new List<PointD2D> { /*new PointD2D(13, 9),*/ new PointD2D(13, 9), new PointD2D(19, 9), new PointD2D(13, 9), new PointD2D(13, 9), },
                new List<int> { /*250, */125, 125, 125, 125 }) },
            { "ChopTreeLeft", new Animation("CaveManWorker/CaveManSpriteSheet",
                new List<Rectangle> {/* new Rectangle(0 + 16, 384 + 16, 32, 33),*/ new Rectangle(64 + 16, 384 + 16, 32, 32), new Rectangle(128 + 16, 384 + 16, 38, 32), new Rectangle(192 + 16, 384 + 16, 32, 32), new Rectangle(256 + 16, 384 + 15, 33, 32) },
                new List<PointD2D> {/* new PointD2D(13, 9), */new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 9), new PointD2D(13, 10), },
                new List<int> {/* 250,*/ 125, 125, 125, 125 }) },
        };

        public EntityInventory Inventory { get; set; }

        static CaveManWorker()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });
        }

        public CaveManWorker(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
            Inventory = new EntityInventory(6);
            Inventory.SetStackSizeFor(Material.Sapling, 5);
            AnimationRenderer = new SpriteSheetAnimationRenderer(new PointD2D(13.0, 9.0), StringToAnimation);
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public CaveManWorker() : base()
        {
            AnimationRenderer = new SpriteSheetAnimationRenderer(new PointD2D(13.0, 9.0), StringToAnimation);
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            Inventory = new EntityInventory(message);
            AnimationRenderer.FromMessage(message);

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);
            AnimationRenderer.Write(message);

            WriteTasks(message);
        }

        public override void OnMove(SharedGameState sharedState, int timeMS, double dx, double dy)
        {
            AnimationRenderer.UpdateSprite(sharedState, timeMS, dx, dy);
        }

        public void OnChopping(SharedGameState sharedState, int timeMS, string Animation)
        {
            AnimationRenderer.UpdateSprite(sharedState, timeMS, 0, 0, Animation);
        }

        public void Reset(Direction? direction = null)
        {
            AnimationRenderer.Reset(direction);
        }
        public override void OnStop(SharedGameState sharedState)
        {
            AnimationRenderer.MoveComplete(sharedState);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            AnimationRenderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(CollisionMesh.Right - CollisionMesh.Left), (int)(CollisionMesh.Bottom - CollisionMesh.Top), overlay);
        }
    }
}
