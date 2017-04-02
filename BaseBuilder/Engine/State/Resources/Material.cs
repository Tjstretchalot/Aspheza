using System;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework.Content;

namespace BaseBuilder.Engine.State.Resources
{
    /// <summary>
    /// Describes a material. This class is designed to be completely agnostic of the quantity of material,
    /// as if it were an enum, and is meant to have each material initialized once.
    /// </summary>
    public class Material : Renderable
    {
        // ** Update GetMaterialByID if modifying ** //
        // ** Update MaterialComboBoxItem if modifying ** //

        public static Material GoldOre { get; }
        public static Material CarrotSeed { get; }
        public static Material Carrot { get; }
        public static Material WheatSeed { get; }
        public static Material Wheat { get; }
        public static Material Flour { get; }
        public static Material Bread { get; }
        public static Material Sugarcane { get; }
        public static Material Sugar { get; }
        public static Material Rum { get; }
        public static Material Wood { get; }
        public static Material Lumber { get; }
        public static Material Sapling { get; }
        public static Material Chicken { get; }
        public static Material Egg { get; }


        // ** Update GetMaterialByID if modifying ** //
        // ** Update MaterialComboBoxItem if modifying ** //

        static Material()
        {
            // ** Update GetMaterialByID if modifying ** //
            // ** Update MaterialComboBoxItem if modifying ** //

            GoldOre = new Material("materials", new Rectangle(132, 0, 32, 32), "Gold Ore", "Gold Ore\nSmelts into gold", 1);

            CarrotSeed = new Material("materials", new Rectangle(33, 0, 32, 32), "Carrot Seed", "Carrot Seed\nCan be planted in a farm to grow into carrots", 2);
            Carrot = new Material("materials", new Rectangle(33, 33, 32, 32), "Carrot", "Carrot\nAn average source of food", 3);

            WheatSeed = new Material("materials", new Rectangle(0, 0, 32, 32), "Wheat Seed", "Wheat Seed\nCan be planted in a farm to grow into wheat", 4);
            Wheat = new Material("materials", new Rectangle(0, 33, 32, 32), "Wheat", "Wheat\nCan be made into flour via a mill. Also serves as\na poor source of food", 5);
            Flour = new Material("materials", new Rectangle(0, 66, 32, 32), "Flour", "Flour\nCan be made into bread via a bakery.", 6);
            Bread = new Material("materials", new Rectangle(0, 99, 32, 32), "Bread", "Bread.", 7);

            Sugarcane = new Material("materials", new Rectangle(99, 0, 32, 32), "Sugarcane", "Sugarcane\nCan be milled into sugar or planted for more sugarcane.", 8);
            Sugar = new Material("materials", new Rectangle(99, 33, 32, 32), "Sugar", "Sugar\nUsed in many finished food.", 9);
            Rum = new Material("materials", new Rectangle(99, 66, 32, 32), "Rum", "Rum\n.", 9);

            Sapling = new Material("materials", new Rectangle(66, 0, 32, 32), "Sapling", "Sapling\nCan be planted for new trees.", 10);
            Wood = new Material("materials", new Rectangle(66, 33, 32, 32), "Wood", "Wood\nCan be made into lumber via a lumbermill.", 11);
            Lumber = new Material("materials", new Rectangle(66, 66, 32, 32), "Lumber", "Lumber\nUsed to make buildings.", 12);

            Chicken = new Material("materials", new Rectangle(33, 66, 32, 32), "Chicken", "Chicken\nCan be placed in a chicken coop to make eggs.", 13);
            Egg = new Material("materials", new Rectangle(33, 99, 32, 32), "Egg", "Egg\nUsed in many finished foods.", 14);
            
            // ** Update GetMaterialByID if modifying ** //
            // ** Update MaterialComboBoxItem if modifying ** //
        }

        public int ID { get; }
        public string Name { get; }
        public string HoverText { get; }

        private string SpriteName;
        private Texture2D Texture;
        private Rectangle SourceRect;

        private Material(string spriteName, Rectangle sourceRect, string name, string hoverText, int id)
        {
            Name = name;
            SpriteName = spriteName;
            SourceRect = sourceRect;
            HoverText = hoverText;
            ID = id;
        }

        public void Render(RenderContext context, PointD2D screenTopLeft, Color overlay)
        {
            if(Texture == null)
                Texture = context.Content.Load<Texture2D>(SpriteName);

            var destRect = new Rectangle((int)screenTopLeft.X, (int)screenTopLeft.Y, 32, 32);

            context.SpriteBatch.Draw(Texture, sourceRectangle: SourceRect, destinationRectangle: destRect, color: overlay);
        }

        /// <summary>
        /// Get as a texture component located at 0,0. The result may be modified.
        /// </summary>
        /// <param name="content">The content manager</param>
        /// <returns>The texture component</returns>
        public TextureComponent GetAsTextureComponent(ContentManager content)
        {
            if (Texture == null)
                Texture = content.Load<Texture2D>(SpriteName);

            return new TextureComponent(Texture, new Rectangle(0, 0, 32, 32), SourceRect, false);
        }

        public static Material GetMaterialByID(int id)
        {
            if (id == GoldOre.ID)
                return GoldOre;
            if (id == CarrotSeed.ID)
                return CarrotSeed;
            if (id == WheatSeed.ID)
                return WheatSeed;
            if (id == Carrot.ID)
                return Carrot;
            if (id == Wheat.ID)
                return Wheat;
            if (id == Flour.ID)
                return Flour;
            if (id == Bread.ID)
                return Bread;
            if (id == Sugarcane.ID)
                return Sugarcane;
            if (id == Sugar.ID)
                return Sugar;
            if (id == Wood.ID)
                return Wood;
            if (id == Lumber.ID)
                return Lumber;
            if (id == Sapling.ID)
                return Sapling;
            if (id == Chicken.ID)
                return Chicken;
            if (id == Egg.ID)
                return Egg;
            if (id == Rum.ID)
                return Rum;

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