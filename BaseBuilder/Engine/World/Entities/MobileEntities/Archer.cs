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
    public class Archer : MobileEntity
    {
        private SpriteRenderer SpriteRenderer;
        private const double SpeedConst = 0.005;
        private static RectangleD2D _CollisionMesh;

        static Archer()
        {
            _CollisionMesh = new RectangleD2D(1, 1);
        }

        public Archer(PointD2D position, int id) : base(position, _CollisionMesh, id, SpeedConst)
        {
            SpriteRenderer = new SpriteRenderer("Archer");
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public Archer() : base()
        {
            SpriteRenderer = new SpriteRenderer("Archer");
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
            SpriteRenderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, overlay);
        }
    }
}
