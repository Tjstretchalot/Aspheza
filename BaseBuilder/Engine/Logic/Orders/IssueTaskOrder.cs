using BaseBuilder.Engine.Networking.Packets;
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
using BaseBuilder.Engine.World.Entities.EntityTasks;
using BaseBuilder.Engine.World.Entities;
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.Logic.Orders
{
    public class IssueTaskOrder : GamePacket, IOrder
    {
        public Entity Entity;
        public IEntityTask Task;

        public IssueTaskOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            Entity = null;
            Task = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            int entID = message.ReadInt32();
            bool mobile = message.ReadBoolean();

            if (mobile)
                Entity = gameState.World.MobileEntities.Find((e) => e.ID == entID);
            else
                Entity = gameState.World.ImmobileEntities.Find((e) => e.ID == entID);

            var taskTypeID = message.ReadInt16();
            Task = TaskIdentifier.InitEntityTask(TaskIdentifier.GetTypeOfID(taskTypeID), gameState, message);
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(Entity.ID);
            message.Write(typeof(MobileEntity).IsAssignableFrom(Entity.GetType()));
            message.Write(TaskIdentifier.GetIDOfTask(Task.GetType()));
            Task.Write(message);
        }
    }
}
