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

namespace BaseBuilder.Engine.World.Entities.ImmobileEntities
{
    public class House : ImmobileEntity
    {
        private static short NetId = 1001;
        private static PolygonD2D _CollisionMesh;

        static House()
        {
            _CollisionMesh = new RectangleD2D(1, 1);
            EntityIdentifier.Register(typeof(House), NetId);
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

            var sourceRect = new Rectangle(
                0, 0, 16, 16
                );

            var destRect = new Rectangle(
                (int)screenTopLeft.X, (int)screenTopLeft.Y, (int)(1 * context.Camera.Zoom), (int)(1 * context.Camera.Zoom)
                );

            context.SpriteBatch.Draw(sheet, sourceRectangle: sourceRect, destinationRectangle: destRect);
        }
    }
}
