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
        private const double SpeedConst = 0.005;
        private static CollisionMeshD2D _CollisionMesh;

        public EntityInventory Inventory { get; set; }

        static CaveManWorker()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });
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

            SyncAnimationFromMessage(message);
            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            Inventory.Write(message);

            WriteAnimationSync(message);
            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            base.Render(context, screenTopLeft, overlay);

            AnimationRenderer.Render(context, overlay, screenTopLeft, 1);
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);

            AnimationRenderer?.Update(context.ElapsedMS);
        }

        protected override void InitRenderer(RenderContext context)
        {
            string sourceFile = "CaveManWorker/CaveManSpriteSheet";

            AnimationRenderer = new AnimationRendererBuilder(context.Content)

            // Idle
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame()
            .EndAnimation()
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame(y: 32)
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame(y: 64)
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame(y: 96)
            .EndAnimation()
            // End Idle

            // Movements
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile, yLocation: 0)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile, yLocation: 32)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile, yLocation: 64)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile, yLocation: 96)
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
            .AddFrame(true, 4, 16, width: 44, topLeftDif: new PointD2D(12, 0))
            .AddFrame(true, 0, 16, width: 48, topLeftDif: new PointD2D(16, 0))
            .AddFrame(true, 14, 16, width: 34, topLeftDif: new PointD2D(2, 0))
            .AddFrame(true, 16, 16)
            .EndAnimation()
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Logging, new PointD2D(0, 0), 100, sourceFile, 64, 256)
            .AddFrame(true, 16, 16, width: 41)
            .AddFrame(true, 16, 16, 45, 38)
            .AddFrame(true, 16, 16, width: 48)
            .AddFrame(true, 16, 16, 45, 38)
            .AddFrame(true, 16, 16, width: 38)
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Logging, new PointD2D(0, 0), 125, sourceFile, 64, 320, 1)
            .AddFrame(true, 16, 16, width: 33)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 10, 16, width: 38, topLeftDif: new PointD2D(6, 0))
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 33)
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Logging, new PointD2D(0, 0), 125, sourceFile, 64, 384, 1)
            .AddFrame(true, 16, 16, 33)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 38)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 33, topLeftDif: new PointD2D(1, 0))
            .EndAnimation()
            // End Logging

            .Build();
        }
    }
}
