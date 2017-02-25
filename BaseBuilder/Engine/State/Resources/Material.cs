using System;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Engine.State.Resources
{
    /// <summary>
    /// Describes a material. This class is designed to be completely agnostic of the quantity of material,
    /// as if it were an enum, and is meant to have each material initialized once.
    /// </summary>
    public class Material : Renderable
    {
        public static Material GoldOre { get; }

        static Material()
        {
            GoldOre = new Material("materials", new Rectangle(0, 0, 16, 16), 1);
        }

        public int ID { get; }

        private string SpriteName;
        private Rectangle SourceRect;

        private Material(string spriteName, Rectangle sourceRect, int id)
        {
            SpriteName = spriteName;
            SourceRect = sourceRect;
            ID = id;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            var destRect = new Rectangle((int)screenTopLeft.X, (int)screenTopLeft.Y, 16, 16);

            context.SpriteBatch.Draw(texture, sourceRectangle: SourceRect, destinationRectangle: destRect, color: overlay);
        }

        public static Material GetMaterialByID(int id)
        {
            if (id == GoldOre.ID)
                return GoldOre;

            throw new InvalidProgramException($"No material with id {id}");
        }
    }
}