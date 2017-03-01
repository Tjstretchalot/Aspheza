using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using BaseBuilder.Engine.Networking.Packets;
using BaseBuilder.Engine.Networking;
using BaseBuilder.Engine.World.Entities.Utilities;
using BaseBuilder.Engine.World.Entities.MobileEntities;

namespace BaseBuilder.Engine.Logic.Orders
{
    /// <summary>
    /// An order to move items from one container to another
    /// </summary>
    public class TransferItemsOrder : GamePacket, IOrder
    {
        public Container From;
        public Container To;
        public int Index;

        public TransferItemsOrder(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            From = null;
            To = null;
        }

        public override void LoadFrom(NetContext context, SharedGameState gameState, NetIncomingMessage message)
        {
            bool fromIsMobile = message.ReadBoolean();
            int fromID = message.ReadInt32();

            bool toIsMobile = message.ReadBoolean();
            int toID = message.ReadInt32();

            Index = message.ReadInt32();


            if (fromIsMobile)
                From = (Container)gameState.World.MobileEntities.Find((m) => m.ID == fromID);
            else
                From = (Container)gameState.World.ImmobileEntities.Find((im) => im.ID == fromID);
            
            if (toIsMobile)
                To = (Container)gameState.World.MobileEntities.Find((m) => m.ID == toID);
            else
                To = (Container)gameState.World.ImmobileEntities.Find((im) => im.ID == toID);
        }

        public override void SaveTo(NetContext context, SharedGameState gameState, NetOutgoingMessage message)
        {
            bool fromIsMobile = typeof(MobileEntity).IsAssignableFrom(From.GetType());
            bool toIsMobile = typeof(MobileEntity).IsAssignableFrom(To.GetType());

            message.Write(fromIsMobile);
            message.Write(From.ID);

            message.Write(toIsMobile);
            message.Write(To.ID);

            message.Write(Index);
        }
    }
}
