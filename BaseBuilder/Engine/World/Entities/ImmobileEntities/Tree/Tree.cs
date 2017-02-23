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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities.Tree
{
    public class Tree : SpriteSheetBuilding
    {
        protected TreeSize Size;
        protected TreeStyle Style;
        protected TreeColor Color;

        public Tree() : base()
        {
        }

        public Tree(PointD2D position, PolygonD2D collisionMesh, int id, string sheetName, List<Tuple<Rectangle, PointD2D>> sourceRectsToOffsetLocations, TreeSize size, TreeStyle style, TreeColor color) : base(position, collisionMesh, id, sheetName, sourceRectsToOffsetLocations)
        {
            Size = size;
            Style = style;
            Color = color;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            ID = message.ReadInt32();
            Position = new PointD2D(message);
            Size = (TreeSize)message.ReadInt32();
            Style = (TreeStyle)message.ReadInt32();
            Color = (TreeColor)message.ReadInt32();

            TasksFromMessage(gameState, message);


            var dataFromInfo = Tuple.Create(new PolygonD2D(message), new List<Tuple<Rectangle, PointD2D>>()); // TODO this is just to see the type
            CollisionMesh = dataFromInfo.Item1;
            SourceRectsToOffsetLocations = dataFromInfo.Item2;
        }

        public override void Write(NetOutgoingMessage message)
        {
            message.Write(ID);
            Position.Write(message);
            message.Write((int)Size);
            message.Write((int)Style);
            message.Write((int)Color);

            WriteTasks(message);
        }
    }
}
