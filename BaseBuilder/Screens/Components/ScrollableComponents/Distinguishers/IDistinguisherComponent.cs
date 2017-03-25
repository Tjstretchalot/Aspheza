using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components.ScrollableComponents.Distinguishers
{
    /// <summary>
    /// Describes a component of a distinguisher.
    /// </summary>
    public interface IDistinguisherComponent
    {
        /// <summary>
        /// Called prior to begining a spritebatch. Can be used for initialization or offscreen
        /// rendering.
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="topLeftX">Top-left x of bounding box</param>
        /// <param name="topLeftY">Top-left y of bounding box</param>
        /// <param name="width">Width of bounding box</param>
        /// <param name="height">Height of bounding box</param>
        void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, int topLeftX, int topLeftY, int width, int height);

        /// <summary>
        /// Renders this distinguisher. Called before rendering actual components, but 
        /// among components it depends how the distinguisher was constructed
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        /// <param name="topLeftX">Top-left x of bounding box</param>
        /// <param name="topLeftY">Top-left y of bounding box</param>
        /// <param name="width">Width of bounding box</param>
        /// <param name="height">Height of bounding box</param>
        void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, int topLeftX, int topLeftY, int width, int height);

        /// <summary>
        /// Dispose of any unmanaged resources created by this component
        /// </summary>
        void Dispose();
    }
}
