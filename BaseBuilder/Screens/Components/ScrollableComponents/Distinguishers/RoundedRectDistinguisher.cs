using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BaseBuilder.Screens.Components.ScrollableComponents.Distinguishers
{
    /// <summary>
    /// This distinguisher draws a rounded rect in the background.
    /// </summary>
    public class RoundedRectDistinguisher : IDistinguisherComponent
    {
        protected Texture2D Texture;
        protected Color CenterColor;
        protected Color InnerBorderColor;
        protected Color OuterBorderColor;
        protected int Radius;
        protected int InnerBorder;
        protected int OuterBorder;

        /// <summary>
        /// Initialize a new rounded rect distinguisher.
        /// </summary>
        /// <param name="centerColor">The color inside the borders</param>
        /// <param name="innerBorderColor">The color of the inner border</param>
        /// <param name="outerBorderColor">The color of the outer border</param>
        /// <param name="radius">The radius of the corners</param>
        /// <param name="innerBorder">How far from the edge is still outside the inner border. Must be larger than outer border to be visible</param>
        /// <param name="outerBorder">How far from the edge is still outside the outer border.</param>
        public RoundedRectDistinguisher(Color centerColor, Color innerBorderColor, Color outerBorderColor, int radius, int innerBorder, int outerBorder)
        {
            CenterColor = centerColor;
            InnerBorderColor = innerBorderColor;
            OuterBorderColor = outerBorderColor;
            Radius = radius;
            InnerBorder = innerBorder;
            OuterBorder = outerBorder;
        }

        public void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, int topLeftX, int topLeftY, int width, int height)
        {
            if(Texture == null)
            {
                Texture = RoundedRectUtils.CreateRoundedRect(content, graphics, graphicsDevice, width, height, CenterColor, InnerBorderColor, OuterBorderColor, Radius,
                    InnerBorder, OuterBorder);
            }
        }

        public void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int topLeftX, int topLeftY, int width, int height)
        {
            if(Texture != null)
            {
                spriteBatch.Draw(Texture, new Rectangle(topLeftX, topLeftY, width, height), Color.White);
            }
        }

        public void Dispose()
        {
            Texture?.Dispose();
            Texture = null;
        }
    }
}
