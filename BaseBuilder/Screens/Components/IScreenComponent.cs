using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.Components
{
    /// <summary>
    /// Describes a component of a screen.
    /// </summary>
    public interface IScreenComponent
    {
        /// <summary>
        /// Where this component is located
        /// </summary>
        Rectangle Location { get; }

        /// <summary>
        /// Updates the component. 
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="deltaMS">Time in milliseconds since the last call to update</param>
        void Update(ContentManager content, int deltaMS);

        /// <summary>
        /// Draws the component
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);
    }
}
