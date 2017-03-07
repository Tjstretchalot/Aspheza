using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
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
        /// Gets the height of this task item when nto expanded on 
        /// the live view, in pixels.
        /// </summary>
        int LiveMinimizedHeight { get; }

        /// <summary>
        /// Gets the height of this task item when expanded on the
        /// live view, in pixels.
        /// </summary>
        int LiveExpandedHeight { get; }

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
        /// <param name="sharedState">The shared state</param>
        /// <param name="localState">The local state</param>
        /// <param name="netContext">The next context</param>
        /// <param name="renderContext">The render context</param>
        void LoadedOrChanged(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext);

        /// <summary>
        /// Create the entity task that corresponds to this task item.
        /// </summary>
        /// <param name="sharedState">The shared state</param>
        /// <param name="localState">The local state</param>
        /// <param name="netContext">The net context</param>
        /// <returns></returns>
        IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        /// <summary>
        /// Draws this task, with the top left at the specified point.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void DrawInspect(RenderContext context, int x, int y);
    }
}
