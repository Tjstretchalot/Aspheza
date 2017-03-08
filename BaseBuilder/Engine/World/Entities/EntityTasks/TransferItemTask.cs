using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseBuilder.Engine.State;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;
using BaseBuilder.Engine.State.Resources;
using BaseBuilder.Engine.World.WorldObject.Entities;
using BaseBuilder.Engine.World.Entities.EntityTasks.TransferRestrictors;
using BaseBuilder.Engine.Math2D.Double;
using BaseBuilder.Engine.World.Entities.Utilities;

namespace BaseBuilder.Engine.World.Entities.EntityTasks
{
    public class TransferItemTask : IEntityTask
    {
        Transferer transfer = new Transferer();
        EntityTaskStatus? LastReturn;
        IFindTarget _From;
        int FromID;
        Container From;
        IFindTarget _To;
        int ToID;
        Entity To;
        List<ITransferRestrictors> Restrictors;
        ITransferResultDecider Decider;

        public TransferItemTask(Container from, PointD2D toLoc, List<ITransferRestrictors> restrictors, ITransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new IFindTarget(FromID);
            _To = new IFindTarget(toLoc);
            Restrictors = restrictors;
            Decider = decider;
        }

        public TransferItemTask(Container from, int toID, List<ITransferRestrictors> restrictors, ITransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new IFindTarget(FromID);
            ToID = toID;
            _To = new IFindTarget(toID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public TransferItemTask(Container from, Entity to, List<ITransferRestrictors> restrictors, ITransferResultDecider decider)
        {
            LastReturn = null;
            FromID = from.ID;
            _From = new IFindTarget(FromID);
            ToID = to.ID;
            _To = new IFindTarget(to.ID);
            Restrictors = restrictors;
            Decider = decider;
        }

        public TransferItemTask(NetIncomingMessage message)
        {
            FromID = message.ReadInt32();
            _To = new IFindTarget(message);
            Decider = new ITransferResultDecider(message);
            
        }

        public void Write(NetOutgoingMessage message)
        {
            message.Write(FromID);
            _To.Write(message);
            Decider.Write(message);
            foreach (var restrictor in Restrictors)
                restrictor.Write(message);
        }

        public string PrettyDescription
        {
            get
            {
                return "Transfering Item";
            }
        }

        public double Progress
        {
            get
            {
                return 0;
            }
        }

        public string TaskDescription
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string TaskName
        {
            get
            {
                return "Transfer";
            }
        }

        public string TaskStatus
        {
            get
            {
                return "No status available";
            }
        }

        public void Cancel(SharedGameState gameState)
        {
        }

        public void Reset(SharedGameState gameState)
        {
            LastReturn = null;
        }

        public EntityTaskStatus SimulateTimePassing(SharedGameState gameState, int timeMS)
        {
            if (LastReturn.HasValue)
                return LastReturn.Value;

            if (To == null)
            {
                To = _To.FindTarget(gameState);
                ToID = To.ID;
            }
            if (From == null)
            {
                From = _From.FindTarget(gameState) as Container;
            }

            Dictionary<Material, int> transfers = From.Inventory.GetSimplifiedInvitory();

            foreach (var restrict in Restrictors)
            {
                restrict.Restrict(gameState, From, To, transfers);
                restrict.Reset();
            }

            Dictionary<Material, int> transfered = transfer.Transfer(gameState, From, To, transfers);

            LastReturn = Decider.Decide(transfered);
            if (LastReturn == EntityTaskStatus.Success)
                Decider.Reset();
            return LastReturn.Value;
        }

        public void Update(ContentManager content, SharedGameState sharedGameState, LocalGameState localGameState)
        {
        }
    }
}
