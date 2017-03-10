﻿using BaseBuilder.Engine.Math2D.Double;
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
        /// If this component works better when it's on top of other components.
        /// Typically this means it does not respect its size.
        /// </summary>
        bool HighPriorityZ { get; }

        /// <summary>
        /// Updates the component. 
        /// </summary>
        /// <param name="content">Content manager</param>
        /// <param name="deltaMS">Time in milliseconds since the last call to update</param>
        void Update(ContentManager content, int deltaMS);

        /// <summary>
        /// Handles the mouse state. There is a boolean to mark if events have been handled
        /// already.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="last">The last mouse state</param>
        /// <param name="current">The current mouse state</param>
        /// <param name="handled">If the mouse has been "handled". This means don't act on clicks, but ok to handle position</param>
        /// <param name="scrollHandled">If scrolling has been done already</param>
        /// <returns>If the mouse was handled</returns>
        void HandleMouseState(ContentManager content, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled);

        /// <summary>
        /// Handles the keyboard state, returning if other components should
        /// get a chance to handle the keyboard state.
        /// </summary>
        /// <param name="content">The content</param>
        /// <param name="last">The last keyboard state</param>
        /// <param name="current">The current keyboard state</param>
        /// <param name="handled">If the keyboard has been handled</param>
        /// <returns>If the keyboard was handled</returns>
        void HandleKeyboardState(ContentManager content, KeyboardState last, KeyboardState current, ref bool handled);

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
