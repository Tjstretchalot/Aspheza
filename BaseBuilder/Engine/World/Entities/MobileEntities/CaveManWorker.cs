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

namespace BaseBuilder.Engine.World.Entities.MobileEntities
{
    public class CaveManWorker : MobileEntity
    {
        private SpriteSheetAnimationRenderer AnimationRenderer;
        private const double SpeedConst = 0.005;
        private static RectangleD2D _CollisionMesh;

        public EntityInventory Inventory { get; protected set; }

        static CaveManWorker()
        {
            _CollisionMesh = new RectangleD2D(1, 1);
        }

        public CaveManWorker(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
            Inventory = new EntityInventory(6);
            AnimationRenderer = new SpriteSheetAnimationRenderer();
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public CaveManWorker() : base()
        {
            AnimationRenderer = new SpriteSheetAnimationRenderer();
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

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            AnimationRenderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, overlay);
        }
    }
}
