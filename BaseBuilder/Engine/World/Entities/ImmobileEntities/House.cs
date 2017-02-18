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
    public class House : ImmobileEntity
    {
        protected static List<Tuple<Rectangle, PointD2D>> SourceRectsToOffsetLocations;

        private static short NetId = 1001;
        private static PolygonD2D _CollisionMesh;

        static House()
        {
            _CollisionMesh = new RectangleD2D(2, 3);
            EntityIdentifier.Register(typeof(House), NetId);

            SourceRectsToOffsetLocations = new List<Tuple<Rectangle, PointD2D>>();

            // wall
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(289, 374, 16, 16), new PointD2D(0, 2)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(289, 391, 16, 16), new PointD2D(0, 3)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(323, 374, 16, 16), new PointD2D(1, 2)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(323, 391, 16, 16), new PointD2D(1, 3)));

            //roof
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 357, 16, 16), new PointD2D(0, 0)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 374, 16, 16), new PointD2D(0, 1)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(340, 391, 16, 16), new PointD2D(0, 2)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 357, 16, 16), new PointD2D(1, 0)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 374, 16, 16), new PointD2D(1, 1)));
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(357, 391, 16, 16), new PointD2D(1, 2)));

            // door
            SourceRectsToOffsetLocations.Add(Tuple.Create(new Rectangle(544, 17, 16, 16), new PointD2D(0.5, 3)));


        }

        public House(PointD2D position, int id) : base(position, _CollisionMesh, id)
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
        }

        public override void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            message.Write(ID);
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft)
        {
            var sheet = context.Content.Load<Texture2D>("roguelikeSheet_transparent.png");

            foreach (var tuple in SourceRectsToOffsetLocations)
            {
                var sourceRect = tuple.Item1;
                var offset = tuple.Item2;
                var destRect = new Rectangle(
                        (int)(offset.X * context.Camera.Zoom + screenTopLeft.X), (int)(offset.Y * context.Camera.Zoom + screenTopLeft.Y), (int)context.Camera.Zoom, (int)context.Camera.Zoom
                    );
                context.SpriteBatch.Draw(sheet, sourceRectangle: sourceRect, destinationRectangle: destRect);
            }

            //var destRect = new Rectangle(
            //    (int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(1 * context.Camera.Zoom), (int)(1 * context.Camera.Zoom)
            //    );

            
        }
    }
}
