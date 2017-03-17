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
using BaseBuilder.Engine.World.Entities.Utilities.Animations;

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class OverseerMage : MobileEntity
    {
        private const double SpeedConst = 0.005;
        private static CollisionMeshD2D _CollisionMesh;

        static OverseerMage()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 1) });
        }

        public OverseerMage(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public OverseerMage() : base()
        {
            CollisionMesh = _CollisionMesh;
            SpeedUnitsPerMS = SpeedConst;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if (AnimationRenderer == null)
                InitRenderer(context);
            AnimationRenderer.Render(context, overlay, screenTopLeft, 1);
        }

        public override void Update(UpdateContext context)
        {
            base.Update(context);

            AnimationRenderer?.Update(context.ElapsedMS);
        }

        public void InitRenderer(RenderContext context)
        {
            string sourceFile = "WizardOverseer/WizardSpriteSheet";

            AnimationRenderer = new AnimationRendererBuilder(context.Content)

            // Idle
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile)
            .AddFrame()
            .EndAnimation()
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile, yLocation: 32)
            .AddFrame()
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile, yLocation: 64)
            .AddFrame()
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Idle, new PointD2D(0, 0), 50000, sourceFile, yLocation: 96)
            .AddFrame()
            .EndAnimation()
            // End Idle

            // Movement
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile)
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .AddFrame()
            .EndAnimation()
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Moving, new PointD2D(0, 0), 250, sourceFile, yLocation: 32)
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
            // End Movement

            // Casting
            // Down
            .BeginAnimation(Direction.Down, AnimationType.Casting, new PointD2D(0, 0), 100, sourceFile, 64, 128)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 13, 16, width: 35, topLeftDif: new PointD2D(3, 0))
            .AddFrame(true, 7, 15, 33, 41, topLeftDif: new PointD2D(9, 1))
            .AddFrame(true, 4, 15, 33, 44, topLeftDif: new PointD2D(12, 1))
            .AddFrame(true, 9, 11, 37, 39, topLeftDif: new PointD2D(7, 5))
            .AddFrame(true, 5, 16, width: 43, topLeftDif: new PointD2D(11, 0), displayTime: 250)
            .EndAnimation()
            // Up
            .BeginAnimation(Direction.Up, AnimationType.Casting, new PointD2D(0, 0), 100, sourceFile, 64, 192)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 37)
            .AddFrame(true, 16, 14, 34, 42, topLeftDif: new PointD2D(0, 2))
            .AddFrame(true, 16, 14, 34, 44, topLeftDif: new PointD2D(0, 2))
            .AddFrame(true, 16, 9, 39, 41, topLeftDif: new PointD2D(0, 7))
            .AddFrame(true, 16, 16, width: 43, displayTime: 250)
            .EndAnimation()
            // Right
            .BeginAnimation(Direction.Right, AnimationType.Casting, new PointD2D(0, 0), 100, sourceFile, 64, 256)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 15, 16, width: 33, topLeftDif: new PointD2D(1, 0))
            .AddFrame(true, 11, 16, width: 37, topLeftDif: new PointD2D(5, 0))
            .AddFrame(true, 16, 13, 35, topLeftDif: new PointD2D(0, 3))
            .AddFrame(true, 13, 16, width: 35, topLeftDif: new PointD2D(3, 0), displayTime: 250)
            .EndAnimation()
            // Left
            .BeginAnimation(Direction.Left, AnimationType.Casting, new PointD2D(0, 0), 100, sourceFile, 64, 320)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16)
            .AddFrame(true, 16, 16, width: 33)
            .AddFrame(true, 16, 16, width: 37)
            .AddFrame(true, 16, 13, 35, topLeftDif: new PointD2D(0, 3))
            .AddFrame(true, 16, 16, width: 35, displayTime: 250)
            .EndAnimation()
            // End Casting

            .Build();
        }
    }
}
