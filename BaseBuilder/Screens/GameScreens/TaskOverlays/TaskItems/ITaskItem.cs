using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    /// <summary>
    /// Describes an item on task overlay. Task items must be added to the
    /// TaskItemIdentifier when created, which allows going from EntityTasks
    /// to TaskItems.
    /// </summary>
    public interface ITaskItem
    {
        /// <summary>
        /// Triggered when this task item will require a redraw.
        /// </summary>
        event EventHandler InspectRedrawRequired;

        /// <summary>
        /// Triggered when this task item is drawn in the inspect
        /// overlay and wants to add a child.
        /// </summary>
        event EventHandler InspectAddPressed;

        /// <summary>
        /// Triggered when this task item is drawn in the 
        /// delete overlay and wants to delete itself.
        /// </summary>
        event EventHandler InspectDeletePressed;

        /// <summary>
        /// Triggered when the task item wants to save
        /// its changes to the shared state.
        /// </summary>
        event EventHandler InspectSaveRequired;

        /// <summary>
        /// Gets or sets the parent of this task item.
        /// </summary>
        ITaskItem Parent { get; set;  }

        /// <summary>
        /// Gets or sets the task that corresponds with this task item
        /// </summary>
        IEntityTask Task { get; set; }

        /// <summary>
        /// Gets the children of this task item. A task item does
        /// not draw its own children in the live menu.
        /// </summary>
        List<ITaskItem> Children { get; }

        /// <summary>
        /// If this task item can be expanded in the live view. If expandable is
        /// true, than must have children.
        /// </summary>
        bool Expandable { get; }

        /// <summary>
        /// If this task item is expanded. Can only be
        /// true if this item is expandable
        /// </summary>
        bool Expanded { get; set; }

        /// <summary>
        /// The size that this item takes up in the inspect menu.
        /// </summary>
        PointI2D InspectSize { get; }

        /// <summary>
        /// Gets the name of this task in the add task menu and live task menu.
        /// </summary>
        string TaskName { get; }

        /// <summary>
        /// Determines if this task item is currently in a valid state (i.e. able to create a task)
        /// </summary>
        /// <param name="sharedState">the shared state</param>
        /// <param name="localState">the local state</param>
        /// <param name="netContext">the net context</param>
        /// <returns>If this task item is in a good state</returns>
        bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        /// <summary>
        /// Called before accessing any visual-related components or when there
        /// was a change to the underlying task.
        /// </summary>
        /// <param name="renderContext">The render context</param>
        void LoadedOrChanged(RenderContext renderContext);

        /// <summary>
        /// Create the entity task that corresponds to this task item.
        /// </summary>
        /// <param name="sharedState">The shared state</param>
        /// <param name="localState">The local state</param>
        /// <param name="netContext">The net context</param>
        /// <returns></returns>
        IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        /// <summary>
        /// Called prior to draw and prior to the sprite batch begin.
        /// </summary>
        /// <param name="context">Context</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        void PreDrawInspect(RenderContext context, int x, int y);

        /// <summary>
        /// Draws this task, with the top left at the specified point.
        /// </summary>
        /// <param name="context">Render context</param>
        /// <param name="x">Top-left x</param>
        /// <param name="y">Top-left y</param>
        void DrawInspect(RenderContext context, int x, int y);

        /// <summary>
        /// Updates this task item when in the mouse state, for things like
        /// button presses.
        /// </summary>
        /// <param name="sharedGameState">Shared state</param>
        /// <param name="localGameState">Local state</param>
        /// <param name="netContext">The net context</param>
        /// <param name="timeMS">The time in milliseconds since last update</param>
        void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS);

        /// <summary>
        /// Handles the mouse state in the inspect menu. Only called if currently live
        /// in the mouse state and the mouse is inside the mouse state and it is this
        /// task items turn to consume the mouse. The mouse positions are adjusted to
        /// be relative to the draw position (i.e. if your drawn on an offscreen texture at
        /// 0,0 and that texture is drawn onto the screen at 50,50, the mouse positions
        /// here will be based on 0,0 not 50,50)
        /// </summary>
        /// <param name="sharedGameState">Shared game state</param>
        /// <param name="localGameState">Local game state</param>
        /// <param name="netContext">Net context</param>
        /// <param name="last">The last mouse state</param>
        /// <param name="current">The current mouse state</param>
        /// <returns>If the mouse was handled</returns>
        bool HandleInspectMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current);

        /// <summary>
        /// Handles the keyboard state in the inspect menu.
        /// </summary>
        /// <param name="sharedGameState">Shared state</param>
        /// <param name="localGameState">Local state</param>
        /// <param name="netContext">Net context</param>
        /// <param name="last">The last keyboard state</param>
        /// <param name="current">The current keyboard state</param>
        /// <returns>If the keyboard was handled</returns>
        bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current);

        /// <summary>
        /// Disposes of anything required for the inspect menu. It should still be
        /// able to run again if LoadedOrChanged is called.
        /// </summary>
        void DisposeInspect();
    }
}
