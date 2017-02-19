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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class House : SpriteSheetBuilding
    {
        protected static List<Tuple<Rectangle, PointD2D>> _SourceRectsToOffsetLocations;
        protected static string _SheetName;

        private static short NetId = 1001;
        private static PolygonD2D _CollisionMesh;

        static House()
        {
            _SheetName = "roguelikeSheet_transparent";

            _CollisionMesh = new RectangleD2D(2, 3);
            EntityIdentifier.Register(typeof(House), NetId);

            _SourceRectsToOffsetLocations = new List<Tuple<Rectangle, PointD2D>>();
            // Shed
            // wall
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(289, 374, 16, 16), new PointD2D(0, 2)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(289, 391, 16, 16), new PointD2D(0, 3)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(323, 374, 16, 16), new PointD2D(1, 2)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(323, 391, 16, 16), new PointD2D(1, 3)));

            // roof
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 357, 16, 16), new PointD2D(0, 0)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 374, 16, 16), new PointD2D(0, 1)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 391, 16, 16), new PointD2D(0, 2)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 357, 16, 16), new PointD2D(1, 0)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 374, 16, 16), new PointD2D(1, 1)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 391, 16, 16), new PointD2D(1, 2)));

            // door
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(544, 17, 16, 16), new PointD2D(0.5, 3)));

        }

        public House(PointD2D position, int id) : base(position, _CollisionMesh, id, _SheetName, _SourceRectsToOffsetLocations)
        {
        }
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public House() : base()
        {
        }

        public override void FromMessage(NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            CollisionMesh = _CollisionMesh;
            SheetName = _SheetName;
            SourceRectsToOffsetLocations = _SourceRectsToOffsetLocations;
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
        }
    }
}
