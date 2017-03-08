using BaseBuilder.Engine.Math2D.Double;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        /// Where the center of this component is.
        /// </summary>
        Point Center { get; set; }

        /// <summary>
        /// The size of this component.
        /// </summary>
        Point Size { get; }

        /// <summary>
        /// Updates the component. 
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="deltaMS">Time in milliseconds since the last call to update</param>
        void Update(ContentManager content, int deltaMS);

        /// <summary>
        /// Handles the mouse state, returning if other components should
        /// get a chance to handle the mouse state
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="last">The last mouse state</param>
        /// <param name="current">The current mouse state</param>
        /// <returns>If the mouse was handled</returns>
        bool HandleMouseState(ContentManager content, MouseState last, MouseState current);

        /// <summary>
        /// Handles the keyboard state, returning if other components should
        /// get a chance to handle the keyboard state.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="last">The last keyboard state</param>
        /// <param name="current">The current keyboard state</param>
        /// <returns>If the keyboard was handled</returns>
        bool HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current);

        /// <summary>
        /// Called prior to the main sprite batch begin
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="graphics">The graphics</param>
        /// <param name="graphicsDevice">The graphics device</param>
        void PreDraw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice);

        /// <summary>
        /// Draws the component
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="graphics">Graphics</param>
        /// <param name="graphicsDevice">Graphics device</param>
        /// <param name="spriteBatch">Sprite batch</param>
        void Draw(ContentManager content, GraphicsDeviceManager graphics, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch);

        /// <summary>
        /// Dispose of any materials
        /// </summary>
        void Dispose();
    }
}
