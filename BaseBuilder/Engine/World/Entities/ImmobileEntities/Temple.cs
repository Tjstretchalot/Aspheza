using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.State.Resources;
using System.IO;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Temple : ImmobileEntity
    {
        protected static CollisionMeshD2D _CollisionMesh;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 100, 132);

        static Temple()
        {
            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(6, 8) });
        }

        public Temple(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Temple", SourceRec);
            _HoverText = "Temple";

        }

        public Temple() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Temple", SourceRec);
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            _HoverText = message.ReadString();

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write(_HoverText);

            WriteTasks(message);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 100 / 16.0, 132 / 16.0, overlay);
        }
    }
}
