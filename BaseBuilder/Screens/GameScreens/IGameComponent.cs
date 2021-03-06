﻿using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens
{
    /// <summary>
    /// Describes this that can be rendered on the game screen
    /// </summary>
    public interface IMyGameComponent
    {
        /// <summary>
        /// Where the top-left of this component is on the screen.
        /// </summary>
        PointI2D ScreenLocation { get; set; }

        /// <summary>
        /// The size of this component.
        /// </summary>
        PointI2D Size { get; }

        /// <summary>
        /// The "Z" of this game component right now. Game components get the chance
        /// to act on the mouse state / keyboard state in order of descending Z. They 
        /// are drawn in order of ascending Z. When  the Z of a component has changed, 
        /// LocalGameLogic#UpdateComponentZ should be called
        /// </summary>
        /// <see cref="Engine.Logic.LocalGameLogic.UpdateComponentZ(IMyGameComponent)"/>
        int Z { get; }

        /// <summary>
        /// Handles the state of the mouse for this component. 
        /// </summary>
        /// <param name="sharedGameState">The shared state of the game</param>
        /// <param name="localGameState">The local state of the game</param>
        /// <param name="netContext">The net context</param>
        /// <param name="last">The mouse state last frame</param>
        /// <param name="current">The mouse state this frame</param>
        /// <param name="handled">If the mouse has been handled</param>
        /// <param name="scrollHandled">If scrolling has specifically been handled</param>
        void HandleMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current, ref bool handled, ref bool scrollHandled);

        /// <summary>
        /// Handles the state of the keyboard for this component. Returns true if this component
        /// has acted on the state of the keyboard, returns false otherwise. If this component returns
        /// true, no more components will have HandleKeyboardState called this frame.
        /// </summary>
        /// <param name="sharedGameState">The shared state of the game</param>
        /// <param name="localGameState">The local state of the game</param>
        /// <param name="last">The keyboard state last frame</param>
        /// <param name="current">The keyboard state this frame</param>
        /// <returns>True if this component hnadled the keyboard, false otherwise</returns>
        bool HandleKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current, List<Keys> keysReleasedThisFrame);

        /// <summary>
        /// This is called prior to the main SpriteBatch#Begin and can be used to
        /// write offscreen. 
        /// </summary>
        /// <param name="renderContext">Render context</param>
        void PreDraw(RenderContext renderContext);

        /// <summary>
        /// Draws this component to the screen
        /// </summary>
        void Draw(RenderContext context);

        /// <summary>
        /// Dispose any parts of this component that need disposing
        /// </summary>
        void Dispose();

        /// <summary>
        /// Called every frame, regardless of the mouse/keyboard state.
        /// </summary>
        /// <param name="sharedGameState">The shared game state</param>
        /// <param name="localGameState">The local game state</param>
        void Update(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS);
    }
}
