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
    class SpriteEntity : Entity
    {
        protected string SpriteName;

        public SpriteEntity(string spriteName)
        {
            SpriteName = spriteName;
        }

        public override void Render(RenderContext context, PointD2D screenTopLeft)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            var TmpI = new VectorD2D(0, 0);
            var TmpJ = new VectorD2D(0, 1);

            var TmpLeft = CollisionMesh.ProjectOntoAxis(TmpI).Start;
            var TmpTop = CollisionMesh.ProjectOntoAxis(TmpJ).End;
            var TmpWidth = CollisionMesh.ProjectOntoAxis(TmpI).Length;
            var TmpHeight = CollisionMesh.ProjectOntoAxis(TmpJ).Length;


            context.SpriteBatch.Draw(texture,
                new Rectangle(
                    (int)(screenTopLeft.X + TmpLeft), (int)(screenTopLeft.Y + TmpTop),
                    (int)(TmpWidth), (int)(TmpHeight)
                    ), Color.White);
        }
    }
}
