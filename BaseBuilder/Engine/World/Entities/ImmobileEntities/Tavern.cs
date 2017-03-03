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
    public class Tavern : ImmobileEntity
    {
        protected static PolygonD2D _CollisionMesh;

        protected SpriteRenderer Renderer;
        static Rectangle SourceRec = new Rectangle(0, 0, 218, 212);
                
        static Tavern()
        {
            _CollisionMesh = new PolygonD2D(new List<PointD2D> { new PointD2D(0, 5), new PointD2D(10, 0), new PointD2D(13.625, 0), new PointD2D(13.625, 13), new PointD2D(9.5625, 13), new PointD2D(0, 10.5) });
        }

        public Tavern(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Tavern", SourceRec);
            _HoverText = "Tavern";
            
        }

        public Tavern() : base()
        {
            CollisionMesh = _CollisionMesh;
            Renderer = new SpriteRenderer("Tavern", SourceRec);
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
            Renderer.Render(context, (int)screenTopLeft.X, (int)screenTopLeft.Y, 218 / 16.0, 212 / 16.0, overlay);
        }
    }
}
