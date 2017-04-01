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
using BaseBuilder.Engine.World.Entities.Utilities;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree
{
    public class TreePointy : SpriteSheetBuilding
    {
        protected static List<Tuple<Rectangle, PointD2D>> _SourceRectsToOffsetLocations;
        protected static string _SheetName;

        private static CollisionMeshD2D _CollisionMesh;

        static TreePointy()
        {
            _SheetName = "roguelikeSheet_transparent";

            _CollisionMesh = new CollisionMeshD2D(new List<PolygonD2D> { new RectangleD2D(1, 2) });

            _SourceRectsToOffsetLocations = new List<Tuple<Rectangle, PointD2D>>();

            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(272, 170, 16, 16), new PointD2D(0, 0)));
            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(272, 187, 16, 16), new PointD2D(0, 1)));

        }

        public TreePointy(PointD2D position, int id) : base(position, _CollisionMesh, id, _SheetName, _SourceRectsToOffsetLocations)
        {
        }
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public TreePointy() : base()
        {
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            CollisionMesh = _CollisionMesh;
            SheetName = _SheetName;
            SourceRectsToOffsetLocations = _SourceRectsToOffsetLocations;

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);

            WriteTasks(message);
        }

        public override SpriteSheetAnimationRenderer GetInprogressRenderable(ContentManager content)
        {
            throw new NotImplementedException();
        }
    }
}
