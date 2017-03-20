using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Screens.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// A combo box item for a material combo box.
    /// </summary>
    public class MaterialComboBoxItem : ComboBoxItem<Material>
    {
        public static List<ComboBoxItem<Material>> AllMaterialsWithFont(SpriteFont font)
        {
            // sort alphabetically
            return new List<ComboBoxItem<Material>>
            {
                new MaterialComboBoxItem(font, Material.Bread),
                new MaterialComboBoxItem(font, Material.Carrot),
                new MaterialComboBoxItem(font, Material.CarrotSeed),
                new MaterialComboBoxItem(font, Material.Flour),
                new MaterialComboBoxItem(font, Material.GoldOre),
                new MaterialComboBoxItem(font, Material.Lumber),
                new MaterialComboBoxItem(font, Material.Sapling),
                new MaterialComboBoxItem(font, Material.Sugar),
                new MaterialComboBoxItem(font, Material.Sugarcane),
                new MaterialComboBoxItem(font, Material.Wheat),
                new MaterialComboBoxItem(font, Material.WheatSeed),
                new MaterialComboBoxItem(font, Material.Wood),
            };
        }
        
        public Material Material;

        protected RenderContext ReusableContext;

        public MaterialComboBoxItem(SpriteFont font, Material material) : base(font, null, material)
        {
            Material = material;
            Text = material.Name;

            ReusableContext = new RenderContext();
        }

        public override void Initialize(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice)
        {
            base.Initialize(content, graphics, graphicsDevice);

            MinSize.X = MinSize.X + 35;
            MinSize.Y = Math.Max(MinSize.Y, 32);
        }
        public override void DrawImpl(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int x, int y)
        {
            DrawRect.X = x;
            DrawRect.Y = y;
            DrawRect.Width = Size.X;
            DrawRect.Height = Size.Y;
            
            spriteBatch.Draw(BackgroundTexture, new Rectangle(x, y, Size.X, Size.Y), Color.White);
            
            ReusableContext.Content = content;
            ReusableContext.Graphics = graphics;
            ReusableContext.GraphicsDevice = graphicsDevice;
            ReusableContext.SpriteBatch = spriteBatch;
            ReusableContext.DefaultFont = Font;

            Material.Render(ReusableContext, new PointD2D(DrawRect.Center.X - MinSize.X / 2, DrawRect.Center.Y - 16), Hovered ? Color.White : Color.LightGray);
            spriteBatch.DrawString(Font, Text, new Vector2(DrawRect.Center.X - MinSize.X / 2 + 35, DrawRect.Center.Y - Font.LineSpacing / 2), Hovered ? Color.White : Color.LightGray);
        }
    }
}