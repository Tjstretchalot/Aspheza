using System;
using System.Collections.Generic;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using Microsoft.Xna.Framework.Input;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public abstract class TaskItem : ITaskItem
    {
        public abstract event EventHandler InspectAddPressed;
        public abstract event EventHandler InspectDeletePressed;
        public abstract event EventHandler InspectRedrawRequired;
        public abstract event EventHandler InspectSaveRequired;

        public virtual List<ITaskItem> Children { get; protected set; }
        public virtual bool Expandable { get; protected set; }

        public virtual bool Expanded { get; set; }

        public virtual PointI2D InspectSize { get; protected set; }

        public virtual string TaskName { get; protected set; }

        public ITaskItem Parent { get; set; }

        public IEntityTask Task { get; set; }

        public abstract IEntityTask CreateEntityTask(ITaskable taskable, SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        public abstract void PreDrawInspect(RenderContext context, int x, int y);
        public abstract void DrawInspect(RenderContext context, int x, int y);

        public abstract bool IsValid(SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        public abstract void LoadedOrChanged(RenderContext renderContext);

        public virtual void UpdateInspect(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, int timeMS)
        {
        }

        public virtual bool HandleInspectMouseState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, MouseState last, MouseState current)
        {
            return false;
        }

        public virtual bool HandleInspectKeyboardState(SharedGameState sharedGameState, LocalGameState localGameState, NetContext netContext, KeyboardState last, KeyboardState current)
        {
            return false;
        }

        public abstract void DisposeInspect();

        public virtual bool CanBeAssignedTo(ITaskable taskable)
        {
            return true;
        }
    }
}