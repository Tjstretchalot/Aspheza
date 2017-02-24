using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Lidgren.Network;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class MageTower : ImmobileEntity
    {
        protected static List<Tuple<Rectangle, PointD2D>> _SourceRectsToOffsetLocations;
        protected static string _SheetName;
        
        private static PolygonD2D _CollisionMesh;

        static MageTower()
        {

            _CollisionMesh = new RectangleD2D(3, 4);

            _SourceRectsToOffsetLocations = new List<Tuple<Rectangle, PointD2D>>();

            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(0, 0, 48, 64), new PointD2D(4, 0)));

        }

        public MageTower(PointD2D position, int id) : base(position, _CollisionMesh, id)
        {
        }
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public MageTower() : base()
        {
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            CollisionMesh = _CollisionMesh;

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
            throw new NotImplementedException();
        }
    }
}
