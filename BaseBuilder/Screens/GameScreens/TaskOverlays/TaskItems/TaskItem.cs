using System;
using System.Collections.Generic;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.World.Entities.EntityTasks;

namespace BaseBuilder.Screens.GameScreens.TaskOverlays.TaskItems
{
    public abstract class TaskItem : ITaskItem
    {
        public virtual List<ITaskItem> Children { get; protected set; }
        public virtual bool Expandable { get; protected set; }

        public virtual bool Expanded { get; set; }

        public virtual PointI2D InspectSize { get; protected set; }

        public virtual int LiveExpandedHeight { get; protected set; }

        public virtual int LiveMinimizedHeight { get; protected set; }

        public virtual string TaskName { get; protected set; }

        public abstract IEntityTask CreateEntityTask(SharedGameState sharedState, LocalGameState localState, NetContext netContext);

        public abstract void DrawInspect(RenderContext context, int x, int y);

        public abstract void LoadedOrChanged(SharedGameState sharedState, LocalGameState localState, NetContext netContext, RenderContext renderContext);
    }
}