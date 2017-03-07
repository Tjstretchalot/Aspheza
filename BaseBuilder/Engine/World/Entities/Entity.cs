using BaseBuilder.Engine.Context;
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
        public int ID { get; protected set; }

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
        /// Not-null if this entity has text on hover
        /// </summary>
        protected string _HoverText { get; set; }

        public virtual string HoverText
        {
            get
            {
                return _HoverText;
            }
        }
        

        /// <summary>
        /// Is this entity currently selected
        /// </summary>
        public bool Selected;

        public Queue<IEntityTask> TaskQueue { get; set; }
        public IEntityTask CurrentTask { get; set; }

        public event EventHandler TaskFinishing;
        public event EventHandler TasksCancelled;
        public event EventHandler TasksReplacing;
        public event EventHandler TasksReplaced;

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

        /// <summary>
        /// Called when entity moves
        /// </summary>
        /// <param name="sharedState"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public virtual void OnMove(SharedGameState sharedState, int timeMS, double dx, double dy)
        {

        }

        /// <summary>
        /// Called when entity completes a move
        /// </summary>
        /// <param name="sharedState"></param>
        public virtual void OnStop(SharedGameState sharedState)
        {

        }

        public virtual void SimulateTimePassing(SharedGameState sharedState, int timeMS)
        {
            if (CurrentTask == null && TaskQueue.Count > 0)
            {
                CurrentTask = TaskQueue.Dequeue();
            }

            if (CurrentTask != null)
            {
                var status = CurrentTask.SimulateTimePassing(sharedState, timeMS);

                if(status != EntityTaskStatus.Running)
                {
                    CurrentTask = null;
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
            TaskQueue.Enqueue(task);
        }

        public void ClearTasks(SharedGameState gameState)
        {
            CurrentTask?.Cancel(gameState);
            CurrentTask = null;

            while(TaskQueue.Count > 0)
            {
                TaskQueue.Dequeue().Cancel(gameState);
            }
        }

        public void ReplaceTasks(Queue<IEntityTask> newQueue)
        {
            throw new NotImplementedException();
        }
    }
}
