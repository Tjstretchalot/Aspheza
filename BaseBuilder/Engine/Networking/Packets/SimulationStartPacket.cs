using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.Context;
using Lidgren.Network;

namespace BaseBuilder.Engine.Networking.Packets
{
    /// <summary>
    /// A simulation start packet indicates that syncing is complete and each
    /// client should simulate the passing of time, then transition to the 
    /// waiting state.
    /// 
    /// Upon recieving a simulation start packet:
    ///   - Simulate the passing of time.
    ///   - Recycle orders and clear orders.
    ///   - Transition to ConnectionState.Waiting
    ///   - Await SyncStartPacket
    /// </summary>
    public class SimulationStartPacket : GamePacket
    {
        public int SimulationTime;

        public SimulationStartPacket(GamePacketPool pool) : base(pool)
        {
        }

        public override void Clear()
        {
            SimulationTime = -1;
        }

        public override void LoadFrom(NetContext context, NetIncomingMessage message)
        {
            SimulationTime = message.ReadInt32();
        }

        public override void SaveTo(NetContext context, NetOutgoingMessage message)
        {
            message.Write(SimulationTime);
        }
    }
}
