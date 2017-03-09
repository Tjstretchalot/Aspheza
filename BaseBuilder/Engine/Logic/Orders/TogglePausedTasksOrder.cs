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
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.Logic.Orders
{
    /// <summary>
    /// Toggles if an entity is running his tasks.
    /// </summary>
    public class TogglePausedTasksOrder : GamePacket, IOrder
    {
        public Entity Entity;

        public TogglePausedTasksOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            Entity = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            int entID = message.ReadInt32();
            bool mobile = message.ReadBoolean();

            if (mobile)
                Entity = gameState.World.MobileEntities.Find((m) => m.ID == entID);
            else
                Entity = gameState.World.ImmobileEntities.Find((im) => im.ID == entID);
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            message.Write(Entity.ID);
            message.Write(typeof(MobileEntity).IsAssignableFrom(Entity.GetType()));
        }
    }
}
