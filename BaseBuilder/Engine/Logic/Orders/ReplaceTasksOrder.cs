using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.World.Entities.EntityTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class ReplaceTasksOrder : GamePacket, IOrder
    {
        public Entity Entity;
        public List<IEntityTask> NewQueue;

        public ReplaceTasksOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            Entity = null;
            NewQueue = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            var entID = message.ReadInt32();
            var mobile = message.ReadBoolean();

            if (mobile)
                Entity = gameState.World.MobileEntities.Find((m) => m.ID == entID);
            else
                Entity = gameState.World.ImmobileEntities.Find((im) => im.ID == entID);

            int taskLen = message.ReadInt32();

            NewQueue = new List<IEntityTask>(taskLen);

            for(int i = 0; i < taskLen; i++)
            {
                var taskID = message.ReadInt16();

                var task = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskID), gameState, message);
                NewQueue.Add(task);
            }
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(Entity.ID);
            message.Write(typeof(MobileEntity).IsAssignableFrom(Entity.GetType()));

            message.Write(NewQueue.Count);

            foreach(var task in NewQueue)
            {
                var id = TaskIdentifier.GetIDOfTask(task.GetType());

                message.Write(id);
                task.Write(message);
            }
        }
    }
}
