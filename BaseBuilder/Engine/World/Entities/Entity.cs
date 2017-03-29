﻿using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities.Utilities;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.World.WorldObject.Entities
{
    /// <summary>
    /// An entity is anything that isn't a tile.
    /// </summary>
    /// <remarks>
    /// Entities have a position and a collision mesh. Like a vertex mesh in 3D games is a collection
    /// of polygons, the CollisionMesh is the 2D equivalent (a collection of lines). If Entities are 
    /// bounded and convex and do not have any round sections, which we will assume is true, a collection
    /// of lines is simply a polygon.
    /// 
    /// Furthermore, entities are able to render themselves.
    /// </remarks>
    public abstract class Entity : Renderable, ITaskable, Thing
    {
        public int ID { get; set; }

        public bool Destroyed { get; set; }

        /// <summary>
        /// Where the entity is located in the world.
        /// </summary>
        public PointD2D Position { get; set; }

        /// <summary>
        /// The collision mesh that can be used for intersection. Recall this does not itself
        /// reflect the position.
        /// </summary>
        public CollisionMeshD2D CollisionMesh { get; protected set; }

        /// <summary>
        /// The list of preferred adjacent points and directions to face, or null if no preference.
        /// </summary>
        public virtual List<Tuple<PointI2D, Direction>> PreferredAdjacentPoints { get { return null; } }

        /// <summary>
        /// Not-null if this entity has text on hover
        /// </summary>
        protected string _HoverText { get; set; }

        public virtual string HoverText
        {
            get
            {
                if(_HoverText == null)
                {
                    return $"ID = {ID}, Pos = {Position}";
                }
                return _HoverText;
            }
        }
        

        /// <summary>
        /// Is this entity currently selected
        /// </summary>
        public bool Selected;

        protected bool _IsPaused;
        /// <summary>
        /// True if we are not running our tasks right now
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return _IsPaused;
            }

            set
            {
                if(_IsPaused != value)
                {
                    _IsPaused = value;
                    PausedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler PausedChanged;

        public Queue<IEntityTask> TaskQueue { get; set; }
        public IEntityTask CurrentTask { get; set; }

        public event EventHandler TaskFinishing;
        public event EventHandler TasksCancelled;
        public event EventHandler TasksReplacing;
        public event EventHandler TasksReplaced;
        public event EventHandler TaskFinished;
        public event EventHandler TaskStarting;
        public event EventHandler TaskStarted;
        public event EventHandler TaskQueueing;
        public event EventHandler TaskQueued;

        protected Entity(PointD2D position, CollisionMeshD2D collisionMesh, int id)
        {
            Position = position;
            CollisionMesh = collisionMesh;
            ID = id;
            TaskQueue = new Queue<IEntityTask>();
        }

        /// <summary>
        /// This should only be used with FromMessage
        /// </summary>
        protected Entity()
        {
        }
        
        protected virtual void TasksFromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            _IsPaused = message.ReadBoolean();
            var numTasks = message.ReadInt16();
            TaskQueue = new Queue<IEntityTask>(numTasks);
            for(int i = 0; i < numTasks; i++)
            {
                var taskID = message.ReadInt16();
                TaskQueue.Enqueue(TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message));
            }

            bool currentTask = message.ReadBoolean();
            if(currentTask)
            {
                var taskID = message.ReadInt16();
                CurrentTask = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
            }
        }

        protected virtual void WriteTasks(NetOutgoingMessage message)
        {
            message.Write(_IsPaused);
            message.Write((short)TaskQueue.Count);
            for(int i = 0; i < TaskQueue.Count; i++)
            {
                var task = TaskQueue.ElementAt(i);

                message.Write(TaskIdentifier.GetIDOfTask(task.GetType()));
                task.Write(message);
            }

            if(CurrentTask == null)
            {
                message.Write(false);
            }
            else
            {
                message.Write(true);
                message.Write(TaskIdentifier.GetIDOfTask(CurrentTask.GetType()));
                CurrentTask.Write(message);
            }
        }

        public virtual void FromMessage(SharedGameState gameState, NetIncomingMessage message)
        {
            Position = new PointD2D(message);
            CollisionMesh = new CollisionMeshD2D(message);
            ID = message.ReadInt32();

            TasksFromMessage(gameState, message);
        }

        public virtual void Write(NetOutgoingMessage message)
        {
            Position.Write(message);
            CollisionMesh.Write(message);
            message.Write(ID);

            WriteTasks(message);
        }

        /// <summary>
        /// See <see cref="Renderable"/>
        /// </summary>
        public abstract void Render(RenderContext context, PointD2D screenTopLeft, Color overlay);

        /// <summary>
        /// Updates this entity in the given context. Note that this should only do visual
        /// related updates, like ticking an animation. 
        /// </summary>
        /// <param name="context"></param>
        public virtual void Update(UpdateContext context)
        {
        }
        
        public virtual void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            if (IsPaused)
                return;

            if (CurrentTask == null && TaskQueue.Count > 0)
            {
                TaskStarting?.Invoke(null, EventArgs.Empty);
                CurrentTask = TaskQueue.Dequeue();
                TaskStarted?.Invoke(null, EventArgs.Empty);
            }

            if (CurrentTask != null)
            {
                var taskstatus = CurrentTask.SimulateTimePassing(sharedState, timeMS);

                if (taskstatus != EntityTaskStatus.Running)
                {
                    TaskFinishing?.Invoke(null, EventArgs.Empty);
                    if (TaskQueue.Count > 0)
                    {
                        TaskStarting?.Invoke(null, EventArgs.Empty);
                        CurrentTask = TaskQueue.Dequeue();
                        TaskFinished?.Invoke(null, EventArgs.Empty);
                        TaskStarted?.Invoke(null, EventArgs.Empty);
                    }
                    else
                    {
                        CurrentTask = null;
                        TaskFinished?.Invoke(null, EventArgs.Empty);
                    }
                }
            }

        }

        protected void Init(PointD2D position, CollisionMeshD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// Returns true if this Entity contains the specified point. Returns false otherwise.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>If this entity contains the specified point</returns>
        public bool Contains(PointD2D point)
        {
            return CollisionMesh.Contains(point, Position);
        }

        public void TilesIntersectedAt(List<PointI2D> list)
        {
            CollisionMesh.TilesIntersectedAt(Position, list);
        }

        public void QueueTask(IEntityTask task)
        {
            TaskQueueing?.Invoke(null, EventArgs.Empty);
            TaskQueue.Enqueue(task);
            TaskQueued?.Invoke(null, EventArgs.Empty);
        }

        public void ClearTasks(SharedGameState gameState)
        {
            TasksCancelled?.Invoke(null, EventArgs.Empty);

            CurrentTask?.Cancel(gameState);
            CurrentTask = null;

            while(TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue().Cancel(gameState);
            }
        }

        public void ReplaceTasks(Queue<IEntityTask> newQueue)
        {
            TasksReplacing?.Invoke(null, EventArgs.Empty);

            CurrentTask = null;
            TaskQueue.Clear();
            TaskQueue = newQueue;
            if(TaskQueue.Count > 0)
                CurrentTask = TaskQueue.Dequeue();

            TasksReplaced?.Invoke(null, EventArgs.Empty);
        }
    }
}
