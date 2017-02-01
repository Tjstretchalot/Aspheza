using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BaseBuilder.Engine.World.WorldObject.Entities
{
    /// <summary>
    /// Describes an entity that is rendered using a sprite.
    /// </summary>
    public class SpriteEntity : Entity
    {
        protected string SpriteName;

        public SpriteEntity(PointD2D position, PolygonD2D collisionMesh, string spriteName) : base(position, collisionMesh)
        {
            SpriteName = spriteName;
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            context.SpriteBatch.Draw(texture,
                new Rectangle(
                    (int)(screenTopLeft.X + CollisionMesh.Left), (int)(screenTopLeft.Y + CollisionMesh.Top),
                    (int)(CollisionMesh.Right - CollisionMesh.Left), (int)(CollisionMesh.Bottom - CollisionMesh.Top)
                    ), Color.White);
        }
    }
}
