using System;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.GameScreens.BuildOverlays
{
    public abstract class SimpleBuildOverlayMenuItem : BuildOverlayMenuItem
    {
        protected string SpriteName;

        public SimpleBuildOverlayMenuItem(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, bool selectable, string spriteName) : base(content, graphics, graphicsDevice, spriteBatch, selectable)
        {
            SpriteName = spriteName;
        }

        public override void Render(Rectangle menuScreenLocation, int yOffset, bool selected)
        {
            var texture = Content.Load<Texture2D>(SpriteName);

            DrawTexture(menuScreenLocation, yOffset, texture, Location, new Rectangle(0, 0, texture.Width, texture.Height), selected ? Color.Azure : Color.White);
        }

        public override void Update(int elapsedMS, bool selected)
        {
        }
    }
}