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
        public static Material CarrotSeed { get; }
        public static Material WheatSeed { get; }
        public static Material Carrot { get; }
        public static Material Wheat { get; }

        static Material()
        {
            GoldOre = new Material("materials", new Rectangle(0, 0, 32, 32), "Gold Ore\nSmelts into gold", 1);
            CarrotSeed = new Material("materials", new Rectangle(32, 0, 32, 32), "Carrot Seed\nCan be planted in a farm to grow into carrots", 2);
            WheatSeed = new Material("materials", new Rectangle(64, 0, 32, 32), "Wheat Seed\nCan be planted in a farm to grow into wheat", 3);
            Carrot = new Material("materials", new Rectangle(32, 0, 32, 32), "Carrot\nAn average source of food", 4);
            Wheat = new Material("materials", new Rectangle(64, 0, 32, 32), "Wheat\nCan be made into flour via a mill. Also serves as\na poor source of food", 5);
        }

        public int ID { get; }

        public string HoverText { get; }

        private string SpriteName;
        private Rectangle SourceRect;

        private Material(string spriteName, Rectangle sourceRect, string hoverText, int id)
        {
            SpriteName = spriteName;
            SourceRect = sourceRect;
            HoverText = hoverText;
            ID = id;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            var texture = context.Content.Load<Texture2D>(SpriteName);

            var destRect = new Rectangle((int)screenTopLeft.X, (int)screenTopLeft.Y, 32, 32);

            context.SpriteBatch.Draw(texture, sourceRectangle: SourceRect, destinationRectangle: destRect, color: overlay);
        }

        public static Material GetMaterialByID(int id)
        {
            if (id == GoldOre.ID)
                return GoldOre;

            throw new InvalidProgramException($"No material with id {id}");
        }

        public static bool operator ==(Material m1, Material m2)
        {
            if (ReferenceEquals(m1, null) && ReferenceEquals(m2, null))
                return true;

            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null))
                return false;

            return m1.ID == m2.ID;
        }

        public static bool operator !=(Material m1, Material m2)
        {
            if (ReferenceEquals(m1, null) && ReferenceEquals(m2, null))
                return false;

            if (ReferenceEquals(m1, null) || ReferenceEquals(m2, null))
                return true;

            return m1.ID != m2.ID;
        }

        public override bool Equals(object obj)
        {
            var m2 = obj as Material;

            return this == m2;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override string ToString()
        {
            return $"Material [ID={ID}, HoverText={HoverText}]";
        }
    }
}