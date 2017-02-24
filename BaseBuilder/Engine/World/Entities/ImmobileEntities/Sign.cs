using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using BaseBuilder.Engine.State;
using Lidgren.Network;

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class Sign : SpriteSheetBuilding
    {
        protected static List<Tuple<Rectangle, PointD2D>> _SourceRectsToOffsetLocations;
        protected static string _SheetName;
        
        private static PolygonD2D _CollisionMesh;

        static Sign()
        {

            _CollisionMesh = new RectangleD2D(1, 1);

            _SourceRectsToOffsetLocations = new List<Tuple<Rectangle, PointD2D>>();

            _SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(323, 0, 16, 16), new PointD2D(0, 0)));

        }


        protected string Text;

        public Sign(PointD2D position, int id, string text) : base(position, _CollisionMesh, id, "roguelikeSheet_transparent", _SourceRectsToOffsetLocations)
        {
            SetText(text);
        }
        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        public Sign() : base()
        {
            CollisionMesh = _CollisionMesh;
            SheetName = "roguelikeSheet_transparent";
            SourceRectsToOffsetLocations = _SourceRectsToOffsetLocations;
        }

        public override void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            ID = message.ReadInt32();
            SetText(message.ReadString());

            TasksFromMessage(gameState, message);
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
            message.Write(Text);

            WriteTasks(message);
        }

        public void SetText(string text)
        {
            Text = text;
            _HoverText = text;
        }
    }
}
