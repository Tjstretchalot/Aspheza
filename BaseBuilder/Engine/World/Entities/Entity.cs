using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.Math2D;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.State;
using BaseBuilder.Engine.Utility;
using BaseBuilder.Engine.World.Entities.EntityTasks;
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
    public abstract class Entity : Renderable, ITaskable
    {
        public int ID { get; protected set; }
        /// <summary>
        /// Where the entity is located in the world.
        /// </summary>
        public PointD2D Position { get; protected set; }

        /// <summary>
        /// The polygon that can be used for intersection. Recall this does not itself
        /// reflect the position.
        /// </summary>
        public PolygonD2D CollisionMesh { get; protected set; }

        /// <summary>
        /// Not-null if this entity has text on hover
        /// </summary>
        protected string _HoverText { get; set; }

        public virtual string HoverText
        {
            get
            {
                if (_HoverText != null)
                    return _HoverText;

                if (CurrentTask != null)
                    return CurrentTask.TaskDescription;
                return $"Position = ({Position.X}, {Position.Y})";
            }
        }

        /// <summary>
        /// Is this entity currently selected
        /// </summary>
        public bool Selected;

        public Queue<IEntityTask> Tasks;
        public IEntityTask CurrentTask;

        protected Entity(PointD2D position, PolygonD2D collisionMesh, int id)
        {
            Position = position;
            CollisionMesh = collisionMesh;
            ID = id;
            Tasks = new Queue<IEntityTask>();
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
            Tasks = new Queue<IEntityTask>(numTasks);
            for(int i = 0; i < numTasks; i++)
            {
                var taskID = message.ReadInt16();
                Tasks.Enqueue(TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message));
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
            message.Write((short)Tasks.Count);
            for(int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks.ElementAt(i);

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
            CollisionMesh = new PolygonD2D(message);
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
            if (CurrentTask == null && Tasks.Count > 0)
            {
                CurrentTask = Tasks.Dequeue();
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

        protected void Init(PointD2D position, PolygonD2D collisionMesh)
        {
            Position = position;
            CollisionMesh = collisionMesh;
        }

        /// <summary>
        /// Returns true if this Entity contains the specified point. Returns false otherwise.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <param name="strict">False if touching constitutes intersection, true otherwise.</param>
        /// <returns></returns>
        public bool Contains(PointD2D point, bool strict = false)
        {
            return CollisionMesh.Contains(point, Position, strict);
        }

        public void TilesIntersectedAt(List<PointI2D> list)
        {
            CollisionMesh.TilesIntersectedAt(Position, list);
        }

        public void QueueTask(IEntityTask task)
        {
            Tasks.Enqueue(task);
        }

        public void ClearTasks()
        {
            CurrentTask = null;
            Tasks.Clear();
        }
    }
}
